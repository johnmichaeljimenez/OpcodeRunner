using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public class Jump : Command
	{
		[CommandParameter]
		private string labelName = "";

		public override void OnEnter()
		{
			Instruction.Jump(labelName);
		}
	}

	public class JumpIf : Command
	{
		[CommandParameter]
		private string key = "";
		[CommandParameter]
		private string labelName = "";

		public override void OnEnter()
		{
			var baseKey = key;
			var invert = Utils.HasPrefix("!", ref baseKey);
			var var = Engine.GetVar(baseKey);
			Utils.TryParseBool(var.Value.ToString(), out var boolValue);

			if (invert)
				boolValue = !boolValue;

			if (boolValue)
				Instruction.Jump(labelName);
		}
	}
}