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
}
