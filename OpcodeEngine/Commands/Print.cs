using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public class Print : Command
	{
		[CommandParameter]
		private string msg = "<empty>";

		public override void OnEnter()
		{
			Console.WriteLine($"[{Engine.CurrentTick}] {msg}");
		}
	}
}