using System.Reflection;

namespace OpcodeEngine;

public class Engine
{
	public readonly Dictionary<string, bool> SaveKeys = new();
	private Dictionary<string, Type> commandTypes;

	private readonly List<Instruction> allCommands = new();
	private readonly List<Instruction> runningCommands = new();
	private readonly List<Instruction> _toRemove = new();

	public Engine()
	{
		InitRegistry();
	}

	public void FireTrigger(string triggerName)
	{
		foreach (var i in allCommands)
		{
			if (i.TriggerKey != triggerName.ToUpper())
				continue;

			Run(i);
		}
	}

	public void Run(Instruction instruction)
	{
		instruction.IsPaused = false;
		instruction.SetIndex(0);

		if (!runningCommands.Contains(instruction))
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

	public Instruction Compile(string id, string code)
	{
		if (string.IsNullOrEmpty(code))
			return null;

		var ins = new Instruction()
		{
			ID = id
		};

		var n = 0;
		var eventCheck = true;
		foreach (var i in code.Trim().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
		{
			if (string.IsNullOrEmpty(i))
				continue;

			var line = i.Trim();
			if (line.StartsWith("#"))
				continue;

			if (eventCheck)    //event conditions must be at the top before command lines are defined
			{
				if (line.StartsWith("@"))
				{
					ins.TriggerKey = line.Substring(1).Trim().ToUpper();
					eventCheck = false;
					continue;
				}
			}

			eventCheck = false;

			if (line.StartsWith(">"))
			{
				var labelName = line.Substring(1).Trim();
				if (ins.Labels.ContainsKey(labelName))
					throw new InvalidOperationException($"Label '{labelName}' already exists");

				ins.Labels.Add(labelName, n);
				continue;
			}

			var parts = line.SplitArguments();
			var typeName = parts[0].ToUpper();

			if (!commandTypes.TryGetValue(typeName, out var type))
				throw new Exception($"Unknown command: '{typeName}'");

			var command = Activator.CreateInstance(type) as Command;
			command.Initialize(this, ins);
			command.OnInit(parts.Skip(1).ToArray());

			ins.Commands.Add(command);
			n++;
		}

		allCommands.Add(ins);
		return ins;
	}
}
