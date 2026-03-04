using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VStartNext.Core.Agent;
using VStartNext.Core.Config;
using VStartNext.Infrastructure.Security;
using VStartNext.Infrastructure.Storage;

namespace VStartNext.Infrastructure.AI;

public sealed class OpenAiCompatibleAgentModelRouter : IAgentModelRouter
{
    private readonly AppConfigFileStore _configStore;
    private readonly ISecretProtector _secretProtector;
    private readonly HttpClient _httpClient;

    public OpenAiCompatibleAgentModelRouter(
        AppConfigFileStore configStore,
        ISecretProtector secretProtector,
        HttpClient? httpClient = null)
    {
        _configStore = configStore;
        _secretProtector = secretProtector;
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<string> CompleteAsync(string prompt)
    {
        var config = _configStore.Load().ModelSettings;
        if (string.IsNullOrWhiteSpace(config.BaseUrl))
        {
            throw new InvalidOperationException("Model BaseUrl is not configured.");
        }

        if (string.IsNullOrWhiteSpace(config.EncryptedApiKey))
        {
            throw new InvalidOperationException("Model API key is not configured.");
        }

        var candidateModels = ResolveCandidateModels(config.Route);
        if (candidateModels.Count == 0)
        {
            throw new InvalidOperationException("No model route is configured.");
        }

        var apiKey = _secretProtector.Unprotect(config.EncryptedApiKey);
        ModelProviderException? lastFailure = null;
        foreach (var model in candidateModels)
        {
            try
            {
                return await CompleteWithModelAsync(config, apiKey, prompt, model);
            }
            catch (ModelProviderException ex) when (ex.CanTryNextModel)
            {
                lastFailure = ex;
                continue;
            }
        }

        throw new InvalidOperationException(lastFailure?.Message ?? "Model request failed.");
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        var value = baseUrl.Trim();
        return value.EndsWith("/", StringComparison.Ordinal) ? value : $"{value}/";
    }

    private async Task<string> CompleteWithModelAsync(
        AiModelSettings config,
        string apiKey,
        string prompt,
        string model)
    {
        var retries = Math.Max(0, config.RetryCount);
        var endpoint = new Uri(new Uri(NormalizeBaseUrl(config.BaseUrl)), "chat/completions");

        for (var attempt = 0; attempt <= retries; attempt++)
        {
            try
            {
                using var request = BuildRequest(endpoint, apiKey, prompt, config, model);
                using var cts = BuildTimeoutCts(config.TimeoutSeconds);
                using var response = await _httpClient.SendAsync(request, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    return ParseMessage(body);
                }

                var statusCode = (int)response.StatusCode;
                if (IsRetryableStatusCode(response.StatusCode) && attempt < retries)
                {
                    continue;
                }

                var canTryNextModel = response.StatusCode != System.Net.HttpStatusCode.Unauthorized &&
                    response.StatusCode != System.Net.HttpStatusCode.Forbidden;
                throw new ModelProviderException($"Model provider returned {statusCode}", canTryNextModel);
            }
            catch (TaskCanceledException) when (attempt < retries)
            {
                continue;
            }
            catch (HttpRequestException) when (attempt < retries)
            {
                continue;
            }
            catch (TaskCanceledException ex)
            {
                throw new ModelProviderException($"Model request timed out for model '{model}'", true, ex);
            }
            catch (HttpRequestException ex)
            {
                throw new ModelProviderException($"Model request failed for model '{model}': {ex.Message}", true, ex);
            }
        }

        throw new ModelProviderException($"Model request failed for model '{model}'", true);
    }

    private static IReadOnlyList<string> ResolveCandidateModels(AiModelRouteSettings route)
    {
        var result = new List<string>();

        AddIfPresent(route.PlannerModel);
        AddIfPresent(route.ChatModel);
        AddIfPresent(route.ReflectionModel);
        return result;

        void AddIfPresent(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var model = value.Trim();
            if (result.Any(existing => string.Equals(existing, model, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            result.Add(model);
        }
    }

    private static HttpRequestMessage BuildRequest(
        Uri endpoint,
        string apiKey,
        string prompt,
        AiModelSettings config,
        string model)
    {
        var payload = new
        {
            model,
            temperature = config.Temperature,
            max_tokens = config.MaxTokens,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");
        return request;
    }

    private static CancellationTokenSource BuildTimeoutCts(int timeoutSeconds)
    {
        return timeoutSeconds > 0
            ? new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds))
            : new CancellationTokenSource();
    }

    private static bool IsRetryableStatusCode(System.Net.HttpStatusCode statusCode)
    {
        return statusCode == System.Net.HttpStatusCode.RequestTimeout ||
            statusCode == System.Net.HttpStatusCode.TooManyRequests ||
            statusCode == System.Net.HttpStatusCode.BadGateway ||
            statusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
            statusCode == System.Net.HttpStatusCode.GatewayTimeout ||
            (int)statusCode >= 500;
    }

    private static string ParseMessage(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var choices = doc.RootElement.GetProperty("choices");
        if (choices.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        var first = choices[0];
        var message = first.GetProperty("message");
        if (!message.TryGetProperty("content", out var contentElement))
        {
            return string.Empty;
        }

        return contentElement.GetString() ?? string.Empty;
    }

    private sealed class ModelProviderException : Exception
    {
        public ModelProviderException(string message, bool canTryNextModel, Exception? innerException = null)
            : base(message, innerException)
        {
            CanTryNextModel = canTryNextModel;
        }

        public bool CanTryNextModel { get; }
    }
}
