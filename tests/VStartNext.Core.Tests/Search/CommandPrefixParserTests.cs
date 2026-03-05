using FluentAssertions;
using VStartNext.Core.Search;
using Xunit;

namespace VStartNext.Core.Tests.Search;

public class CommandPrefixParserTests
{
    [Fact]
    public void Parse_CalcPrefix_Detected()
    {
        var parser = new CommandPrefixParser();

        var result = parser.Parse("calc: 2*36");

        result.Type.Should().Be(CommandPrefixType.Calc);
        result.Payload.Should().Be("2*36");
    }

    [Fact]
    public void Parse_RunPrefix_Detected()
    {
        var parser = new CommandPrefixParser();

        var result = parser.Parse(@"run: C:\Tools\Chrome.lnk");

        result.Type.Should().Be(CommandPrefixType.Run);
        result.Payload.Should().Be(@"C:\Tools\Chrome.lnk");
    }
}
