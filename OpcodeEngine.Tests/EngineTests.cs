using FluentAssertions;
using OpcodeEngine.Commands;
using OpcodeEngine.Core;

namespace OpcodeEngine.Tests;

public partial class EngineTests
{
    [Fact]
    public void Test_Dump()
    {
        var commands = new string[] { "WAIT", "MUL", "JUMPIF" };
        var engineOutput = "";
        var engine = new Engine();

        engine.OnOutput += (str) => { engineOutput += $"{str}"; };
        engine.DumpCommands(commands);
        Console.WriteLine(engineOutput);

        engineOutput.Should().ContainAll(commands);
    }

    [Fact]
    public void Test_Regex()
    {
        var strSplitArg = """CALL "HELLO WORLD HE SAYS "HI"" 10""";
        Core.Utils.SplitArguments(strSplitArg).Should().BeEquivalentTo("CALL", "HELLO WORLD HE SAYS HI", "10");
    }

    [Fact]
    public void Test_HelloWorld()
    {
        var engine = Utils.Test(out var output, "HelloWorld.ops");
        output.Should().Be("HELLO WORLD");
    }

    [Fact]
    public void Test_Trigger()
    {
        var engine = Utils.Test(out var output, "Trigger1.ops", "Trigger2.ops");
        output.Should().Be("TRIGGER TEST\nHELLO I AM FROM TRIGGER2, MY NUMBER IS 120\nMY RESPONSE IS: GOODBYE\nFOLLOWUP: WHY?\nTrue");
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
    public void Test_ExecOrder()
    {
        var engine = Utils.Test(out var output, true, "ExecutionOrder.ops", "HelloWorld.ops", "Math.ops");
        var tickCount = engine.CurrentTick;
        tickCount.Should().Be(1);
    }

    [Fact]
    public void Test_RNG()
    {
        var engine = Utils.Test(out var output, true);
        engine.RNG.Should().NotBeNull();

        engine.RandomSeed = 12;
        engine.RandomSeed.Should().Be(12);
    }

    [Fact]
    public void Test_Lists()
    {
        var engine = Utils.Test(out var output, "Lists.ops");
        output.Should().Be("apple\nbanana\ncherry\n5\n10\n25\nSUM: 40");
    }

    [Fact]
    public void Test_DefaultArg()
    {
        var engine = Utils.Test(out var output, "DefaultArg.ops");
        output.Should().Be("<empty>\n*\n5\n9");
    }

    [Fact]
    public void Test_TriggerSequence()
    {
        var engine = new Engine();
        engine.CompileFile("scripts/Sequence.ops");
        engine.Initialize();

        var output = "";
        engine.OnOutput += str => output += $"{str}";

        var seq = new TriggerSequence(engine);
        seq.Enqueue("SECOND");
        seq.Enqueue("FIRST");
        seq.Enqueue("   ");//should be ignored
        seq.Enqueue(null);//should be ignored

        while (true)
        {
            seq.Tick();
            engine.Tick(0.033333f);
            if (seq.Tick() && !engine.IsRunning)
                break;
        }

        output.Trim().Should().Be("B1\nA1\nA2");
    }

    [Fact]
    public void Test_TriggerSequence_TickContract()
    {
        var engine = new Engine();
        engine.CompileFile("scripts/Sequence.ops");
        engine.Initialize();

        var output = "";
        engine.OnOutput += str => output += $"{str}";

        var seq = new TriggerSequence(engine);
        seq.Tick().Should().BeTrue();//empty queue -> done immediately

        seq.Enqueue("FIRST");
        seq.Tick().Should().BeFalse();//something fired and is running

        while (!seq.Tick())
            engine.Tick(0.033333f);

        output.Should().Contain("A1\nA2");
        seq.Tick().Should().BeTrue();//drained
    }
}
