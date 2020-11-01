using CaveGame.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Server
{

	public static class Conversions
	{
		public static System.ConsoleColor ToConsoleColor(this Color c)
		{
			int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0; // Bright bit
			index |= (c.R > 64) ? 4 : 0; // Red bit
			index |= (c.G > 64) ? 2 : 0; // Green bit
			index |= (c.B > 64) ? 1 : 0; // Blue bit
			return (System.ConsoleColor)index;
		}
	}

	public struct ConsoleMsg
	{
		public string Text;
		public System.ConsoleColor Color;

		public ConsoleMsg(Message msg)
		{
			Text = msg.Text;
			Color = msg.TextColor.ToConsoleColor();
		}

		public ConsoleMsg(string text)
		{
			Text = text;
			Color = Microsoft.Xna.Framework.Color.White.ToConsoleColor();
		}
		public ConsoleMsg(string text, Color color)
		{
			Text = text;
			Color = color.ToConsoleColor();
		}
	}

	public class ConsoleOuputWrapper : IMessageOutlet
	{

		public List<ConsoleMsg> Messages { get; set; }

		public ConsoleOuputWrapper()
		{
			Messages = new List<ConsoleMsg>();
		}

		public void Out(string message, Color color)
		{
			//Console.ForegroundColor = color.ToConsoleColor();
			//Console.WriteLine(message);
			//Console.ForegroundColor = ConsoleColor.White;
			Messages.Add(new ConsoleMsg(message, color));
		}

		public void Out(string message)
		{
			//Console.WriteLine(message);
			Messages.Add(new ConsoleMsg(message, Color.White));
		}
	}
}
