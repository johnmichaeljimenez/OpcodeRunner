using System.Diagnostics;
using OpcodeEngine;
using OpcodeEngine.Core;

namespace OpcodeTest;

public class Program
{
	public static void Main(string[] args)
	{
		var commandString = """
		@_INIT
		DECVAR CTR 10 int
		PRINT "INITIALIZED PROGRAM"

		PRINT "BEGIN COUNTDOWN"
		WAIT 1
		>LOOP
			JUMPEQ CTR 0 END
			PRINT "[[CTR]]"
			SUB CTR 1
			WAIT 1
			JUMP LOOP

		>END
			PRINT "DONE!"
			BREAK
		""";

		var engine = new Engine(immediateMode: true);
		engine.Compile("Test", commandString);
		engine.Initialize();

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
