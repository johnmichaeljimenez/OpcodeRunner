using YesodScript.Engine.Core;

namespace YesodScript.Engine.Commands
{
	public class Break : Command
	{
		public override void OnEnter()
		{
			Instruction.IsRunning = false;
		}
	}
}