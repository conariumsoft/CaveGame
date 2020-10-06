//#define SERVER

using CaveGame.Client.UI;
using CaveGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CaveGame.Client
{
	public class GameChat
	{
		public bool Open { get; set; }
		public GameChat(GameClient client) {

			inputBox = new TextInput();
			inputBox.ClearOnReturn = true;
			inputBox.Handler += client.SendChatMessage;

			MessageHistory = new List<Message>();
			Open = false;
			TextInputManager.ListenTextInput += OnTextInput;
		}

		public void OnTextInput(object sender, TextInputEventArgs args)
		{

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
			if (kb.IsKeyDown(Keys.T) && Open == false)
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

		public void Draw(SpriteBatch spriteBatch)
		{
			#region Draw box
			Color backgroundColor = new Color(0, 0, 0, 0.75f);
			Vector2 chatsize = new Vector2(400, 15*14);
			Vector2 chatpos = new Vector2(0, GameGlobals.Height - 20 - (15 * 14));

			Color inputBoxColor = new Color(0.15f, 0.15f, 0.25f);
			Vector2 inputBoxPosition = new Vector2(0, GameGlobals.Height-20);
			Vector2 inputBoxSize = new Vector2(chatsize.X, 20);
			spriteBatch.Begin();
			if (Open)
			{
				
				spriteBatch.Rect(backgroundColor, chatpos, chatsize);
				spriteBatch.Rect(inputBoxColor, inputBoxPosition, inputBoxSize);
				spriteBatch.OutlineRect(new Color(0.0f, 0.0f, 0.0f), chatpos, chatsize);
			}
			#endregion

			//Draw message history
			int iter = MessageHistory.Count;
			int count = 0;
			lock (MessageHistory)
				foreach (Message message in MessageHistory.ToArray().Reverse())
				{
					var pos = new Vector2(0, GameGlobals.Height - 20 - ((count+1)*14));
					spriteBatch.Print(message.TextColor, pos, message.Text);
					iter--;
					count++;

					if (Open && count > 14)
						break;

					if (!Open && count > 5)
						break;
				}

			if (Open)
			{
				spriteBatch.Print(new Color(1.0f, 1.0f, 1.0f), inputBoxPosition, inputBox.DisplayText);
			}
			

			spriteBatch.End();
		}
	}
}
