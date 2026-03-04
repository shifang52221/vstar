using FluentAssertions;
using VStartNext.Core.Agent;
using VStartNext.Infrastructure.Agent;
using Xunit;

namespace VStartNext.Core.Tests.Agent;

public class AgentAuditFileStoreTests
{
    [Fact]
    public void AppendThenLoadRecent_RoundTripsAuditEntry()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vstartnext-audit-{Guid.NewGuid():N}.json");
        try
        {
            var store = new AgentAuditFileStore(path, maxEntries: 5);
            var entry = new AgentAuditEntry(
                DateTimeOffset.Parse("2026-03-04T12:00:00+08:00"),
                "open chrome",
                AgentExecutionMode.ExecuteAll,
                true,
                "Completed",
                ["launch_app(chrome)"]);

            store.Append(entry);
            var recent = store.LoadRecent();

            recent.Should().HaveCount(1);
            recent[0].Input.Should().Be("open chrome");
            recent[0].Mode.Should().Be(AgentExecutionMode.ExecuteAll);
            recent[0].Steps.Should().ContainSingle("launch_app(chrome)");
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
    public void Append_WhenExceedingMaxEntries_KeepsMostRecent()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vstartnext-audit-{Guid.NewGuid():N}.json");
        try
        {
            var store = new AgentAuditFileStore(path, maxEntries: 2);
            store.Append(new AgentAuditEntry(
                DateTimeOffset.UtcNow.AddMinutes(-2),
                "old-1",
                AgentExecutionMode.ExecuteAll,
                true,
                "ok",
                []));
            store.Append(new AgentAuditEntry(
                DateTimeOffset.UtcNow.AddMinutes(-1),
                "old-2",
                AgentExecutionMode.ExecuteAll,
                true,
                "ok",
                []));
            store.Append(new AgentAuditEntry(
                DateTimeOffset.UtcNow,
                "newest",
                AgentExecutionMode.ExecuteSingleStep,
                true,
                "ok",
                []));

            var recent = store.LoadRecent();

            recent.Should().HaveCount(2);
            recent.Select(x => x.Input).Should().Equal("old-2", "newest");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
