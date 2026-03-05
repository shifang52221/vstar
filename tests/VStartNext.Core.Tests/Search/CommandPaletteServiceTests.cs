using FluentAssertions;
using VStartNext.Core.Search;
using Xunit;

namespace VStartNext.Core.Tests.Search;

public class CommandPaletteServiceTests
{
    [Fact]
    public async Task Execute_Calc_ReturnsComputedText()
    {
        var service = new CommandPaletteService(new FakeExecutor());

        var result = await service.ExecuteAsync("calc: 2*36");

        result.DisplayText.Should().Be("72");
    }

    [Fact]
    public async Task Execute_Url_DispatchesExactUrl()
    {
        var fake = new FakeExecutor();
        var service = new CommandPaletteService(fake);

        await service.ExecuteAsync("url: https://example.com");

        fake.LastTarget.Should().Be("https://example.com");
    }

    [Fact]
    public async Task Execute_Ws_AddsHttps_WhenSchemeMissing()
    {
        var fake = new FakeExecutor();
        var service = new CommandPaletteService(fake);

        await service.ExecuteAsync("ws: openai.com");

        fake.LastTarget.Should().Be("https://openai.com");
    }

    [Fact]
    public async Task Execute_Run_DispatchesLocalTarget()
    {
        var fake = new FakeExecutor();
        var service = new CommandPaletteService(fake);

        await service.ExecuteAsync(@"run: C:\Tools\Chrome.lnk");

        fake.LastTarget.Should().Be(@"C:\Tools\Chrome.lnk");
    }

    private sealed class FakeExecutor : ICommandActionExecutor
    {
        public string? LastTarget { get; private set; }

        public Task ExecuteOpenTargetAsync(string target)
        {
            LastTarget = target;
            return Task.CompletedTask;
        }
    }
}
