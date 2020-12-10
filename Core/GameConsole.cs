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

    }
}
