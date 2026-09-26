using FluentAssertions;

namespace OpcodeEngine.Tests;

public partial class EngineTests
{
    [Fact]
    public void Test_Misnky()
    {
        var engine = Utils.Test("special\\Minsky.ops", out var output);
        output.Should().Contain("ENDED:\n12");
    }

    [Fact]
    public void Test_Countdown()
    {
        var engine = Utils.Test("special\\Countdown.ops", out var output);
        output.Should().Be("INITIALIZED PROGRAM\nBEGIN COUNTDOWN\n10\n9\n8\n7\n6\n5\n4\n3\n2\n1\nDONE!");
    }
}