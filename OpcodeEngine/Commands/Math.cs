using System.Globalization;
using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public abstract class VarMath : Command
	{
		[CommandParameter]
		private string key = "";

		[CommandParameter]
		private string value = "";

		protected abstract decimal Apply(decimal a, decimal b);

		public override void OnEnter()
		{
			base.OnEnter();

			var var = Engine.GetVar(key);

			if (var == null)
				throw new InvalidOperationException($"Var '{key}' was not found.");

			if (var.Type != typeof(int) && var.Type != typeof(float))
				throw new NotSupportedException($"Var '{key}' is of type '{var.Type.Name}'; math operations require int or float.");

			decimal b;

			try
			{
				b = Convert.ToDecimal(ResolveOperand(value), CultureInfo.InvariantCulture);
			}
			catch (Exception e) when (e is FormatException || e is InvalidCastException)
			{
				throw new FormatException($"Could not parse '{value}' as a number for parameter 'value'.");
			}

			decimal a = Convert.ToDecimal(var.Value, CultureInfo.InvariantCulture);
			var result = Apply(a, b);

			var.Value = var.Type == typeof(int)
				? (object)decimal.ToInt32(decimal.Truncate(result))
				: (float)result;
		}
		
		//operands are Var names if one exists, otherwise treated as literals
		private object ResolveOperand(string operand)
		{
			var var = Engine.GetVar(operand);
			return var != null ? var.Value : operand;
		}
	}

	public class Add : VarMath
	{
		protected override decimal Apply(decimal a, decimal b) => a + b;
	}

	public class Sub : VarMath
	{
		protected override decimal Apply(decimal a, decimal b) => a - b;
	}

	public class Mul : VarMath
	{
		protected override decimal Apply(decimal a, decimal b) => a * b;
	}

	public class Div : VarMath
	{
		protected override decimal Apply(decimal a, decimal b) => a / b;
	}
}