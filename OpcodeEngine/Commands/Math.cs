using System.Globalization;
using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public abstract class VarMath : Command
	{
		[CommandParameter]
		private string key = "";

		[CommandParameter]
		private float value = 0;

		protected abstract float Apply(float a, float b);

		public override void OnEnter()
		{
			base.OnEnter();

			var var = Engine.GetVar(key);

			if (var == null)
				throw new InvalidOperationException($"Var '{key}' was not found.");

			if (var.Type != typeof(int) && var.Type != typeof(float))
				throw new NotSupportedException($"Var '{key}' is of type '{var.Type.Name}'; math operations require int or float.");

			var a = Convert.ToSingle(var.Value, CultureInfo.InvariantCulture);
			var result = Apply(a, value);

			var.Value = var.Type == typeof(int)
				? (object)(int)(float.Truncate(result))
				: (float)result;
		}
	}

	public class Add : VarMath { protected override float Apply(float a, float b) => a + b; }
	public class Sub : VarMath { protected override float Apply(float a, float b) => a - b; }
	public class Mul : VarMath { protected override float Apply(float a, float b) => a * b; }
	public class Div : VarMath { protected override float Apply(float a, float b) => a / b; }
}