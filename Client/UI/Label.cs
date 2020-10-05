using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client.UI
{
	public enum TextXAlignment
	{
		Left,
		Center,
		Right
	}
	public enum TextYAlignment
	{
		Top,
		Center,
		Bottom,
		
	}


	public interface ITextInputListener
	{

	}

	public static class TextInputManager
	{
		public delegate void TextInputEvent(object sender, TextInputEventArgs args);
		public static event TextInputEvent ListenTextInput;
		public static void OnTextInput(object sender, TextInputEventArgs args)
		{
			ListenTextInput?.Invoke(sender, args);
		}
	} 

	public class TextInputLabel : Label
	{
		public Color BackgroundTextColor { get; set; }
		public string BackgroundText { get; set; }

		public TextInput Input { get; set; }

		public bool Selected { get; set; }
		public bool MouseOver { get; set; }

		MouseState prevMouse;

		public TextInputLabel() : base() {

			Input = new TextInput();
			TextInputManager.ListenTextInput += Input.OnTextInput;

			Selected = false;
			BackgroundText = "";
			BackgroundTextColor = Color.Gray;
		}


		private bool IsMouseInside(MouseState mouse)
		{
			return (mouse.X > AbsolutePosition.X && mouse.Y > AbsolutePosition.Y
				&& mouse.X < (AbsolutePosition.X + AbsoluteSize.X)
				&& mouse.Y < (AbsolutePosition.Y + AbsoluteSize.Y));
		}

		public override void Update(GameTime gt)
		{
			Text = Input.DisplayText;
			MouseState mouse = Mouse.GetState();

			MouseOver = IsMouseInside(mouse);

			if (mouse.LeftButton == ButtonState.Pressed)
			{
				if (MouseOver)
					Selected = true;
				else
					Selected = false;
			}
				


			Input.Focused = Selected;
			if (Selected)
			{
				Input.Update(gt);
			}


			base.Update(gt);
		}

		public override void Draw(SpriteBatch sb)
		{
			base.Draw(sb);
			Vector2 textDim = Font.MeasureString(BackgroundText);
			Vector2 TextOutputPosition = AbsolutePosition;

			// Text Alignment
			if (TextXAlign == TextXAlignment.Center)
			{
				TextOutputPosition += new Vector2((AbsoluteSize.X / 2) - (textDim.X / 2), 0);
			}
			if (TextXAlign == TextXAlignment.Right)
			{
				TextOutputPosition += new Vector2(AbsoluteSize.X - textDim.X, 0);
			}

			if (TextYAlign == TextYAlignment.Center)
			{
				TextOutputPosition += new Vector2(0, (AbsoluteSize.Y / 2) - (textDim.Y / 2));
			}
			if (TextYAlign == TextYAlignment.Bottom)
			{
				TextOutputPosition += new Vector2(0, AbsoluteSize.Y - textDim.Y);
			}

			TextOutputPosition.Floor();
			if (Input.inputBuffer == "")
			{
				sb.Print(Font, BackgroundTextColor, TextOutputPosition, BackgroundText);
			}
		}
	}


	public class Label : UIRect
	{

		public ITextProvider Provider { get; set; }
		public string Text { get; set; }
		public Color TextColor { get; set; }
		public SpriteFont Font { get; set; }
		public TextXAlignment TextXAlign { get; set; }
		public TextYAlignment TextYAlign { get; set; }
		public bool TextWrap { get; set; }
		public bool ProviderShowInternal { get; set; }

		public Label() : base()
		{
			Font = Renderer.ComicSans10;
			Text = "Bottom Text";
			TextColor = Color.White;
		}

		public override void Update(GameTime gt)
		{
			if (Provider != null)
			{
				
				if (Provider.InternalText == "")
					Text = "";
				else 
					Text = Provider.DisplayText;

				if (ProviderShowInternal)
					Text = Provider.InternalText;

			}
			base.Update(gt);
		}


		public string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
		{
			string[] words = text.Split(' ');
			StringBuilder sb = new StringBuilder();
			float lineWidth = 0f;
			float spaceWidth = spriteFont.MeasureString(" ").X;


			

			foreach (string word in words)
			{
				Vector2 size = spriteFont.MeasureString(word);

				if (lineWidth + size.X < maxLineWidth)
				{
					sb.Append(word + " ");
					lineWidth += size.X + spaceWidth;
				}
				else
				{
					sb.Append("\n" + word + " ");
					lineWidth = size.X + spaceWidth;
				}
			}

			return sb.ToString();
		}

		public Vector2 GetTextDimensions()
		{
			return Font.MeasureString(Text);
		}

		public override void Draw(SpriteBatch sb)
		{

			

			base.Draw(sb);
			string DisplayedText = Text;
			if (TextWrap)
				DisplayedText = WrapText(Font, Text, AbsoluteSize.X);

			Vector2 textDim =  Font.MeasureString(DisplayedText);
			Vector2 TextOutputPosition = AbsolutePosition;

			// Text Alignment
			if (TextXAlign == TextXAlignment.Center)
			{
				TextOutputPosition += new Vector2((AbsoluteSize.X / 2) - (textDim.X/2), 0);
			}
			if (TextXAlign == TextXAlignment.Right)
			{
				TextOutputPosition += new Vector2(AbsoluteSize.X - textDim.X, 0);
			}

			if (TextYAlign == TextYAlignment.Center)
			{
				TextOutputPosition += new Vector2(0, (AbsoluteSize.Y / 2) - (textDim.Y/2));
			}
			if (TextYAlign == TextYAlignment.Bottom)
			{
				TextOutputPosition += new Vector2(0, AbsoluteSize.Y - textDim.Y);
			}
			TextOutputPosition.Floor();
			sb.Print(Font, TextColor, TextOutputPosition, DisplayedText);
		}
	}
}
