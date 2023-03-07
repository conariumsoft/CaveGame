using CaveGame.Client.Input;
using CaveGame.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TextCopy;

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
		public string InputBuffer { get; set; }
		public float CursorBlinkTimer { get; set; }
		public int CursorPosition { get; set; }
		public bool Focused { get; set; }

		public bool SpecialSelection { get; set; }
		public int SpecialSelectionLower { get; set; }
		public int SpecialSelectionUpper { get; set; }

		public string GetScissorTextBefore() => InputBuffer.Substring(0, SpecialSelectionLower);
		public string GetScissorTextDuring() => InputBuffer.Substring(SpecialSelectionLower, SpecialSelectionUpper - SpecialSelectionLower);
		public string GetScissorTextAfter() => InputBuffer.Substring(SpecialSelectionUpper);

		public string InternalText { get { return InputBuffer; } }
		public List<char> BlacklistedCharacters;

		public bool ClearOnReturn { get; set; }

		public string DisplayText
		{
			get
			{
				if (Math.Floor(CursorBlinkTimer * 4) % 2 == 0)
				{
					return InputBuffer.Insert(CursorPosition, "|");
				}
				return InputBuffer;
			}
		}

		KeyboardState previousKB = Keyboard.GetState();
		KeyboardState currentKB = Keyboard.GetState();

		KeyPressDetector xKeyHandler = new KeyPressDetector(Keys.X);
		KeyPressDetector cKeyHandler = new KeyPressDetector(Keys.C);
		KeyPressDetector vKeyHandler = new KeyPressDetector(Keys.V);
		KeyPressDetector leftArrowHandler = new KeyPressDetector(Keys.Left);
		KeyPressDetector rightArrowHandler = new KeyPressDetector(Keys.Right);
		public TextInput()
		{
			BlacklistedCharacters = new List<char>();
			Focused = true;
			CursorPosition = 0;
			CursorBlinkTimer = 0;
			InputBuffer = "";

		}

		public void BlacklistCharacter(char block)
        {
			BlacklistedCharacters.Add(block);
        }
		public void BlacklistCharacter(string block)
		{
			BlacklistedCharacters.Add(block[0]);
		}

		public void OnTextInput(object sender, TextInputEventArgs args)
		{
			if (Focused)
			{

				if (BlacklistedCharacters.Contains(args.Character))
					return;

				if (args.Key == Keys.Tab)
					return;

				if (args.Key == Keys.Enter)
				{
					Handler?.Invoke(this, InputBuffer);
					if (ClearOnReturn)
					{
						SpecialSelection = false;
						InputBuffer = "";
						CursorPosition = 0;
					}
					return;
				}
				if (args.Key == Keys.Back)
				{
					if (SpecialSelection)
					{
						InputBuffer = InputBuffer.Remove(SpecialSelectionLower, SpecialSelectionUpper - SpecialSelectionLower);
						CursorPosition = SpecialSelectionLower;
						SpecialSelection = false;
						return;
					} else
					{
						if (CursorPosition > 0)
						{
							InputBuffer = InputBuffer.Remove(CursorPosition - 1);
							CursorPosition--;
						}
						return;
					}
					
				}
				InputBuffer = InputBuffer.Insert(CursorPosition, args.Character.ToString());
				CursorPosition += 1;
			}
			
		}

		private bool JustPressed(Keys key) => currentKB.IsKeyDown(key) && !previousKB.IsKeyDown(key);



		public void Update(GameTime gameTime)
		{
			leftArrowHandler.Update(gameTime);
			rightArrowHandler.Update(gameTime);


			currentKB = Keyboard.GetState();

			float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
			CursorBlinkTimer += delta;

			if (SpecialSelection)
				Debug.WriteLine("lower {0}, upper {1}", SpecialSelectionLower, SpecialSelectionUpper);


			if (currentKB.IsKeyDown(Keys.LeftControl))
			{
				if (JustPressed(Keys.A))
                {
					SpecialSelection = true;
					SpecialSelectionLower = 0;
					SpecialSelectionUpper = InputBuffer.Length;
                }
				if (JustPressed(Keys.C))
				{
					if (SpecialSelection)
						ClipboardService.SetText(InputBuffer.Substring(SpecialSelectionLower, SpecialSelectionUpper - SpecialSelectionLower));
					else
						ClipboardService.SetText(InputBuffer);
				}
				if (JustPressed(Keys.V))
				{
					
					if (ClipboardService.GetText() != null)
					{
						InputBuffer = InputBuffer.Insert(CursorPosition, ClipboardService.GetText());
						CursorPosition += ClipboardService.GetText().Length;
					}
					SpecialSelection = false;
				}
				if (JustPressed(Keys.X))
				{
					if (SpecialSelection)
					{
						ClipboardService.SetText(InputBuffer.Substring(SpecialSelectionLower, SpecialSelectionUpper - SpecialSelectionLower));
						InputBuffer = InputBuffer.Remove(SpecialSelectionLower, SpecialSelectionUpper - SpecialSelectionLower);
					}else
					{
						ClipboardService.SetText(InputBuffer);
						InputBuffer = "";
						CursorPosition = 0;
					}
					SpecialSelection = false;
				}

			}



			if (leftArrowHandler.KeySignal)
			{
				if (currentKB.IsKeyDown(Keys.LeftShift))
				{

					if (SpecialSelection == false)
					{
						SpecialSelectionUpper = CursorPosition;
					}
					SpecialSelection = true;
					CursorPosition--;
					CursorPosition = Math.Max(CursorPosition, 0);
					SpecialSelectionLower = CursorPosition;
				} else
				{
					SpecialSelection = false;
					CursorPosition--;
				}
				
			}


			if (rightArrowHandler.KeySignal)
			{
				if (currentKB.IsKeyDown(Keys.LeftShift))
				{
					if (SpecialSelection == false)
					{
						SpecialSelectionLower = CursorPosition;
					}
					SpecialSelection = true;
					CursorPosition++;
					CursorPosition = Math.Min(CursorPosition, InputBuffer.Length);
					SpecialSelectionUpper = CursorPosition;
				}
				else
				{
					SpecialSelection = false;
					CursorPosition++;
				}
			}
				

			CursorPosition = Math.Max(CursorPosition, 0);
			CursorPosition = Math.Min(CursorPosition, InputBuffer.Length);

			previousKB = currentKB;

		}
	}

}
