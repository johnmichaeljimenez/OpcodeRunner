using OpcodeEngine.Core;

namespace OpcodeEngine.Commands;

public class SetVar : Command
{
	[CommandParameter]
	private string key;

	[CommandParameter]
	private string value;

	[CommandParameter]
	private bool relative;

	public override void OnEnter()
	{
		base.OnEnter();
		Engine.GetVar(key).SetValue(value, relative);
	}
}

public class DecVar : Command
{
	[CommandParameter]
	private string key;

	[CommandParameter]
	private string value;

	[CommandParameter]
	private string typeName;

	public override void OnEnter()
	{
		base.OnEnter();

		var newVar = new Var()
		{
			Name = key,
			Type = GetStringType()
		};

		if (Engine.GetVar(key) != null)
			throw new Exception($"Var '{key}' already exists.");

		Engine.Vars.Add(newVar);
		newVar.SetValue(value, false);
	}

	private Type GetStringType()
	{
		return typeName?.ToLowerInvariant() switch
		{
			"int" => typeof(int),
			"float" => typeof(float),
			"string" => typeof(string),
			"bool" => typeof(bool),
			_ => null
		};
	}
}