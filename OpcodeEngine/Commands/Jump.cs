using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{

	public class Jump : Command
	{
		[CommandParameter]
		protected string labelName = "";
		
		[CommandParameter]
		private string key = "";

		private bool hasKey;
		protected bool inverted;

		public override void OnInit()
		{
			hasKey = !string.IsNullOrEmpty(key);
			if (!hasKey)
				return;

			if (key.StartsWith("!"))
			{
				key = key.Substring(1);
				inverted = true;
			}
		}

		public override void OnEnter()
		{
			bool shouldJump = !hasKey || (inverted ? !Condition : Condition);

			if (!shouldJump)
				return;

			Instruction.Jump(labelName);
		}

		protected virtual bool Condition => Engine.SaveKeys.ContainsKey(key) && Engine.SaveKeys[key];
	}
}