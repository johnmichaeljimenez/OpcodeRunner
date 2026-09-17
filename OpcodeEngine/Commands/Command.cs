using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public abstract class Command
	{
		protected Engine Engine { get; private set; }
		protected Instruction Instruction { get; private set; }
		internal void Initialize(Engine engine, Instruction instruction)
		{
			Engine = engine;
			Instruction = instruction;
		}

		public virtual void OnInit(params string[] args)
		{

		}

		public virtual void OnEnter()
		{

		}

		public virtual void OnExit()
		{

		}

		public virtual bool OnTick(float deltaTime) => true;
	}
}