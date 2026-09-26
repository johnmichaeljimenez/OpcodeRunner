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
		DECVAR CTR 10 int

		PRINT "HAHA"
		PRINT "LET'S GO"

		PRINT "BEGIN COUNTDOWN!"
		>LOOP
			JUMPEQ CTR 1 END
			SUB CTR 1
			PRINT "[[CTR]]"
			WAIT 1
			JUMP LOOP

		>END
			PRINT "DONE!"
			BREAK
		""";

		var engine = new Engine(immediateMode: true);
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
