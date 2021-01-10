using CaveGame.Client;
using CaveGame.Core.Game.Tiles;
using System;
using System.Diagnostics;

namespace Cave
{
    public static class Program
	{

		[STAThread]
		static void Main()
		{
			Tile.AssertTileEnumeration();
			using (var game = new CaveGameGL())
			{
#if !DEBUGBALLS

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
