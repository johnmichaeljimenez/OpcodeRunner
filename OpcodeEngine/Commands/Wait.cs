namespace OpcodeEngine.Commands
{
	public class Wait : Command
	{
		private float delay = 0;
		private float t = 0;

		public override void OnInit(params string[] args)
		{
			float.TryParse(args[0], out delay);
		}

		public override void OnEnter()
		{
			t = delay;
		}

		public override bool OnTick(float deltaTime)
		{
			t -= deltaTime;
			if (t > 0)
				return false;

			return true;
		}
	}
}