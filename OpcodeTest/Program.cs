using System.Diagnostics;
using OpcodeEngine;
using OpcodeEngine.Core;

namespace OpcodeTest;

public class Program
{
	public static void Main(string[] args)
	{
		var commandString = """
		@NEW_GAME_START

		PRINT "HELLO"
		SAVE TEST TRUE
		WAIT 0.2
		JUMP OK TEST
			PRINT "CONDITION NOT MET"
		JUMP END
		>OK
			PRINT "CONDITION MET"
		
		RNG 0.4 RNG_OK
		BREAK

		>RNG_OK
			PRINT "RNG OK!"

		>END
			PRINT "END"
			BREAK
		""";

		var engine = new Engine();
		var command = engine.Compile("Test", commandString);
		engine.FireTrigger("NEW_GAME_START");

		var stopwatch = Stopwatch.StartNew();
		var lastTime = 0L;
		var totalElapsed = 0f;

		while (command.IsRunning)
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
