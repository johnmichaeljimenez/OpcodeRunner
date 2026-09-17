namespace OpcodeEngine.Commands
{

	public class Jump : Command
	{
		private string key = "";
		private bool hasCondition;
		protected string labelName;
		protected bool inverted;

		public override void OnInit(params string[] args)
		{
			hasCondition = args.Length > 1;
			if (!hasCondition)  //label only, so no condition
			{
				labelName = args[0];
				hasCondition = false;
				return;
			}


			ParseKey(args[0]);
			labelName = args[1];
		}

		protected void ParseKey(string argsKey)
		{
			key = argsKey;
			if (key.StartsWith("!"))
			{
				key = key.Substring(1);
				inverted = true;
			}
		}

		public override void OnEnter()
		{
			bool shouldJump = !hasCondition || (inverted ? !Condition : Condition);

			if (!shouldJump)
				return;

			Instruction.Jump(labelName);
		}

		protected virtual bool Condition => Engine.SaveKeys.ContainsKey(key) && Engine.SaveKeys[key];
	}
}