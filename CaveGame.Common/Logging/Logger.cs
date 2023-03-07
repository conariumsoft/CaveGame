using System;
using System.IO;

namespace CaveGame.Common
{
    public static class Logger
    {

        public static void Log(string data)
        {
            string date = DateTime.Now.ToString("yy-MM-dd");
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            File.AppendAllText(
                Path.Combine("logs", $"log_{date}.txt"), 
                DateTime.Now.ToString("HH-mm-ss") + ": " + data + "\n"
            );
        }
    }
}
