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
}