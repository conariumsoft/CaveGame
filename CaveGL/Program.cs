using CaveGame.Client;
using CaveGame.Core.Game.Tiles;
using System;
using System.Diagnostics;

namespace Cave
{
    public static class Program
	{

		[STAThread]
		static void Main(string[] args)
		{
			// parse command line arguments
			CaveGameArguments arguments = new CaveGameArguments();

			for (int x = 0; x < args.Length; x++)
			{
				switch(args[x])
				{
					case "-world":
						arguments.AutoLoadWorld = args[x + 1];
						break;
					case "-connect":
						arguments.AutoConnectName = args[x + 1];
						arguments.AutoConnectAddress = args[x + 2];
						break;
					default:
						break;

				}
			}

			Tile.AssertTileEnumeration();
			using (var game = new CaveGameGL(arguments))
			{
#if !DEBUG

				try
                {
					
					game.Run();
					game.Exit();
				} catch(Exception e)
                {
					CrashReport report = new CrashReport(game, e);
					report.GenerateHTMLReport();
					throw e;
					
                }
#else
				game.Run();
				game.Exit();
#endif
			}

			Process.GetCurrentProcess().CloseMainWindow();
			Environment.Exit(Environment.ExitCode);

		}
	}
}
