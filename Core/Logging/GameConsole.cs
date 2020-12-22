using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
    public static class GameConsole
    {
        private static IMessageOutlet messenger;
        public static void SetInstance(IMessageOutlet outlet) => messenger = outlet;
        public static void Log(string message) => messenger?.Out(message);
        public static void Log(string message, Color color) => messenger?.Out(message, color);

        public static void LogWTF(string message) => messenger?.Out($"[WTF]: {message}", new Color(0, 0.8f, 0.8f));
        public static void LogWarning(string message) => messenger?.Out($"[WARN]: {message}", new Color(1, 0.7f, 0));
        public static void LogProblem(string message) => messenger?.Out($"[PROBLEM]: {message}", new Color(1, 0f, 0.5f));
        public static void LogError(string message) => messenger?.Out($"[ERROR]: {message}", new Color(1, 0f, 0));

    }
}
