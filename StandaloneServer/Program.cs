using CaveGame.Core.FileUtil;
using CaveGame.Core.Game.Tiles;
using CaveGame.Server;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CaveGame.Core;

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
			Tile.AssertTileEnumeration();

			Console.Title = "CaveGameServer";
			Console.CursorVisible = false;
			ConsoleOuputWrapper consoleWrapper = new ConsoleOuputWrapper();
			//GameServer server = new GameServer(config);

			ServerConfig config = Configuration.Load<ServerConfig>("Config.xml", true);
			WorldMetadata worldMDT = new WorldMetadata { Name = config.World, Seed = 420 };
			StandaloneGameServer server = new StandaloneGameServer(config, worldMDT);
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

			

			Console.SetCursorPosition(Console.WindowLeft, Console.WindowHeight - 1);
			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write("> ".PadRight(Console.WindowWidth-2));
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
			Console.SetCursorPosition(0, 0);

			while (true)
			{
				Task.Delay(10);
				Console.SetCursorPosition(0, 0);


				// now lets handle our "output stream"
				while (consoleWrapper.BufferMessages.Count > Console.WindowHeight - 1)
				{
					consoleWrapper.BufferMessages.RemoveAt(0);
				}
				int ic = 0;

				int start = Math.Max(0, consoleWrapper.BufferMessages.Count - Console.WindowHeight - 1);
				int end = Math.Min(consoleWrapper.BufferMessages.Count , Console.WindowHeight);

				
				Console.SetCursorPosition(0, 0);
				for (int i = start; i < end; i++)
				{
					var bufferMessage = consoleWrapper.BufferMessages[i];

					//Console.SetCursorPosition(Console.WindowLeft, i);
					Console.ForegroundColor = bufferMessage.Color;
					Console.Write(""); 
					Console.Write(bufferMessage.Text);
					ic += Math.Min(bufferMessage.Text.Length / Console.WindowWidth, 1);
					if (ic > Console.WindowHeight)
                    {
						break;
                    }
				}
				Console.ForegroundColor = ConsoleColor.White;

				if (Console.KeyAvailable)
				{
					
					char c = Console.ReadKey(true).KeyChar;
					switch (c)
					{
						case '\r': server.OnCommand(inputBuf); inputBuf = "";  break;
						case '\b': if (inputBuf.Length > 0) { inputBuf = inputBuf.Remove(inputBuf.Length - 1); }break;
						default: inputBuf += c; break;
					}

					Console.BackgroundColor = ConsoleColor.White;
					Console.ForegroundColor = ConsoleColor.Black;
					Console.SetCursorPosition(Console.WindowLeft, Console.WindowHeight - 1);
					Console.Write(" ".PadRight(Console.WindowWidth-2));

					Console.SetCursorPosition(Console.WindowLeft, Console.WindowHeight - 1);

					Console.Write("> " + inputBuf);

					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.White;
					Console.SetCursorPosition(0, 0);
					for (int i = 0; i < Console.WindowHeight - 1; i++)
					{
						Console.Write("".PadRight(Console.WindowWidth - 1));
					}
				}
			}
		}
	}
}
