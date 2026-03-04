using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VStartNext.Core.Agent;
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

        var apiKey = _secretProtector.Unprotect(config.EncryptedApiKey);
        var endpoint = new Uri(new Uri(NormalizeBaseUrl(config.BaseUrl)), "chat/completions");

        var payload = new
        {
            model = config.Route.PlannerModel,
            temperature = config.Temperature,
            max_tokens = config.MaxTokens,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Model provider returned {(int)response.StatusCode}");
        }

        var body = await response.Content.ReadAsStringAsync();
        return ParseMessage(body);
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        var value = baseUrl.Trim();
        return value.EndsWith("/", StringComparison.Ordinal) ? value : $"{value}/";
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
}
