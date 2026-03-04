using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using VStartNext.Core.Agent;

namespace VStartNext.Infrastructure.AI;

public sealed class OpenAiCompatibleAgentPlanner : IAgentPlanner
{
    private readonly IAgentModelRouter _modelRouter;

    public OpenAiCompatibleAgentPlanner(IAgentModelRouter modelRouter)
    {
        _modelRouter = modelRouter;
    }

    public async Task<AgentActionPlan> PlanAsync(
        AgentPlannerRequest request,
        IProgress<string>? planningProgress = null,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(request);
        string completion;
        if (planningProgress is null)
        {
            completion = await _modelRouter.CompleteAsync(prompt);
        }
        else
        {
            completion = await CompleteWithStreamingAsync(
                prompt,
                planningProgress,
                cancellationToken);
        }

        var plan = TryParsePlan(completion, request);
        return plan ?? BuildFallbackPlan(request);
    }

    private async Task<string> CompleteWithStreamingAsync(
        string prompt,
        IProgress<string> planningProgress,
        CancellationToken cancellationToken)
    {
        try
        {
            var completionBuilder = new StringBuilder();
            await foreach (var token in _modelRouter.StreamCompletionAsync(prompt, cancellationToken)
                               .WithCancellation(cancellationToken))
            {
                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                planningProgress.Report(token);
                completionBuilder.Append(token);
            }

            if (completionBuilder.Length > 0)
            {
                return completionBuilder.ToString();
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // fallback to non-stream completion below
        }

        return await _modelRouter.CompleteAsync(prompt);
    }

    private static string BuildPrompt(AgentPlannerRequest request)
    {
        var tools = request.AvailableTools.Count == 0
            ? "launch_app, open_url, open_path, quick_action"
            : string.Join(", ", request.AvailableTools);
        var languageMode = ResolvePlanningLanguageMode(request);
        var languageRule = languageMode switch
        {
            "zh-CN" => "Language rule: Use Chinese wording for any human-readable arguments when needed.",
            "en-US" => "Language rule: Use English wording for any human-readable arguments when needed.",
            _ => "Language rule: Keep bilingual-compatible wording for any human-readable arguments when needed."
        };

        return string.Join(
            '\n',
            [
                "You are a desktop launcher planning assistant.",
                "Output JSON only. Do not include markdown fences.",
                $"Language mode: {languageMode}",
                "Schema:",
                "{",
                "  \"intent\": \"Automation|Search\",",
                "  \"steps\": [",
                "    {",
                "      \"toolName\": \"launch_app|open_url|open_path|quick_action\",",
                "      \"arguments\": \"string\",",
                "      \"riskLevel\": \"Low|Medium|High\"",
                "    }",
                "  ]",
                "}",
                "Rules:",
                "- Use available tools only.",
                "- Keep toolName machine-readable and unchanged.",
                "- riskLevel must be one of Low, Medium, High.",
                "- quick_action arguments must be one of shutdown, restart, lock, open_settings.",
                languageRule,
                $"Available tools: {tools}",
                $"User input: {request.Input}"
            ]);
    }

    private static string ResolvePlanningLanguageMode(AgentPlannerRequest request)
    {
        return request.Language switch
        {
            AgentLanguage.Chinese => "zh-CN",
            AgentLanguage.English => "en-US",
            AgentLanguage.Mixed => InferLanguageModeFromInput(request.Input),
            _ => "bilingual"
        };
    }

    private static string InferLanguageModeFromInput(string input)
    {
        var hasChinese = false;
        var hasEnglish = false;

        foreach (var ch in input)
        {
            if (ch >= '\u4e00' && ch <= '\u9fff')
            {
                hasChinese = true;
                continue;
            }

            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
            {
                hasEnglish = true;
            }
        }

        if (hasChinese && hasEnglish)
        {
            return "bilingual";
        }

        if (hasChinese)
        {
            return "zh-CN";
        }

        if (hasEnglish)
        {
            return "en-US";
        }

        return "bilingual";
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
        var canQuickAction = request.AvailableTools.Count == 0 ||
            request.AvailableTools.Contains("quick_action", StringComparer.OrdinalIgnoreCase);

        if (canQuickAction && TryExtractQuickAction(input, out var action, out var riskLevel))
        {
            steps.Add(new AgentPlanStep("quick_action", action, riskLevel));
        }
        else if (canOpenUrl && TryExtractUrl(input, out var url))
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

    private static bool TryExtractQuickAction(
        string input,
        out string action,
        out AgentRiskLevel riskLevel)
    {
        var normalized = input.Trim().ToLowerInvariant();

        if (ContainsAny(normalized, "shutdown", "power off", "turn off", "关机"))
        {
            action = "shutdown";
            riskLevel = AgentRiskLevel.High;
            return true;
        }

        if (ContainsAny(normalized, "restart", "reboot", "重启"))
        {
            action = "restart";
            riskLevel = AgentRiskLevel.High;
            return true;
        }

        if (ContainsAny(normalized, "lock", "lock screen", "锁屏"))
        {
            action = "lock";
            riskLevel = AgentRiskLevel.Medium;
            return true;
        }

        if (ContainsAny(normalized, "settings", "open settings", "设置"))
        {
            action = "open_settings";
            riskLevel = AgentRiskLevel.Low;
            return true;
        }

        action = string.Empty;
        riskLevel = AgentRiskLevel.Low;
        return false;
    }

    private static bool ContainsAny(string input, params string[] candidates)
    {
        return candidates.Any(candidate =>
            input.Contains(candidate, StringComparison.OrdinalIgnoreCase));
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
