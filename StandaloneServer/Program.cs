using CaveGame.Core.FileUtil;
using CaveGame.Server;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace StandaloneServer
{
	public static class Program
	{

		private delegate bool ConsoleCtrlHandlerDelegate(int sig);

		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerDelegate handler, bool add);

		static ConsoleCtrlHandlerDelegate _consoleCtrlHandler;


		[STAThread]
		static void Main()
		{
			ConsoleOuputWrapper consoleWrapper = new ConsoleOuputWrapper();
			//GameServer server = new GameServer(config);

			ServerConfig config = Configuration.Load<ServerConfig>("Config.xml", true);
			GameServer server = new GameServer(config);
			server.Output = consoleWrapper;

			_consoleCtrlHandler += s =>
			{
				server.OnShutdown();
				return false;
			};
			SetConsoleCtrlHandler(_consoleCtrlHandler, true);

			server.LoadPlugins();
			server.Start();
			server.Run();
		}
	}
}
