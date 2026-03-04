using System.Net.Http.Headers;

namespace VStartNext.Infrastructure.AI;

public sealed class ModelConnectionTester
{
    private readonly HttpClient _httpClient;

    public ModelConnectionTester(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<ModelConnectionTestResult> TestAsync(
        ModelConnectionTestRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.BaseUrl))
        {
            return new ModelConnectionTestResult(false, null, "BaseUrl is required");
        }

        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            return new ModelConnectionTestResult(false, null, "ApiKey is required");
        }

        try
        {
            var endpoint = new Uri(new Uri(NormalizeBaseUrl(request.BaseUrl)), "models");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.ApiKey);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return new ModelConnectionTestResult(true, (int)response.StatusCode, "Connection succeeded");
            }

            var message = $"Provider returned {(int)response.StatusCode}";
            return new ModelConnectionTestResult(false, (int)response.StatusCode, message);
        }
        catch (Exception ex)
        {
            return new ModelConnectionTestResult(false, null, ex.Message);
        }
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        var value = baseUrl.Trim();
        return value.EndsWith("/", StringComparison.Ordinal) ? value : $"{value}/";
    }
}
