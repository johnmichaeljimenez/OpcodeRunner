using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public class Save : Command
	{
		private string key = "";
		private bool value;

		public override void OnInit(params string[] args)
		{
			key = args[0];
			Utils.TryParseBool(args[1], out value);
		}

		public override void OnEnter()
		{
			Engine.SaveKeys[key] = value;
		}
	}
}