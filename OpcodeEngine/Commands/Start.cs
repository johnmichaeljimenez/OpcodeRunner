using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public class Start : Command
	{
		[CommandParameter]
		private string scriptId = "";

		public override void OnEnter()
		{
			Engine.Run(scriptId);
		}
	}
}