using System.Reflection;
using OpcodeEngine.Commands;

namespace OpcodeEngine.Core;

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
			if (string.IsNullOrEmpty(i.TriggerKey))
				continue;

			var isMatch = false;

			if (triggerName.Contains("*"))
			{
				isMatch = Utils.IsMatchWildcard(i.TriggerKey, triggerName);
			}
			else if (i.TriggerKey.Contains("*"))
			{
				isMatch = Utils.IsMatchWildcard(triggerName, i.TriggerKey);
			}
			else
			{
				isMatch = string.Equals(i.TriggerKey, triggerName, StringComparison.OrdinalIgnoreCase);
			}

			if (isMatch)
			{
				Run(i);
			}
		}
	}

	public void Run(Instruction instruction)
	{
		if (instruction.IsRunning)
			return;

		if (instruction.Commands.Count == 0)
			return;

		instruction.IsRunning = true;
		instruction.IsPaused = false;
		instruction.SetIndex(0);

		if (!runningCommands.Contains(instruction))
			runningCommands.Add(instruction);
	}

	public void Tick(float deltaTime)
	{
		if (deltaTime <= 0 || runningCommands.Count == 0)
			return;

		foreach (var i in runningCommands.ToArray())
		{
			if (i.IsPaused || i.CurrentIndex >= i.Commands.Count)
				continue;

			var line = i.Commands[i.CurrentIndex];
			var currentIndex = i.CurrentIndex;
			var updateDone = line.OnTick(deltaTime);

			if (!updateDone)
				continue;

			line.OnExit();
			OnPostExecuteCommand(line);

			if (!i.IsRunning)
			{
				_toRemove.Add(i);
				continue;
			}

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
			ins.IsRunning = false;
			runningCommands.Remove(ins);
			_toRemove.RemoveAt(i);
		}
	}

	private void InitRegistry()
	{
		commandTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		foreach (var t in AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(GetLoadableTypes)
			.Where(t => t.IsClass && !t.IsAbstract && typeof(Command).IsAssignableFrom(t) && t != typeof(Command)))
		{
			commandTypes.TryAdd(t.Name.ToUpper(), t);
		}
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

	public Instruction CompileFile(string path, string id = null) //null id == file path
	{
		var content = File.ReadAllText(path);
		return Compile(string.IsNullOrEmpty(id)? Path.GetFileNameWithoutExtension(path) : id, content);
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
			if (line.Length == 0)
				continue;

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

	public void Unload(Instruction instruction)
	{
		allCommands.Remove(instruction);
		runningCommands.Remove(instruction);
	}

	protected virtual void OnPostExecuteCommand(Command command)
	{

	}
}
