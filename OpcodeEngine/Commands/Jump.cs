namespace OpcodeEngine.Commands
{

	public class Jump : Command
	{
		private string key = "";
		protected string labelName;
		protected bool inverted;

		public override void OnInit(params string[] args)
		{
			key = args[0];
			ParseLabel(args);
		}

		protected void ParseLabel(params string[] args)
		{
			labelName = args[1];
			if (labelName.StartsWith("!"))
			{
				labelName = labelName.Substring(1);
				inverted = true;
			}
		}

		public override void OnEnter()
		{
			bool shouldJump = inverted ? !Condition : Condition;

			if (!shouldJump)
				return;

			Instruction.Jump(labelName);
		}

		protected virtual bool Condition => Engine.SaveKeys.ContainsKey(key) && Engine.SaveKeys[key];
	}
}