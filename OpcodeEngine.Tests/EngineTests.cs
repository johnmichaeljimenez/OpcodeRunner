using FluentAssertions;

namespace OpcodeEngine.Tests;

public partial class EngineTests
{
    [Fact]
    public void Test_HelloWorld()
    {
        var engine = Utils.Test("HelloWorld.ops", out var output);
        output.Should().Be("HELLO WORLD");
    }
}