using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using OpcodeEngine.Commands;

namespace OpcodeEngine.Core;

public class CommandType
{
	public string Name;
	public Type Type;
	public List<CommandParameter> Parameters;
}

public class CommandParameter
{
	public FieldInfo Field;
	public Type Type;
	public object DefaultValue;
}

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class CommandParameterAttribute : Attribute { }

public class Engine : IDisposable
{
	public long CurrentTick { get; private set; }
	public readonly List<Var> Vars = new();

	private Dictionary<string, CommandType> commandTypes;

	private readonly List<Instruction> allCommands = new();
	private readonly List<Instruction> runningCommands = new();
	private readonly List<Instruction> _toRemove = new();

	public Random RNG { get; private set; }
	private int _randomSeed;
	public int RandomSeed
	{
		get
		{
			return _randomSeed;
		}

		set
		{
			if (_randomSeed == value)
				return;

			_randomSeed = value;
			RNG = new Random(_randomSeed);
		}
	}

	public bool IsRunning => runningCommands.Count > 0;
	public bool ImmediateMode { get; private set; } = false;

	public Action<string> OnOutput;

	public Engine(bool immediateMode = false, int randomSeed = 0)
	{
		RandomSeed = randomSeed;
		ImmediateMode = immediateMode;
		InitRegistry();
	}

	public void Initialize()
	{
		OnInitialize();
		FireTrigger("_INIT");
		Tick(0);
	}

	public void Dispose()
	{
		OnDispose();
		_toRemove.Clear();
		runningCommands.Clear();
		allCommands.Clear();
	}

	public List<Instruction> FireTrigger(string triggerName)
	{
		var fired = new List<Instruction>();

		if (string.IsNullOrEmpty(triggerName))
			return fired;

		var parts = new List<string>();
		if (triggerName.Contains(' '))
		{
			parts = Utils.SplitArguments(triggerName);
			triggerName = parts[0];
			parts.RemoveAt(0);
		}

		foreach (var i in allCommands)
		{
			if (string.IsNullOrEmpty(i.TriggerKey))
				continue;

			if (string.Equals(i.TriggerKey, triggerName, StringComparison.InvariantCultureIgnoreCase))
			{
				var parameters = i.TriggerParameters.Keys.ToList();
				for (int j = 0; j < parts.Count; j++)   //input can be shorter than expected (TODO: provide default value for others)
				{
					i.TriggerParameters[parameters[j]] = parts[j];
				}

				Run(i);
				fired.Add(i);
			}
		}

		return fired;
	}

	public void Run(string id)
	{
		Run(allCommands.FirstOrDefault(p => string.Equals(id, p.ID, StringComparison.InvariantCultureIgnoreCase)));
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

	public void Stop(string id)
	{
		Stop(allCommands.FirstOrDefault(i => string.Equals(i.ID, id, StringComparison.OrdinalIgnoreCase)));
	}

	public void Stop(Instruction instruction)
	{
		if (instruction != null)
		{
			instruction.IsRunning = false;  //deferred
		}
	}

	public void Tick(float deltaTime)
	{
		CurrentTick++;
		if (runningCommands.Count == 0)
			return;

		foreach (var i in runningCommands.ToArray())
		{
			if (i.IsPaused || i.CurrentIndex >= i.Commands.Count)
				continue;

			if (ImmediateMode)
			{
				while (i.IsRunning && !i.IsPaused && i.CurrentIndex < i.Commands.Count)
				{
					var line = i.Commands[i.CurrentIndex];
					var currentIndex = i.CurrentIndex;
					var updateDone = line.OnTick(deltaTime);

					if (!updateDone)
						break; //waiting

					line.OnExit();
					OnPostExecuteCommand(line);

					if (!i.IsRunning)
						break;

					if (currentIndex != i.CurrentIndex)
						continue; //jumped

					i.SetIndex(i.CurrentIndex + 1);
				}

				if (!i.IsRunning || i.CurrentIndex >= i.Commands.Count)
					_toRemove.Add(i);
			}
			else
			{
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

				if (currentIndex != i.CurrentIndex)
					continue;

				i.SetIndex(i.CurrentIndex + 1);
				if (i.CurrentIndex >= i.Commands.Count)
				{
					_toRemove.Add(i);
				}
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
		var validTypes = new List<Type>();
		foreach (var t in AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(GetLoadableTypes)
			.Where(t => t.IsClass && !t.IsAbstract && typeof(Command).IsAssignableFrom(t) && t != typeof(Command)))
		{
			validTypes.Add(t);
		}

		commandTypes = new();
		foreach (var i in validTypes)
		{
			var instance = Activator.CreateInstance(i) as Command;
			if (instance == null)
				continue;

			var parameters = new List<CommandParameter>();
			var typeChain = new List<Type>();
			for (var t = i; t != null && t != typeof(object); t = t.BaseType)
				typeChain.Add(t);
			typeChain.Reverse();

			foreach (var declaringType in typeChain)
			{
				var fields = declaringType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				foreach (var field in fields)
				{
					if (field.GetCustomAttribute<CommandParameterAttribute>() == null)
						continue;

					parameters.Add(new CommandParameter
					{
						Field = field,
						Type = field.FieldType,
						DefaultValue = field.GetValue(instance)
					});
				}
			}

			var name = i.Name.ToUpper();
			commandTypes[name] = new CommandType
			{
				Name = name,
				Type = i,
				Parameters = parameters
			};
		}
	}

	public IEnumerable<Instruction> FindInstructions(Func<Instruction, string> selector, string query)
	{
		if (selector == null)
			throw new ArgumentNullException(nameof(selector));

		return allCommands.Where(i => Utils.IsMatch(selector(i), query));
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
		return Compile(string.IsNullOrEmpty(id) ? Path.GetFileNameWithoutExtension(path) : id, content);
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
				var triggerLine = line;
				if (Utils.HasPrefix("@", ref triggerLine))
				{
					eventCheck = false;
					if (!line.Contains(' '))
					{
						ins.TriggerKey = triggerLine;
						continue;
					}

					var lineParts = triggerLine.SplitArguments();
					ins.TriggerKey = lineParts[0];

					ins.TriggerParameters.Clear();
					for (int j = 1; j < lineParts.Count; j++)
					{
						var varName = lineParts[j];
						varName = varName.Replace("<", "").Replace(">", "");
						ins.TriggerParameters.Add(varName, "");
					}

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

			var command = Activator.CreateInstance(type.Type) as Command;
			command.Initialize(this, ins, type, parts.Skip(1).ToArray());
			command.OnInit();

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

	protected virtual void OnInitialize()
	{

	}

	protected virtual void OnDispose()
	{

	}

	protected virtual void OnPostExecuteCommand(Command command)
	{

	}

	internal string ResolveReferences(string rawValue, Instruction ins)
	{
		if (string.IsNullOrEmpty(rawValue))
			return rawValue;

		var current = Regex.Replace(rawValue, @"\[\[(.*?)\]\]", match =>
		{
			var refName = match.Groups[1].Value.Trim();
			var referenced = GetVar(refName);

			if (referenced == null)
				throw new InvalidOperationException($"Referenced Var '{refName}' was not found.");

			if (referenced.Value == null)
				throw new InvalidOperationException($"Referenced Var '{refName}' has no value.");

			return Convert.ToString(referenced.Value, CultureInfo.InvariantCulture);
		});

		current = Regex.Replace(current, @"<(.*?)>", match =>
		   {
			   var refName = match.Groups[1].Value.Trim();
			   var localValue = ins.TriggerParameters[refName];

			   if (localValue == null)
				   throw new InvalidOperationException($"Local parameter '{refName}' was not found.");

			   return Convert.ToString(localValue, CultureInfo.InvariantCulture);
		   });

		return current;
	}

	public Var GetVar(string name)
	{
		return Vars.FirstOrDefault(v =>
			string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));
	}
}
