﻿using Microsoft.Xna.Framework;

namespace CaveGame.Common
{
	public struct Message
	{
		public string Text;
		public Color TextColor;

		public Message(string text, Color color)
		{
			Text = text;
			TextColor = color;
		}
	}
}
