using FluentAssertions;

namespace YesodScript.Engine.Tests;

public partial class EngineTests
{
    [Fact]
    public void Test_Minsky()
    {
        var vm = Utils.Test(out var output, "special/Minsky.ops");
        output.Should().Contain("ENDED:\n12");
    }

    [Fact]
    public void Test_Countdown()
    {
        var vm = Utils.Test(out var output, "special/Countdown.ops");
        output.Should().Be("INITIALIZED PROGRAM\nBEGIN COUNTDOWN\n10\n9\n8\n7\n6\n5\n4\n3\n2\n1\nDONE!");
    }

    [Fact]
    public void Test_EnemyAI()
    {
        var vm = Utils.Test(out var output, "special/EnemyGame.ops", "special/EnemyAI.ops");

        output.Should().Be(
            "GAME START\n" +
            "ENEMY IDLE dist=10\n" +
            "ENEMY IDLE dist=5\n" +
            "ENEMY SPOTS PLAYER\n" +
            "ENEMY CHASE dist=2\n" +
            "ENEMY IN RANGE\n" +
            "GAME END");
    }
}