using FluentAssertions;

namespace OpcodeEngine.Tests;

public partial class EngineTests
{
    [Fact]
    public void Test_HelloWorld()
    {
        var engine = Utils.Test(out var output, "HelloWorld.ops");
        output.Should().Be("HELLO WORLD");
    }

    [Fact]
    public void Test_Concat()
    {
        var engine = Utils.Test(out var output, "Concat.ops");
        output.Should().Be("Hello, your gold is: 10");
    }

    [Fact]
    public void Test_Math()
    {
        var engine = Utils.Test(out var output, "Math.ops");
        output.Should().Be("15\n12\n48\n9");
    }

    [Fact]
    public void Test_Params()
    {
        var engine = Utils.Test(out var output, "Params.ops", "Params2.ops");
        output.Should().Be("THE SUM IS: 12");
    }
}