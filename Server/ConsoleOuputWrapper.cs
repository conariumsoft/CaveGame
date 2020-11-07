using CaveGame.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
		public ConsoleMsg(string text, ConsoleColor color)
		{
			Text = text;
			Color = color;
		}
	}

	public class ConsoleOuputWrapper : IMessageOutlet
	{

		public List<ConsoleMsg> Messages { get; set; }

		public List<ConsoleMsg> BufferMessages { get; set; }

		public ConsoleOuputWrapper()
		{
			Messages = new List<ConsoleMsg>();
			BufferMessages = new List<ConsoleMsg>();
		}

		public void Import(ConsoleMsg message)
		{
			string text = message.Text;

			if (Regex.IsMatch(text, @"%\d"))
			{

				int cnum = 15;

				while (Regex.IsMatch(text, @"%\d"))
				{
					int idx = text.IndexOf("%");
					char code = text[idx + 1];

					

					
					BufferMessages.Add(new ConsoleMsg(text.Substring(0, idx), (ConsoleColor)cnum));
					bool worked = Int32.TryParse(code.ToString(), out cnum);
					text = text.Substring(idx + 2);
				}
				BufferMessages.Add(new ConsoleMsg(text, (ConsoleColor)cnum));
			} else
			{
				BufferMessages.Add(new ConsoleMsg(text, Color.White));
			}

			
		}

		public void Out(string message, Color color)
		{
			//Console.ForegroundColor = color.ToConsoleColor();
			//Console.WriteLine(message);
			//Console.ForegroundColor = ConsoleColor.White;
			var msg = new ConsoleMsg(message, color);
			Messages.Add(msg);
			Import(msg);

		}

		public void Out(string message)
		{
			//Console.WriteLine(message);
			var msg = new ConsoleMsg(message, Color.White);
			Messages.Add(msg);
			Import(msg);
		}
	}
}
