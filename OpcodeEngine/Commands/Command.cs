using System.Globalization;
using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public abstract class Command
	{
		protected Engine Engine { get; private set; }
		protected Instruction Instruction { get; private set; }

		private CommandType _commandType;
		private string[] _rawArgs;
		
		private Dictionary<int, string> _deferredArgs;

		internal void Initialize(Engine engine, Instruction instruction, CommandType commandType, string[] args)
		{
			Engine = engine;
			Instruction = instruction;
			_commandType = commandType;
			_rawArgs = args;
			_deferredArgs = new Dictionary<int, string>();

			for (int i = 0; i < commandType.Parameters.Count; i++)
			{
				var param = commandType.Parameters[i];
				bool hasArg = i < args.Length
					&& !args[i].Equals("<null>", StringComparison.InvariantCultureIgnoreCase);

				if (hasArg && !IsReference(args[i]))
				{
					try
					{
						object value = ConvertArgument(args[i], param.Type, param.Field.Name);
						param.Field.SetValue(this, value);
					}
					catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
					{
						_deferredArgs[i] = args[i];
						param.Field.SetValue(this, param.DefaultValue);
					}
				}
				else
				{
					param.Field.SetValue(this, param.DefaultValue);
				}
			}
		}

		internal void ResolveDynamicParameters()
		{
			if (_commandType == null || _rawArgs == null) return;

			for (int i = 0; i < _commandType.Parameters.Count; i++)
			{
				if (i >= _rawArgs.Length) continue;
				string raw = _rawArgs[i];

				bool isRef = IsReference(raw);
				bool isDeferred = _deferredArgs != null && _deferredArgs.ContainsKey(i);

				if (!isRef && !isDeferred) continue;

				var param = _commandType.Parameters[i];
				string resolved = Engine.ResolveReferences(raw, Instruction);

				if (resolved == raw && isDeferred)
				{
					var nakedVar = Engine.GetVar(raw);
					if (nakedVar != null && nakedVar.Value != null)
					{
						resolved = Convert.ToString(nakedVar.Value, CultureInfo.InvariantCulture);
					}
				}

				object value = param.Type == typeof(string)
					? resolved
					: ConvertArgument(resolved.Trim(), param.Type, param.Field.Name);

				param.Field.SetValue(this, value);
			}
		}

		private static bool IsReference(string arg)
		{
			if (string.IsNullOrEmpty(arg))
				return false;

			return arg.Contains("[[") || arg.Contains("<");
		}

		internal static object ConvertArgument(string arg, Type type, string fieldName)
		{
			if (type == typeof(string))
				return arg;

			if (type == typeof(bool))
			{
				if (Utils.TryParseBool(arg, out var b))
					return b;

				throw new FormatException($"Could not parse '{arg}' as bool for parameter '{fieldName}'");
			}

			if (type == typeof(float))
			{
				if (float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
					return f;

				throw new FormatException($"Could not parse '{arg}' as float for parameter '{fieldName}'");
			}

			if (type == typeof(int))
			{
				if (int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
					return n;

				throw new FormatException($"Could not parse '{arg}' as int for parameter '{fieldName}'");
			}

			if (type.IsEnum)
			{
				return Enum.Parse(type, arg, ignoreCase: true);
			}

			throw new NotSupportedException($"Unsupported command parameter type '{type}' for parameter '{fieldName}'");
		}

		public virtual void OnInit()
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