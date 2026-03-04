using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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

    public async IAsyncEnumerable<string> StreamCompletionAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
        string? lastFailureMessage = null;
        foreach (var model in candidateModels)
        {
            var retries = Math.Max(0, config.RetryCount);
            for (var attempt = 0; attempt <= retries; attempt++)
            {
                using var request = BuildRequest(
                    new Uri(new Uri(NormalizeBaseUrl(config.BaseUrl)), "chat/completions"),
                    apiKey,
                    prompt,
                    config,
                    model,
                    stream: true);
                using var timeoutCts = BuildTimeoutCts(config.TimeoutSeconds);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    timeoutCts.Token);

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead,
                        linkedCts.Token);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < retries)
                {
                    continue;
                }
                catch (HttpRequestException) when (attempt < retries)
                {
                    continue;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (OperationCanceledException)
                {
                    lastFailureMessage = $"Model stream timed out for model '{model}'";
                    break;
                }
                catch (HttpRequestException ex)
                {
                    lastFailureMessage = $"Model stream failed for model '{model}': {ex.Message}";
                    break;
                }

                using (response)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var hasTokens = false;
                        await foreach (var token in ParseSseTokensAsync(response, linkedCts.Token)
                                           .WithCancellation(cancellationToken))
                        {
                            hasTokens = true;
                            yield return token;
                        }

                        if (hasTokens)
                        {
                            yield break;
                        }

                        lastFailureMessage = $"Model stream returned empty content for model '{model}'";
                        break;
                    }

                    var statusCode = (int)response.StatusCode;
                    if (IsRetryableStatusCode(response.StatusCode) && attempt < retries)
                    {
                        continue;
                    }

                    var canTryNextModel = response.StatusCode != System.Net.HttpStatusCode.Unauthorized &&
                        response.StatusCode != System.Net.HttpStatusCode.Forbidden;
                    if (!canTryNextModel)
                    {
                        throw new InvalidOperationException($"Model provider returned {statusCode}");
                    }

                    lastFailureMessage = $"Model provider returned {statusCode}";
                    break;
                }
            }
        }

        throw new InvalidOperationException(lastFailureMessage ?? "Model stream request failed.");
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
        string model,
        bool stream = false)
    {
        var payload = new
        {
            model,
            temperature = config.Temperature,
            max_tokens = config.MaxTokens,
            stream,
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

    private static async IAsyncEnumerable<string> ParseSseTokensAsync(
        HttpResponseMessage response,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                yield break;
            }

            if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var data = line[5..].Trim();
            if (data.Length == 0)
            {
                continue;
            }

            if (string.Equals(data, "[DONE]", StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            var token = ParseSseToken(data);
            if (!string.IsNullOrEmpty(token))
            {
                yield return token;
            }
        }
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

    private static string ParseSseToken(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                return string.Empty;
            }

            var first = choices[0];
            if (first.TryGetProperty("delta", out var delta) &&
                delta.TryGetProperty("content", out var contentElement) &&
                contentElement.ValueKind == JsonValueKind.String)
            {
                return contentElement.GetString() ?? string.Empty;
            }

            if (first.TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out contentElement) &&
                contentElement.ValueKind == JsonValueKind.String)
            {
                return contentElement.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
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
