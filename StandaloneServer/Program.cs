using CaveGame.Core.FileUtil;
using CaveGame.Server;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Threading;

namespace StandaloneServer
{
	public static class Program
	{
		[STAThread]
		static void Main()
		{

			

			
			ConsoleOuputWrapper consoleWrapper = new ConsoleOuputWrapper();
			//GameServer server = new GameServer(config);

			ServerConfig config = Configuration.Load<ServerConfig>("Config.xml", true);
			GameServer server = new GameServer(config);
			server.Output = consoleWrapper;

			server.LoadPlugins();
			server.Start();
			server.Run();
		}
	}
}
