namespace OpcodeEngine.Commands;

public class _Trigger : Command
{
	public string TriggerKey { get; set; }
	public Dictionary<string, string> Parameters { get; set; } = new();

	public override void OnInit() { }
	public override void OnEnter() { }

	public override bool OnTick(float deltaTime)
	{
		return true;
	}
}