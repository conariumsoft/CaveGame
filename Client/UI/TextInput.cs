using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client.UI
{



	public interface ITextProvider
	{
		string DisplayText { get; }
		string InternalText { get; }
	}

	// TODO Input Limitations

	public class TextInput : ITextProvider
	{

		public delegate void TextInputHandler(object sender, string text);

		public event TextInputHandler Handler;
		public string inputBuffer;
		public float cursorTimer;
		public int cursorPosition;
		public bool Focused;
		public string InternalText { get { return inputBuffer; } }
		public List<char> BlacklistedCharacters;


		public bool ClearOnReturn { get; set; }

		public string DisplayText
		{
			get
			{
				if (Math.Floor(cursorTimer * 4) % 2 == 0)
				{
					return inputBuffer.Insert(cursorPosition, "|");
				}
				return inputBuffer;
			}
		}

		KeyboardState previousKB;

		public TextInput()
		{
			BlacklistedCharacters = new List<char>();
			Focused = true;
			cursorPosition = 0;
			cursorTimer = 0;
			inputBuffer = "";

		}

		public void OnTextInput(object sender, TextInputEventArgs args)
		{
			if (Focused)
			{
				if (BlacklistedCharacters.Contains(args.Character))
					return;

				if (args.Key == Keys.Enter)
				{
					Handler?.Invoke(this, inputBuffer);
					if (ClearOnReturn)
					{
						inputBuffer = "";
						cursorPosition = 0;
					}
					return;
				}
				if (args.Key == Keys.Back)
				{
					if (cursorPosition > 0)
					{
						inputBuffer = inputBuffer.Remove(cursorPosition - 1);
						cursorPosition--;
					}
					return;
				}
				inputBuffer = inputBuffer.Insert(cursorPosition, args.Character.ToString());
				cursorPosition += 1;
			}
			
		}


		public void Update(GameTime gameTime)
		{
			KeyboardState keyboard = Keyboard.GetState();

			float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
			cursorTimer += delta;

			if (keyboard.IsKeyDown(Keys.Left) && !previousKB.IsKeyDown(Keys.Left))
				cursorPosition--;

			if (keyboard.IsKeyDown(Keys.Right) && !previousKB.IsKeyDown(Keys.Right))
				cursorPosition++;

			cursorPosition = Math.Max(cursorPosition, 0);
			cursorPosition = Math.Min(cursorPosition, inputBuffer.Length);

			previousKB = keyboard;

		}
	}


	public class TextInputWithHistory
	{

	}
}
