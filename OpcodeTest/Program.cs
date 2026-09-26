using System.Diagnostics;
using OpcodeEngine;
using OpcodeEngine.Core;

namespace OpcodeTest;

public class Program
{
	public static void Main(string[] args)
	{
		var commandString = """
		@INIT
		DECVAR MSG "HELLO WORLD" string
		DECVAR FLAG false bool
		JUMPIF FLAG END

		PRINT [[MSG]]
		>END
			BREAK
		""";

		var engine = new Engine();
		engine.Compile("Test", commandString);
		
		engine.FireTrigger("INIT");

		var stopwatch = Stopwatch.StartNew();
		var lastTime = 0L;
		var totalElapsed = 0f;

		while (engine.IsRunning)
		{
			var currentTime = stopwatch.ElapsedMilliseconds;
			var delta = (currentTime - lastTime) / 1000f;
			lastTime = currentTime;
			totalElapsed += delta;

			try
			{
				engine.Tick(delta);
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex.Message);
				break;
			}

			Thread.Sleep(1);
		}

	}
}
