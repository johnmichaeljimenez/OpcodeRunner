using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public class Wait : Command
	{
		
		[CommandParameter]
		private float delay = 0.1f;
		private float t = 0;

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