using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public class Save : Command
	{
		[CommandParameter]
		private string key = "";
		
		[CommandParameter]
		private bool value;

		public override void OnEnter()
		{
			Engine.SaveKeys[key] = value;
		}
	}
}