using System;
using System.Collections.Generic;
using System.IO;

namespace CaveGame.Common;


    public static class Logger
    {
        

        // LOGGER FORMAT
        

        private static void LogToFile(string data)
        {
            string date = DateTime.Now.ToString("yy-MM-dd");
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            File.AppendAllText(
                Path.Combine("logs", $"log_{date}.txt"), 
                DateTime.Now.ToString("HH-mm-ss") + ": " + data + "\n"
            );
        }
        
        private static void LogToDebugConsole(string context, ConsoleColor color, string data)
        {
            string timestamp = DateTime.Now.ToString("HH-mm-ss-fff-fffff");// }:{DateTime.Now.Minute}:{DateTime.Now.Second}:{DateTime.Now.Millisecond}:{DateTime.Now.Microsecond}";
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Error.Write($"[{timestamp}] ");
            Console.ForegroundColor = color;
            Console.Error.Write($"[{context}] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Error.WriteLine(data);


        }

        private static void LogToGameConsole(string data)
        {
            GameConsole.Log(data);
        }

        public static void LogClient(string data) => Log("Client", data, ConsoleColor.Cyan);

        public static void LogServer(string data) => Log("Server", data, ConsoleColor.Magenta);

        public static void LogError(string data)=>Log("Error", data, ConsoleColor.DarkRed);


        public static void LogInfo(string data) => Log("CaveGame", data, ConsoleColor.Green);
        public static void LogNote(string data) => Log("Info", data, ConsoleColor.Gray);


        public static void LogCurrentContext(string data)
        {
#if SERVER
            LogServer(data);
#elif CLIENT
            LogClient(data);
#endif
        }
        
        public static void Log(string context, string data, ConsoleColor color)
        {
            
            LogToFile(data);
            LogToDebugConsole(context, color, data);
            LogToGameConsole(data);
        }
    }

