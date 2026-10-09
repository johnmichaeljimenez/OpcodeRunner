using YesodScript.Engine.Core;

namespace YesodScript.Engine.Commands
{
	public class Start : Command
	{
		[CommandParameter]
		private string scriptId = "";

		public override void OnEnter()
		{
			VM.Run(scriptId);
		}
	}
}