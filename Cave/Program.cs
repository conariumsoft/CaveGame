using CaveGame.Client;
using System;
using System.Diagnostics;

namespace Cave
{
	public static class Program
	{

		[STAThread]
		static void Main()
		{
			using (var game = new CaveGameGL())
			{
				game.Run();
				game.Exit();
			}
			Process.GetCurrentProcess().CloseMainWindow();
			Environment.Exit(Environment.ExitCode);

		}
	}
}
