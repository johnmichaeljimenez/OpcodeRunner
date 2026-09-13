using System.Diagnostics;
using OpcodeEngine;

namespace OpcodeTest;

public class Program
{
	public static void Main(string[] args)
	{
		var commandString = """
		
		PRINT HELLO
		SAVE TEST TRUE
		WAIT 2000
		JUMP TEST BYE
			PRINT "SKIP ME"
		>BYE
			PRINT "GOOD BYE"
		
		""";

		var engine = new Engine();
		var command = engine.Create("Test", commandString);
		engine.Run(command);

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
