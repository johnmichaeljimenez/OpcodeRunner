using YesodScript.Engine.Core;

namespace YesodScript.Engine.Commands;

public class _Trigger : Command
{
	public string TriggerKey { get; set; }
	public Dictionary<string, string> Parameters { get; set; } = new();

	public override void OnInit() { }
	public override void OnEnter() { }

	public override bool OnTick(float deltaTime)
	{
		return true;
	}
}

public class TriggerSequence
{
	private readonly VirtualMachine vm;
	private readonly Queue<string> queue = new();
	private List<Instruction> active = new();

	public TriggerSequence(VirtualMachine vm)
	{
		this.vm = vm;
	}

	public void Enqueue(string trigger)
	{
		if (!string.IsNullOrWhiteSpace(trigger))
			queue.Enqueue(trigger);
	}

	public bool Tick()
	{
		while (true)
		{
			if (active.Any(p => p.IsRunning))
				return false;

			active.Clear();
			if (queue.Count == 0)
				return true;

			active = vm.FireTrigger(queue.Dequeue());
		}
	}
}