using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public class Break : Command
	{
		public override void OnEnter()
		{
			Instruction.IsRunning = false;
		}
	}
}