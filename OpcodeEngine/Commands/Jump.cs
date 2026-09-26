using System.Globalization;
using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
	public class Jump : Command
	{
		[CommandParameter]
		private string labelName = "";

		public override void OnEnter()
		{
			Instruction.Jump(labelName);
		}
	}

	public class JumpIf : Command
	{
		[CommandParameter]
		private string key = "";
		[CommandParameter]
		private string labelName = "";

		public override void OnEnter()
		{
			var baseKey = key;
			var invert = Utils.HasPrefix("!", ref baseKey);
			var var = Engine.GetVar(baseKey);
			Utils.TryParseBool(var.Value.ToString(), out var boolValue);

			if (invert)
				boolValue = !boolValue;

			if (boolValue)
				Instruction.Jump(labelName);
		}
	}

	public abstract class JumpCompare : Command
	{
		[CommandParameter]
		private string left = "";

		[CommandParameter]
		private string right = "";

		[CommandParameter]
		private string labelName = "";

		protected abstract bool Compare(int comparisonResult);

		public override void OnEnter()
		{
			var a = ResolveOperand(left);
			var b = ResolveOperand(right);

			if (Compare(CompareValues(a, b)))
				Instruction.Jump(labelName);
		}

		private object ResolveOperand(string operand)
		{
			var var = Engine.GetVar(operand);
			return var != null ? var.Value : operand;
		}

		private static int CompareValues(object a, object b)
		{
			if (TryGetNumber(a, out var x) && TryGetNumber(b, out var y))
				return x.CompareTo(y);

			return string.Compare(
				Convert.ToString(a, CultureInfo.InvariantCulture),
				Convert.ToString(b, CultureInfo.InvariantCulture),
				StringComparison.OrdinalIgnoreCase);
		}

		private static bool TryGetNumber(object value, out float number)
		{
			if (value is int i)
			{
				number = i;
				return true;
			}

			if (value is float f)
			{
				number = f;
				return true;
			}

			return float.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture),
				NumberStyles.Float, CultureInfo.InvariantCulture, out number);
		}
	}

	public class JumpEQ : JumpCompare
	{
		protected override bool Compare(int comparisonResult) => comparisonResult == 0;
	}

	public class JumpNEQ : JumpCompare
	{
		protected override bool Compare(int comparisonResult) => comparisonResult != 0;
	}

	public class JumpGT : JumpCompare
	{
		protected override bool Compare(int comparisonResult) => comparisonResult > 0;
	}

	public class JumpLT : JumpCompare
	{
		protected override bool Compare(int comparisonResult) => comparisonResult < 0;
	}

	public class JumpGE : JumpCompare
	{
		protected override bool Compare(int comparisonResult) => comparisonResult >= 0;
	}

	public class JumpLE : JumpCompare
	{
		protected override bool Compare(int comparisonResult) => comparisonResult <= 0;
	}
}