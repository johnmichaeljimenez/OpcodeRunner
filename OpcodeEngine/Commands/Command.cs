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


		internal void Initialize(Engine engine, Instruction instruction, CommandType commandType, string[] args)
		{
			Engine = engine;
			Instruction = instruction;
			_commandType = commandType;
			_rawArgs = args;

			// scan all args, then map them to commandType. if args is too few, use commandType's fields' default values.
			for (int i = 0; i < commandType.Parameters.Count; i++)
			{
				var param = commandType.Parameters[i];
				bool hasArg = i < args.Length
					&& !args[i].Equals("<null>", StringComparison.InvariantCultureIgnoreCase);

				// [[var]] refs keep their default here; they're resolved at runtime
				object value = hasArg && !IsReference(args[i])
					? ConvertArgument(args[i], param.Type, param.Field.Name)
					: param.DefaultValue;

				param.Field.SetValue(this, value);
			}
		}

		private static bool IsReference(string arg)
		{
			if (string.IsNullOrEmpty(arg)) return false;
			var s = arg.Trim();
			return s.Length >= 4
				&& s.StartsWith("[[", StringComparison.Ordinal)
				&& s.EndsWith("]]", StringComparison.Ordinal);
		}

		internal void ResolveDynamicParameters()
		{
			if (_commandType == null || _rawArgs == null) return;

			for (int i = 0; i < _commandType.Parameters.Count; i++)
			{
				if (i >= _rawArgs.Length) continue;
				string raw = _rawArgs[i];
				if (!IsReference(raw)) continue;

				var param = _commandType.Parameters[i];
				string resolved = Engine.ResolveReferences(raw);

				object value = param.Type == typeof(string)
					? resolved
					: ConvertArgument(resolved.Trim(), param.Type, param.Field.Name);

				param.Field.SetValue(this, value);
			}
		}

		private static object ConvertArgument(string arg, Type type, string fieldName)
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