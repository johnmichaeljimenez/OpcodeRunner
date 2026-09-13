using System.Reflection;

namespace OpcodeEngine;

public class Engine
{
	public readonly Dictionary<string, bool> SaveKeys = new();
	private Dictionary<string, Type> commandTypes;

	private readonly List<Instruction> runningCommands = new();
	private readonly List<Instruction> _toRemove = new();

	public Engine()
	{
		InitRegistry();
	}

	public void Run(Instruction instruction)
	{
		instruction.IsPaused = false;
		instruction.SetIndex(0);

		runningCommands.Add(instruction);
	}

	public void Tick(float deltaTime)
	{
		if (deltaTime <= 0)
			return;

		foreach (var i in runningCommands)
		{
			if (i.IsPaused)
				continue;

			var line = i.Commands[i.CurrentIndex];
			var currentIndex = i.CurrentIndex;
			if (!line.OnTick(deltaTime))
				continue;

			line.OnExit();

			if (currentIndex != i.CurrentIndex) //someone modified index internally, ex. jump
				continue;

			i.SetIndex(i.CurrentIndex + 1);
			if (i.CurrentIndex >= i.Commands.Count)
			{
				_toRemove.Add(i);
			}
		}

		for (int i = _toRemove.Count - 1; i >= 0; i--)
		{
			var ins = _toRemove[i];
			runningCommands.Remove(ins);
			_toRemove.RemoveAt(i);
		}
	}

	private void InitRegistry()
	{
		commandTypes = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(GetLoadableTypes)
			.Where(t => t.IsClass && !t.IsAbstract && typeof(Command).IsAssignableFrom(t) && t != typeof(Command))
			.ToDictionary(
				t => t.Name.ToUpper(),
				t => t,
				StringComparer.OrdinalIgnoreCase
			);
	}

	private IEnumerable<Type> GetLoadableTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException e)
		{
			return e.Types.Where(t => t != null);
		}
	}

	public Instruction Create(string id, string code)
	{
		if (string.IsNullOrEmpty(code))
			return null;

		var ins = new Instruction()
		{
			ID = id
		};

		var n = 0;
		foreach (var i in code.Trim().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
		{
			if (string.IsNullOrEmpty(i))
				continue;

			var line = i.Trim();
			if (line.StartsWith("#"))
				continue;

			if (line.StartsWith(">"))
			{
				ins.Labels.Add(line.Substring(1).Trim(), n);
				continue;
			}

			var parts = line.SplitArguments();
			var type = commandTypes[parts[0].ToUpper()];

			var command = Activator.CreateInstance(type) as Command;
			command.Initialize(this, ins);
			command.OnInit(parts.Skip(1).ToArray());

			ins.Commands.Add(command);
			n++;
		}

		return ins;
	}
}
