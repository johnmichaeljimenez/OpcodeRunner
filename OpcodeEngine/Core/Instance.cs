using OpcodeEngine.Commands;

namespace OpcodeEngine.Core;

public class Instruction
{
	public string ID { get; set; }
	public string TriggerKey { get; set; }

	public int CurrentIndex { get; set; }
	public bool IsPaused { get; set; }
	public bool IsRunning { get; internal set; }

	public readonly List<Command> Commands = new();
	public readonly Dictionary<string, int> Labels = new();

	public void SetIndex(int index)
	{
		CurrentIndex = index;

		if (index >= Commands.Count)
			return;

		Commands[CurrentIndex].OnEnter();
	}

	public void Jump(string name)
	{
		if (!Labels.TryGetValue(name, out var index))
		{
			throw new InvalidOperationException($"Label '{name}' not found.");
		}

		SetIndex(index);
	}
}