using CaveGame.Core.FileUtil;
using CaveGame.Server;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace StandaloneServer
{
	public struct BufferMessage
	{
		public string Text;
		public ConsoleColor Color;
	}

	public static class Program
	{
		private delegate bool ConsoleCtrlHandlerDelegate(int sig);

#if WINDOZE
		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerDelegate handler, bool add);
#else
		private static bool SetConsoleCtrlHandler(ConsoleCtrlHandlerDelegate useless, bool shit)
		{
			return false;
		}
#endif
		static ConsoleCtrlHandlerDelegate _consoleCtrlHandler;

		static int maxlines = 50;

		[STAThread]
		static void Main()
		{
			Console.Title = "CaveGameServer";
			Console.CursorVisible = false;
			ConsoleOuputWrapper consoleWrapper = new ConsoleOuputWrapper();
			//GameServer server = new GameServer(config);

			ServerConfig config = Configuration.Load<ServerConfig>("Config.xml", true);
			StandaloneGameServer server = new StandaloneGameServer(config);
			server.Output = consoleWrapper;

			_consoleCtrlHandler += s =>
			{
				server.Shutdown();
				return false;
			};
			SetConsoleCtrlHandler(_consoleCtrlHandler, true);
			Task.Run(() => {
				server.LoadPlugins();
				server.Start();
				server.Run();
			});
			string inputBuf = "";

			while (true)
			{
				if (Console.KeyAvailable)
				{
					char c = Console.ReadKey(true).KeyChar;
					switch (c)
					{
						case '\r': server.OnCommand(inputBuf); inputBuf = ""; break;
						case '\b': if (inputBuf.Length > 0) { inputBuf = inputBuf.Remove(inputBuf.Length - 1); }break;
						default: inputBuf += c; break;
					}
					Console.BackgroundColor = ConsoleColor.White;
					Console.ForegroundColor = ConsoleColor.Black;
					Console.SetCursorPosition(Console.WindowLeft, Console.WindowHeight-2);
					for (int i = 0; i<Console.WindowWidth-1; i++)
					{
						Console.Write(" ");
					}
					
					Console.SetCursorPosition(Console.WindowLeft, Console.WindowHeight - 2);
					
					Console.Write("> " + inputBuf);

					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.White;
					
				}
				Console.SetCursorPosition(Console.WindowLeft, Console.WindowHeight - 1);

				Console.Write(server.Information.PadRight(Console.WindowWidth-1));
				// now lets handle our "output stream"
				for (int i = 0; i < Math.Min(Console.WindowHeight - 3, consoleWrapper.BufferMessages.Count); i++)
				{

					Console.SetCursorPosition(Console.WindowLeft, i);
					Console.ForegroundColor = consoleWrapper.BufferMessages[i].Color;
					Console.Write(consoleWrapper.BufferMessages[i].Text);
				}
				Console.ForegroundColor = ConsoleColor.White;
			}
		}
	}
}
