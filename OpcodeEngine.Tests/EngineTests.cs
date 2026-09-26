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

    [Fact]
    public void Test_Concat()
    {
        var engine = Utils.Test("Concat.ops", out var output);
        output.Should().Be("Hello, your gold is: 10");
    }
}