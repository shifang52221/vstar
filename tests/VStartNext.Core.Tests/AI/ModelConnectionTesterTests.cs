using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using VStartNext.Infrastructure.AI;
using Xunit;

namespace VStartNext.Core.Tests.AI;

public class ModelConnectionTesterTests
{
    [Fact]
    public async Task TestAsync_WhenProviderResponds200_ReturnsSuccess()
    {
        var handler = new CaptureHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var tester = new ModelConnectionTester(new HttpClient(handler));
        var request = new ModelConnectionTestRequest("https://api.example.com/v1", "sk-test");

        var result = await tester.TestAsync(request);

        result.Success.Should().BeTrue();
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.RequestUri!.ToString().Should().Be("https://api.example.com/v1/models");
        handler.LastRequest.Headers.Authorization.Should().BeEquivalentTo(new AuthenticationHeaderValue("Bearer", "sk-test"));
    }

    [Fact]
    public async Task TestAsync_WhenProviderResponds401_ReturnsFailureStatus()
    {
        var handler = new CaptureHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var tester = new ModelConnectionTester(new HttpClient(handler));
        var request = new ModelConnectionTestRequest("https://api.example.com/v1", "bad-key");

        var result = await tester.TestAsync(request);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public CaptureHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_responder(request));
        }
    }
}
