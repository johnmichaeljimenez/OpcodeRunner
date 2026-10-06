using System.Globalization;
using OpcodeEngine.Core;

namespace OpcodeEngine.Commands;

public static class ListAccess
{
	public static void Set(Engine e, string key, Instruction scope, params object[] values)
	{
		var list = Get(e, key, scope);
		if (list == null)
			throw new InvalidOperationException($"List '{key}' not found.");

		list.Items.Clear();
		list.Items.AddRange(values);
	}

	public static OpList Get(Engine e, string key, Instruction scope) =>
		e.GetVar(key, scope)?.Value as OpList
			?? throw new InvalidOperationException($"'{key}' is not a list.");

	public static int Index(OpList l, string raw, bool allowEnd = false)
	{
		int i = int.Parse(raw, CultureInfo.InvariantCulture);
		if (i < 0) i += l.Items.Count;              // -1 = last
		int max = allowEnd ? l.Items.Count : l.Items.Count - 1;
		if (i < 0 || i > max)
			throw new IndexOutOfRangeException($"Index {raw} out of range for list of {l.Items.Count}.");
		return i;
	}

	public static object Parse(OpList l, string raw, string field) =>
		Command.ConvertArgument(raw, l.ElementType, field);
}

public class DefList : Command
{
	[CommandParameter] private string key;
	[CommandParameter] private string elementType;

	public override void OnEnter()
	{
		bool isLocal = key != null && key.StartsWith("_", StringComparison.Ordinal);
		var vars = isLocal ? Instruction.Vars : Engine.Vars;

		if (vars.Any(v => string.Equals(v.Name, key, StringComparison.OrdinalIgnoreCase)))
			throw new Exception($"Var '{key}' already exists.");

		var t = elementType?.ToLowerInvariant() switch
		{
			"int" => typeof(int),
			"float" => typeof(float),
			"string" => typeof(string),
			"bool" => typeof(bool),
			_ => throw new NotSupportedException($"Unknown list element type '{elementType}'.")
		};

		vars.Add(new Var { Name = key, Type = typeof(OpList), Value = new OpList(t) });
	}
}

public class ListAdd : Command
{
	[CommandParameter] private string key;
	[CommandParameter] private string value;

	public override void OnEnter()
	{
		var l = ListAccess.Get(Engine, key, Instruction);
		l.Items.Add(ListAccess.Parse(l, value, nameof(value)));
	}
}

public class ListSet : Command
{
	[CommandParameter] private string key;
	[CommandParameter] private string index;
	[CommandParameter] private string value;

	public override void OnEnter()
	{
		var l = ListAccess.Get(Engine, key, Instruction);
		l.Items[ListAccess.Index(l, index)] = ListAccess.Parse(l, value, nameof(value));
	}
}

public class ListInsert : Command
{
	[CommandParameter] private string key;
	[CommandParameter] private string index;
	[CommandParameter] private string value;

	public override void OnEnter()
	{
		var l = ListAccess.Get(Engine, key, Instruction);
		l.Items.Insert(ListAccess.Index(l, index, allowEnd: true),
					   ListAccess.Parse(l, value, nameof(value)));
	}
}

public class ListRemove : Command
{
	[CommandParameter] private string key;
	[CommandParameter] private string index;

	public override void OnEnter()
	{
		var l = ListAccess.Get(Engine, key, Instruction);
		l.Items.RemoveAt(ListAccess.Index(l, index));
	}
}

public class ListClear : Command
{
	[CommandParameter] private string key;
	public override void OnEnter() => ListAccess.Get(Engine, key, Instruction).Items.Clear();
}

public class ListGet : Command
{
	[CommandParameter] private string key;
	[CommandParameter] private string index;
	[CommandParameter] private string outVar;

	public override void OnEnter()
	{
		var l = ListAccess.Get(Engine, key, Instruction);
		var v = Engine.GetVar(outVar, Instruction) ?? throw new InvalidOperationException($"Var '{outVar}' not found.");

		// store as the natural element type so [[outVar]] and math work normally
		v.Value = l.ElementType == typeof(string)
			? l.Items[ListAccess.Index(l, index)]
			: Command.ConvertArgument(
				Convert.ToString(l.Items[ListAccess.Index(l, index)], CultureInfo.InvariantCulture),
				v.Type, nameof(outVar));
	}
}

public class ListCount : Command
{
	[CommandParameter] private string key;
	[CommandParameter] private string outVar;

	public override void OnEnter()
	{
		var l = ListAccess.Get(Engine, key, Instruction);
		var v = Engine.GetVar(outVar, Instruction) ?? throw new InvalidOperationException($"Var '{outVar}' not found.");
		v.SetValue(l.Items.Count.ToString(CultureInfo.InvariantCulture), false);
	}
}