using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client
{
	public interface IClipboard
	{
		void SetText(string text);
		string GetText();
	}


	public static class ClipboardManager
	{
		static string Text;

		public static void SetText(string text)
		{
			Text = text;
		}

		public static string GetText()
		{
			return Text;
		}
	}
}