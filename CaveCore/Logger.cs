using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CaveGame.Core
{
	public static class Logger
	{

		public static void Log(string data)
		{
			string date = DateTime.Now.ToString("yy-MM-dd");
			if (!Directory.Exists("logs"))
				Directory.CreateDirectory("logs");
			File.AppendAllText($"logs/log_{date}.txt", DateTime.Now.ToString("HH-mm-ss")+": " +data+"\n");
		}
	}
}
