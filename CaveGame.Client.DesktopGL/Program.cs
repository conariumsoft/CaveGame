using System;
using System.Diagnostics;
using CaveGame.Client.DesktopGL;
using CaveGame.Common;
using CaveGame.Common.Game.Tiles;

namespace CaveGame.Client.DesktopGL;
public static class Program
{

	
	// When Debugging the game, we want direct access to the code
	private static void DebugGameloop()
	{
		using (var game = new CaveGameDesktopClient())
		{
			game.Run();
			game.Exit();
		}
	}


	// In "Release" We want crashes to generate a report
	// that the player can submit to us
	private static void ReleaseGameloop()
	{
		using (var game = new CaveGameDesktopClient())
		{
			try
			{
				game.Run();
				game.Exit();
			}
			catch(Exception e)
			{
				CrashReport report = new CrashReport(game, e);
				report.GenerateHTMLReport();
				throw e;
			}
		}
	}

	[STAThread]
	static void Main(string[] args)
	{
		Logger.LogInfo("Starting Program");
		// parse command line arguments
		Tile.AssertTileEnumeration();
		
#if DEBUG
		DebugGameloop();
#else
		ReleaseGameloop();
#endif
		Process.GetCurrentProcess().CloseMainWindow(); 
		Environment.Exit(Environment.ExitCode); 
	}
}
