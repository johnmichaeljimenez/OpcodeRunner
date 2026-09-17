namespace OpcodeEngine.Commands
{
	public class Print : Command
	{
		private string msg = "";

		public override void OnInit(params string[] args)
		{
			msg = args[0];
		}

		public override void OnEnter()
		{
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.f}] {msg}");
		}
	}
}