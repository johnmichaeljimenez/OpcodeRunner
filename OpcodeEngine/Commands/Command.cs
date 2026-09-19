using System.Globalization;
using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public abstract class Command
	{
		protected Engine Engine { get; private set; }
		protected Instruction Instruction { get; private set; }
		internal void Initialize(Engine engine, Instruction instruction, CommandType commandType, string[] args)
		{
			Engine = engine;
			Instruction = instruction;

			// scan all args, then map them to commandType. if args is too few, use commandType's fields' default values.
			for (int i = 0; i < commandType.Parameters.Count; i++)
			{
				var param = commandType.Parameters[i];
				object value = i < args.Length && !args[i].Equals("<null>", StringComparison.InvariantCultureIgnoreCase)
					? ConvertArgument(args[i], param.Type, param.Field.Name)
					: param.DefaultValue;

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