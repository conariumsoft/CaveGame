using CaveGame.Client;
using System;

namespace Cave
{
	public static class Program
	{

		[STAThread]
		static void Main()
		{
			using (var game = new CaveGameGL())
				game.Run();

			
		}
	}
}
