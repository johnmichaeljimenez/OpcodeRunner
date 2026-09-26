using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public class Call : Command
	{
		[CommandParameter]
		private string id;

		public override void OnEnter()
		{
			Engine.FireTrigger(id);
		}
	}
}