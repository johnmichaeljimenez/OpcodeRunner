using OpcodeEngine.Core;

namespace OpcodeEngine.Tests;

public static class Utils
{
	internal static Engine Test(string scriptFileName, out string output)
	{
		scriptFileName = scriptFileName.Replace("\\", "/"); //for linux

		var engineOutput = "";
		var engine = new Engine(immediateMode: true);
		engine.CompileFile($"scripts/{scriptFileName}");
		engine.OnOutput += (str) => { engineOutput += $"{str}"; };
		engine.Initialize();

		while (engine.IsRunning)
		{
			engine.Tick(0.033333f); //~30fps delta time
		}

		output = engineOutput.Trim();
		return engine;
	}
}