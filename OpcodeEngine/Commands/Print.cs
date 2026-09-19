using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public class Print : Command
	{
		[CommandParameter]
		private string msg = "<empty>";

		public override void OnEnter()
		{
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.f}] {msg}");
		}
	}
}