using System.Text.Json;
using System.Text.RegularExpressions;
using VStartNext.Core.Agent;

namespace VStartNext.Infrastructure.AI;

public sealed class OpenAiCompatibleAgentPlanner : IAgentPlanner
{
    private readonly IAgentModelRouter _modelRouter;

    public OpenAiCompatibleAgentPlanner(IAgentModelRouter modelRouter)
    {
        _modelRouter = modelRouter;
    }

    public async Task<AgentActionPlan> PlanAsync(AgentPlannerRequest request)
    {
        var prompt = BuildPrompt(request);
        var completion = await _modelRouter.CompleteAsync(prompt);

        var plan = TryParsePlan(completion, request);
        return plan ?? BuildFallbackPlan(request);
    }

    private static string BuildPrompt(AgentPlannerRequest request)
    {
        var tools = request.AvailableTools.Count == 0
            ? "launch_app, open_url, open_path"
            : string.Join(", ", request.AvailableTools);
        return
            "You are a desktop launcher planner.\n" +
            "Convert user input into strict JSON only.\n" +
            "Schema:\n" +
            "{\n" +
            "  \"intent\": \"Automation|Search\",\n" +
            "  \"steps\": [\n" +
            "    {\n" +
            "      \"toolName\": \"launch_app|open_url|open_path\",\n" +
            "      \"arguments\": \"string\",\n" +
            "      \"riskLevel\": \"Low|Medium|High\"\n" +
            "    }\n" +
            "  ]\n" +
            "}\n" +
            $"Available tools: {tools}\n" +
            $"User input: {request.Input}";
    }

    private static AgentActionPlan? TryParsePlan(string completion, AgentPlannerRequest request)
    {
        var jsonText = ExtractJson(completion);
        if (jsonText is null)
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (!root.TryGetProperty("intent", out var intentElement) ||
                intentElement.ValueKind != JsonValueKind.String ||
                !Enum.TryParse<AgentIntent>(intentElement.GetString(), true, out var intent))
            {
                return null;
            }

            if (!root.TryGetProperty("steps", out var stepsElement) || stepsElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var steps = new List<AgentPlanStep>();
            foreach (var item in stepsElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                if (!TryReadRequiredString(item, out var toolName, "toolName", "tool_name"))
                {
                    return null;
                }

                if (!TryReadRequiredString(item, out var arguments, "arguments", "args"))
                {
                    return null;
                }

                if (!TryReadRequiredString(item, out var riskLevelText, "riskLevel", "risk_level") ||
                    !Enum.TryParse<AgentRiskLevel>(riskLevelText, true, out var riskLevel))
                {
                    return null;
                }

                if (request.AvailableTools.Count > 0 &&
                    !request.AvailableTools.Contains(toolName, StringComparer.OrdinalIgnoreCase))
                {
                    return null;
                }

                steps.Add(new AgentPlanStep(toolName.Trim(), arguments.Trim(), riskLevel));
            }

            if (intent == AgentIntent.Automation && steps.Count == 0)
            {
                return null;
            }

            return new AgentActionPlan(intent, request.Input, steps);
        }
        catch
        {
            return null;
        }
    }

    private static AgentActionPlan BuildFallbackPlan(AgentPlannerRequest request)
    {
        var steps = new List<AgentPlanStep>();
        var input = request.Input.Trim();

        var canOpenUrl = request.AvailableTools.Count == 0 ||
            request.AvailableTools.Contains("open_url", StringComparer.OrdinalIgnoreCase);
        var canLaunchApp = request.AvailableTools.Count == 0 ||
            request.AvailableTools.Contains("launch_app", StringComparer.OrdinalIgnoreCase);

        if (canOpenUrl && TryExtractUrl(input, out var url))
        {
            steps.Add(new AgentPlanStep("open_url", url, AgentRiskLevel.Low));
        }
        else if (canLaunchApp && TryExtractAppName(input, out var appName))
        {
            steps.Add(new AgentPlanStep("launch_app", appName, AgentRiskLevel.Low));
        }

        var intent = steps.Count == 0 ? AgentIntent.Search : AgentIntent.Automation;
        return new AgentActionPlan(intent, request.Input, steps);
    }

    private static bool TryExtractUrl(string input, out string url)
    {
        foreach (var token in input.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!Uri.TryCreate(token, UriKind.Absolute, out var uri))
            {
                continue;
            }

            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                url = uri.ToString();
                return true;
            }
        }

        url = string.Empty;
        return false;
    }

    private static bool TryExtractAppName(string input, out string appName)
    {
        var openMatch = Regex.Match(input, @"(?i)\bopen\s+([^\s]+)");
        if (openMatch.Success)
        {
            appName = openMatch.Groups[1].Value.Trim();
            return appName.Length > 0;
        }

        var cjkVerbMatch = Regex.Match(input, @"^\p{IsCJKUnifiedIdeographs}+\s*([^\s]+)");
        if (cjkVerbMatch.Success)
        {
            appName = cjkVerbMatch.Groups[1].Value.Trim();
            return appName.Length > 0;
        }

        appName = string.Empty;
        return false;
    }

    private static string? ExtractJson(string completion)
    {
        var text = completion.Trim();
        if (text.Length == 0)
        {
            return null;
        }

        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineEnd = text.IndexOf('\n');
            var contentStart = firstLineEnd >= 0 ? firstLineEnd + 1 : 3;
            var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence > contentStart)
            {
                text = text[contentStart..lastFence].Trim();
            }
        }

        var firstBrace = text.IndexOf('{');
        var lastBrace = text.LastIndexOf('}');
        if (firstBrace < 0 || lastBrace <= firstBrace)
        {
            return null;
        }

        return text[firstBrace..(lastBrace + 1)];
    }

    private static string? ReadString(JsonElement element, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (element.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }
        }

        return null;
    }

    private static bool TryReadRequiredString(JsonElement element, out string value, params string[] keys)
    {
        value = string.Empty;
        var raw = ReadString(element, keys);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        value = raw.Trim();
        return value.Length > 0;
    }

}
