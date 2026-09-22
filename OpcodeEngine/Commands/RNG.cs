using OpcodeEngine.Core;

namespace OpcodeEngine.Commands;

public class RNG : Command
{
	[CommandParameter]
	private float range;
	[CommandParameter]
	private string labelName;

	public override void OnEnter()
	{
		var value = Engine.RNG.NextSingle();
		if (value <= range)
			Instruction.Jump(labelName);
	}
}

public class RNGRange : Command
{
	[CommandParameter]
	private float min;
	[CommandParameter]
	private float max;
	[CommandParameter]
	private string labelName;

	public override void OnEnter()
	{
		var value = Engine.RNG.NextSingle();
		if (value >= min && value <= max)
			Instruction.Jump(labelName);
	}
}