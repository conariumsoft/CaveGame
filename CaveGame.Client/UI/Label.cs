﻿using CaveGame.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CaveGame.Client.UI
{
	


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

		public static string WrapText(this string text, SpriteFont spriteFont, float maxLineWidth)
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


		public TextInputLabel(NLua.Lua state, NLua.LuaTable table) : base(state, table)
		{
			Input = new TextInput();
			TextInputManager.ListenTextInput += Input.OnTextInput;

			Selected = false;
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

		public override void Draw(GraphicsEngine GFX)
		{
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

			
			if (Input.SpecialSelection)
			{
				base.Draw(GFX, true);
				var beforeText = Input.GetScissorTextBefore();
				var middleText = Input.GetScissorTextDuring();
				var afterText = Input.GetScissorTextAfter();
				var start = Font.MeasureString(beforeText);
				var end = Font.MeasureString(middleText);

				//sb.Rect(Color.Blue, TextOutputPosition + new Vector2(start.X, 0), end);

				// first section
				GFX.Text(Font, beforeText, TextOutputPosition, TextColor);
				GFX.Text(Font, middleText, TextOutputPosition + new Vector2(start.X, 0),  Color.Black);
				GFX.Text(Font, afterText , TextOutputPosition + new Vector2(start.X + end.X, 0), TextColor);
			} else
			{
				base.Draw(GFX, false);
				//sb.Print(Font, TextColor, TextOutputPosition, Input.InputBuffer);
			}
			
			if (Input.InputBuffer == "")
			{
				GFX.Text(Font, BackgroundText , TextOutputPosition, BackgroundTextColor);
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

		public bool TextWrapping { get; private set; }
		public int TextWrappingCount { get; private set; }


		public Label(NLua.Lua state, NLua.LuaTable table) : this() {
            this.InitFromLuaPropertyTable(state, table);
		}

		public Label() : base()
        {
			Font = GraphicsEngine.Instance.Fonts.Arial10;
			Text = "Label";
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

			TextWrappingCount = 0;



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
					TextWrappingCount++;
				}
			}

			return sb.ToString();
		}

		public Vector2 GetTextDimensions()
		{
			return Font.MeasureString(Text);
		}

		public void Draw(GraphicsEngine GFX, bool TextOverride)
		{
			base.Draw(GFX);
			if (TextOverride)
				return;



			string DisplayedText = "";
			if (Text!=null)
				DisplayedText = Text;
			if (TextWrap)
				DisplayedText = WrapText(Font, Text, AbsoluteSize.X);

			Vector2 textDim = Font.MeasureString(DisplayedText);
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
			GFX.Text(Font, DisplayedText, TextOutputPosition, TextColor);
		}

		public override void Draw(GraphicsEngine GFX)
		{
			Draw(GFX, false);
		}
	}
}
