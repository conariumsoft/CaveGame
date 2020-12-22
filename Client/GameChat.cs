using CaveGame.Client.UI;
using CaveGame.Core;
using CaveGame.Core.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CaveGame.Client
{

	public enum GameChatSize
	{
		Small,
		Normal,
		Large
	}

	public class GameChat
	{
		public const int OpenMessageHistory = 15;
		public const int ClosedMessageHistory = 5;
		public bool Open { get; set; }
		public GameChatSize ChatSize => GameSettings.CurrentSettings.ChatSize;

		GameClient Client;
		public GameChat(GameClient client) {

			inputBox = new TextInput();
			inputBox.ClearOnReturn = true;
			inputBox.Handler += SendChatMessage;

			MessageHistory = new List<Message>();
			Open = false;
			TextInputManager.ListenTextInput += OnTextInput;
			Client = client;
		}


		public void SendChatMessage(object sender, string message)
        {
			Open = false;
			Client.Send(new ClientChatMessagePacket(message));
		}

		public void OnTextInput(object sender, TextInputEventArgs args)
		{
			if (Client.Game.Console.Open)
				return;

			if (args.Key == Keys.Escape || args.Key == Keys.Tab || args.Key == Keys.OemTilde)
				return;

			if (!Open)
				return;

			

			inputBox.OnTextInput(sender, args);

		}

		TextInput inputBox;

		public List<Message> MessageHistory;

		public void AddMessage(string message, Color color)
		{
			MessageHistory.Add(new Message(message, color));
		}
		public void AddMessage(string message)
		{
			MessageHistory.Add(new Message(message, Color.White));
		}

		public void Update(GameTime gt)
		{
			KeyboardState kb = Keyboard.GetState();
			if (kb.IsKeyDown(Keys.T) && Open == false && Client.Game.Console.Open == false)
			{
				Open = true;
				return;
			}

			inputBox.Focused = Open;
			if (Open)
			{
				inputBox.Update(gt);
			}
		}

		public void Draw(GraphicsEngine GFX)
		{
			int textHeight = 14;
			SpriteFont font = GFX.Fonts.Arial10;

			if (ChatSize == GameChatSize.Small)
			{

			}

			if (ChatSize == GameChatSize.Normal)
			{
				font = GFX.Fonts.Arial14;
				textHeight = 20;

			}

			if (ChatSize == GameChatSize.Large)
			{
				font = GFX.Fonts.Arial16;
				textHeight = 24;
			}



			#region Draw box
			Color backgroundColor = new Color(0, 0, 0, 0.75f);
			Vector2 chatsize = new Vector2(500, 15* textHeight);
			Vector2 chatpos = new Vector2(0, GFX.WindowSize.Y - 30 - (15 * textHeight));

			Color inputBoxColor = new Color(0.15f, 0.15f, 0.25f);
			Vector2 inputBoxPosition = new Vector2(0, GFX.WindowSize.Y - 30);
			Vector2 inputBoxSize = new Vector2(chatsize.X, 20);
			GFX.Begin();
			if (Open)
			{
				GFX.Rect(backgroundColor, chatpos, chatsize);
				GFX.Rect(inputBoxColor, inputBoxPosition, inputBoxSize);
				GFX.OutlineRect(new Color(0.0f, 0.0f, 0.0f), chatpos, chatsize);
			}
			#endregion

			//Draw message history
			int iter = MessageHistory.Count;
			int count = 0;
			lock (MessageHistory)
				foreach (Message message in MessageHistory.ToArray().Reverse())
				{
					var pos = new Vector2(0, GFX.WindowSize.Y - 20 - ((count+1)*textHeight));
					GFX.Text(font, message.Text, pos, message.TextColor);
					iter--;
					count++;

					if (Open && count > OpenMessageHistory)
						break;

					if (!Open && count > ClosedMessageHistory)
						break;
				}

			if (Open)
			{
				//TODO:  duplicate code occurs in Label.cs, consider refactoring
				if (inputBox.SpecialSelection)
				{
					var beforeText = inputBox.GetScissorTextBefore();
					var middleText = inputBox.GetScissorTextDuring();
					var afterText = inputBox.GetScissorTextAfter();
					var start = font.MeasureString(beforeText);
					var end = font.MeasureString(middleText);
					//var final = font.MeasureString(afterText);

					//Debug.WriteLine("{0}|{1}|{2}", beforeText, middleText, afterText);

					GFX.Rect(Color.Blue, inputBoxPosition + new Vector2(start.X, 0), end);

					// first section
					GFX.Text(font, beforeText , inputBoxPosition, Color.White);
					GFX.Text(font, middleText , inputBoxPosition + new Vector2(start.X, 0), Color.Black);
					GFX.Text(font, afterText, inputBoxPosition + new Vector2(start.X + end.X, 0), Color.White);
				} else
				{
					GFX.Text(font, inputBox.DisplayText, inputBoxPosition, new Color(1.0f, 1.0f, 1.0f));
				}
				
			}
			

			GFX.End();
		}
	}
}
