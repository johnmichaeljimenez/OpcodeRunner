namespace OpcodeEngine;

public class Instruction
{
	public string ID { get; set; }

	public int CurrentIndex { get; set; }
	public bool IsPaused { get; set; }
	public bool IsRunning => CurrentIndex < Commands.Count;

	public readonly List<Command> Commands = new();
	public readonly Dictionary<string, int> Labels = new();

	public void SetIndex(int index)
	{
		CurrentIndex = index;

		if (index >= Commands.Count)
			return;

		Commands[CurrentIndex].OnEnter();
	}

	public void Jump(string name)
	{
		var index = Labels[name];
		SetIndex(index);
	}
}

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

public class Wait : Command
{
	private float delay = 0;
	private float t = 0;

	public override void OnInit(params string[] args)
	{
		int.TryParse(args[0], out var millis);
		delay = millis / 1000f;
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

public class Save : Command
{
	private string key = "";
	private bool value;

	public override void OnInit(params string[] args)
	{
		key = args[0];
		Utils.TryParseBool(args[1], out value);
	}

	public override void OnEnter()
	{
		Engine.SaveKeys[key] = value;
	}
}

public class Jump : Command
{
	private string key = "";
	private string labelName;

	public override void OnInit(params string[] args)
	{
		key = args[0];
		labelName = args[1];
	}

	public override void OnEnter()
	{
		if (!Engine.SaveKeys.ContainsKey(key) || !Engine.SaveKeys[key])
			return;

		Instruction.Jump(labelName);
	}
}