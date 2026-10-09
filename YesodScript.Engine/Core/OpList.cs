using System.Globalization;

namespace YesodScript.Engine.Core;

public class OpList
{
	public readonly Type ElementType;
	public readonly List<object> Items = new();

	public OpList(Type elementType) => ElementType = elementType;

	public override string ToString() =>
		"[" + string.Join(", ",
			Items.Select(v => Convert.ToString(v, CultureInfo.InvariantCulture))) + "]";
}