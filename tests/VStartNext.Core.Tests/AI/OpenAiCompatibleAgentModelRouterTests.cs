using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using VStartNext.Core.Config;
using VStartNext.Infrastructure.AI;
using VStartNext.Infrastructure.Security;
using VStartNext.Infrastructure.Storage;
using Xunit;

namespace VStartNext.Core.Tests.AI;

public class OpenAiCompatibleAgentModelRouterTests
{
    [Fact]
    public async Task CompleteAsync_UsesConfiguredModelAndApiKey()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vstartnext-{Guid.NewGuid():N}.json");
        try
        {
            var store = new AppConfigFileStore(path);
            store.Save(new AppConfig
            {
                SchemaVersion = 1,
                ModelSettings = new AiModelSettings
                {
                    BaseUrl = "https://api.example.com/v1",
                    EncryptedApiKey = "enc:api-key",
                    Route = new AiModelRouteSettings
                    {
                        PlannerModel = "gpt-plan",
                        ChatModel = "gpt-chat",
                        ReflectionModel = "gpt-ref"
                    }
                }
            });

            var handler = new CaptureHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"choices":[{"message":{"content":"ok-result"}}]}""")
            });
            var httpClient = new HttpClient(handler);
            var router = new OpenAiCompatibleAgentModelRouter(store, new FakeProtector(), httpClient);

            var result = await router.CompleteAsync("hello");

            result.Should().Be("ok-result");
            handler.LastRequest.Should().NotBeNull();
            handler.LastRequest!.RequestUri!.ToString().Should().Be("https://api.example.com/v1/chat/completions");
            handler.LastRequest.Headers.Authorization.Should().BeEquivalentTo(new AuthenticationHeaderValue("Bearer", "api-key"));
            handler.LastBody.Should().Contain("\"model\":\"gpt-plan\"");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task CompleteAsync_WhenPlannerModelMissing_FallsBackToChatModel()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vstartnext-{Guid.NewGuid():N}.json");
        try
        {
            var store = new AppConfigFileStore(path);
            store.Save(new AppConfig
            {
                SchemaVersion = 1,
                ModelSettings = new AiModelSettings
                {
                    BaseUrl = "https://api.example.com/v1",
                    EncryptedApiKey = "enc:api-key",
                    Route = new AiModelRouteSettings
                    {
                        PlannerModel = string.Empty,
                        ChatModel = "gpt-chat",
                        ReflectionModel = "gpt-ref"
                    }
                }
            });

            var handler = new CaptureHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"choices":[{"message":{"content":"chat-result"}}]}""")
            });
            var router = new OpenAiCompatibleAgentModelRouter(store, new FakeProtector(), new HttpClient(handler));

            var result = await router.CompleteAsync("hello");

            result.Should().Be("chat-result");
            handler.LastBody.Should().Contain("\"model\":\"gpt-chat\"");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task CompleteAsync_WhenPlannerModelReturns404_TriesNextModel()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vstartnext-{Guid.NewGuid():N}.json");
        try
        {
            var store = new AppConfigFileStore(path);
            store.Save(new AppConfig
            {
                SchemaVersion = 1,
                ModelSettings = new AiModelSettings
                {
                    BaseUrl = "https://api.example.com/v1",
                    EncryptedApiKey = "enc:api-key",
                    Route = new AiModelRouteSettings
                    {
                        PlannerModel = "gpt-plan",
                        ChatModel = "gpt-chat",
                        ReflectionModel = "gpt-ref"
                    }
                }
            });

            var responses = new Queue<HttpResponseMessage>(new[]
            {
                new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("""{"error":"model_not_found"}""")
                },
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"choices":[{"message":{"content":"fallback-result"}}]}""")
                }
            });
            var handler = new CaptureHandler(_ => responses.Dequeue());
            var router = new OpenAiCompatibleAgentModelRouter(store, new FakeProtector(), new HttpClient(handler));

            var result = await router.CompleteAsync("hello");

            result.Should().Be("fallback-result");
            handler.RequestBodies.Should().HaveCount(2);
            handler.RequestBodies[0].Should().Contain("\"model\":\"gpt-plan\"");
            handler.RequestBodies[1].Should().Contain("\"model\":\"gpt-chat\"");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task CompleteAsync_WhenAllModelsFail_ThrowsLastModelProviderError()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vstartnext-{Guid.NewGuid():N}.json");
        try
        {
            var store = new AppConfigFileStore(path);
            store.Save(new AppConfig
            {
                SchemaVersion = 1,
                ModelSettings = new AiModelSettings
                {
                    BaseUrl = "https://api.example.com/v1",
                    EncryptedApiKey = "enc:api-key",
                    RetryCount = 0,
                    Route = new AiModelRouteSettings
                    {
                        PlannerModel = "gpt-plan",
                        ChatModel = "gpt-chat",
                        ReflectionModel = "gpt-ref"
                    }
                }
            });

            var handler = new CaptureHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"error":"invalid_model"}""")
            });
            var router = new OpenAiCompatibleAgentModelRouter(store, new FakeProtector(), new HttpClient(handler));

            Func<Task> act = () => router.CompleteAsync("hello");

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*400*");
            handler.RequestBodies.Should().HaveCount(3);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task StreamCompletionAsync_ParsesSseTokensInOrder()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vstartnext-{Guid.NewGuid():N}.json");
        try
        {
            var store = new AppConfigFileStore(path);
            store.Save(new AppConfig
            {
                SchemaVersion = 1,
                ModelSettings = new AiModelSettings
                {
                    BaseUrl = "https://api.example.com/v1",
                    EncryptedApiKey = "enc:api-key",
                    Route = new AiModelRouteSettings
                    {
                        PlannerModel = "gpt-plan",
                        ChatModel = "gpt-chat",
                        ReflectionModel = "gpt-ref"
                    }
                }
            });

            var sseBody = """
                data: {"choices":[{"delta":{"content":"Hel"}}]}

                data: {"choices":[{"delta":{"content":"lo"}}]}

                data: [DONE]
                """;
            var handler = new CaptureHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(sseBody)
            });
            var router = new OpenAiCompatibleAgentModelRouter(store, new FakeProtector(), new HttpClient(handler));
            var tokens = new List<string>();

            await foreach (var token in router.StreamCompletionAsync("hello"))
            {
                tokens.Add(token);
            }

            tokens.Should().Equal("Hel", "lo");
            handler.LastBody.Should().Contain("\"stream\":true");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private sealed class FakeProtector : ISecretProtector
    {
        public string Protect(string plaintext) => $"enc:{plaintext}";

        public string Unprotect(string ciphertext) => ciphertext.Replace("enc:", string.Empty, StringComparison.Ordinal);
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public CaptureHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        public HttpRequestMessage? LastRequest { get; private set; }
        public string LastBody { get; private set; } = string.Empty;
        public List<string> RequestBodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastBody = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            RequestBodies.Add(LastBody);
            return _responder(request);
        }
    }
}
