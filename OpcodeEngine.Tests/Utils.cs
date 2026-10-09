using OpcodeEngine.Core;

namespace OpcodeEngine.Tests;

public static class Utils
{
	internal static Engine Test(out string output, params string[] scriptFileNames)
	{
		return Test(out output, true, scriptFileNames);
	}
	
	internal static Engine Test(out string output, bool immediateMode, params string[] scriptFileNames)
	{
		var engineOutput = "";
		var engine = new Engine();
		foreach (var i in scriptFileNames)
		{
			var path = i.Replace("\\", "/"); //for linux
			engine.CompileFile($"scripts/{path}");
		}

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