using FluentAssertions;
using OpcodeEngine.Core;

namespace OpcodeEngine.Tests;

public partial class EngineTests
{
    [Fact]
    public void Test_Dump()
    {
        var commands = new string[] { "WAIT", "MUL", "JUMPIF" };
        var engineOutput = "";
        var engine = new Engine(immediateMode: true);

        engine.OnOutput += (str) => { engineOutput += $"{str}"; };
        engine.DumpCommands(commands);
        Console.WriteLine(engineOutput);

        engineOutput.Should().ContainAll(commands);
    }

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
        output.Should().Be("My gold is: 30\nI will pay 8\nMy gold is: 22");
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
        var engine = Utils.Test(out var output, "Params.ops", "Params2.ops", "Params3.ops");
        output.Should().Be("CALLED PARAMS 3\nTHE SUM IS: 12");
    }

    [Fact]
    public void Test_Immediate()
    {
        var engine = Utils.Test(out var output, true, "Immediate.ops", "HelloWorld.ops", "Math.ops");
        var tickCount = engine.CurrentTick;
        tickCount.Should().Be(1);
        
        engine = Utils.Test(out output, false, "Immediate.ops", "HelloWorld.ops", "Math.ops");
        tickCount = engine.CurrentTick;
        tickCount.Should().BeGreaterThanOrEqualTo(10);
    }
}