using CaveGame.Client.UI;
using CaveGame.Core.FileUtil;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CaveGame.Client.Menu
{
	[Serializable]
	public class MultiplayerInputHistory : Configuration
	{
		public override void FillDefaults()
		{
			IPAddress = "";
			Username = "";
		}

		public string IPAddress;
		public string Username;

	}

	[Serializable]
	public class ServerHistoryPersistence : Configuration
	{
		public override void FillDefaults()
		{
			IPAddress = new List<string>();
		}

		public List<string> IPAddress;

	}


	public class Multiplayer : IGameContext
	{
		ServerHistoryPersistence serverHistory;
		MultiplayerInputHistory inputHistory;

		public CaveGameGL Game { get; set; }
		Microsoft.Xna.Framework.Game IGameContext.Game => Game;

		public bool Active { get; set; }


		public Multiplayer(CaveGameGL _game)
		{
			inputHistory = Configuration.Load<MultiplayerInputHistory>("mphistory.xml");
			serverHistory = Configuration.Load<ServerHistoryPersistence>("serverhistory.xml");
			Game = _game;
		}

		private void WhenMouseOverButton(TextButton b, MouseState m)
		{
			GameSounds.MenuBlip?.Play(1.0f, 1, 0.0f);
		}
		private void WhenMouseOffButton(TextButton b, MouseState m)
		{
			GameSounds.MenuBlip?.Play(0.8f, 1, 0.0f);
		}

		UIRoot MultiplayerPage;

		private void Timeout(string message)
		{
			Game.CurrentGameContext = Game.TimeoutContext;
			Game.TimeoutContext.Message = message;
		}

		private void OnJoinServer(string address, string username)
		{
			if (address.Length == 0)
			{
				Timeout("Server Address is empty! Please enter a valid IP Address!");
				return;
			}

			if (username.Length == 0)
			{
				Timeout("Please enter a nickname!");
				return;
			}

			if (IPAddress.TryParse(address, out IPAddress _) == false)
			{
			//	Timeout("Server Address is not valid!");
			//	return;
			}

			
			Game.CurrentGameContext = Game.InWorldContext;
			Game.InWorldContext.NetworkUsername = username;
			Game.InWorldContext.ConnectAddress = address;

			inputHistory.IPAddress = address;
			inputHistory.Username = username;
			inputHistory.Save();

		}


		private void ConstructUIElements()
		{
			// what would be epic:
			// ability to enforce a ui style on
			// multiple objects

			MultiplayerPage = new UIRoot(Game.GraphicsDevice);

			Label title = new Label
			{
				BGColor = Color.Transparent,
				BorderColor = Color.Transparent,
				Size = new UICoords(0, 0, 0.3f, 0.1f),
				AnchorPoint = new Vector2(0.5f, 0.5f),
				Position = new UICoords(0, 0, 0.5f, 0.05f),
				Parent = MultiplayerPage,
				TextColor = Color.White,
				Text = "MULTIPLAYER",
				Font = GameFonts.Arial20,
				BorderSize = 0,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
			};

			UIRect buttonList = new UIRect
			{
				Size = new UICoords(220, -20, 0, 0.8f),
				Position = new UICoords(10, 0, 0, 0.1f),
				Parent = MultiplayerPage,
				BGColor = Color.DarkBlue,
			};

			UIListContainer buttons = new UIListContainer
			{
				Padding = 1,
				Parent = buttonList,
			};
			//buttonList.Children.Add(buttons);
			//MultiplayerMenu.Children.Add(buttonList);

			var serverInputBox = new TextInputLabel
			{
				//	Size = new UICoords(200, 25, 0, 0),
				Size = new UICoords(0, 30, 1, 0),
				AnchorPoint = new Vector2(0, 0),
				//Position = new UICoords(20, 0, 0, 0.2f),
				Parent = buttons,
				BGColor = new Color(0.2f, 0.2f, 0.3f),
				BorderColor = Color.DarkBlue,
				//Provider = inputter,
				Font = GameFonts.Arial12,
				BackgroundText = "Server Address",
				BackgroundTextColor = Color.Gray,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
			};
			serverInputBox.Input.InputBuffer = inputHistory.IPAddress;
			serverInputBox.Input.Focused = false;
			serverInputBox.Input.CursorPosition = inputHistory.IPAddress.Length;
			//buttons.Children.Add(serverInputBox);

			var usernameInputBox = new TextInputLabel
			{
				Size = new UICoords(0, 30, 1, 0),
				AnchorPoint = new Vector2(0, 0),
				//	Position = new UICoords(20, 40, 0, 0.2f),
				Parent = buttons,
				//Text = "Test1",
				Font = GameFonts.Arial12,
				BGColor = new Color(0.2f, 0.2f, 0.3f),
				BorderColor = Color.DarkBlue,
				BackgroundText = "Nickname",
				BackgroundTextColor = Color.Gray,
				//Provider = inputter2,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
			};
			usernameInputBox.Input.InputBuffer = inputHistory.Username;
			usernameInputBox.Input.BlacklistedCharacters.Add(' ');
			usernameInputBox.Input.Focused = false;
			//buttons.Children.Add(usernameInputBox);

			var connect = new TextButton
			{
				Size = new UICoords(0, 35, 1, 0),
				//AnchorPoint = new Vector2(0, 1),
				//Position = new UICoords(10, -10, 0, 1f),
				Parent = buttons,
				Text = "CONNECT",
				Font = GameFonts.Arial14,
				BorderSize = 0,
				TextColor = Color.White,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,

				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			connect.OnMouseEnter += WhenMouseOverButton;
			connect.OnMouseExit += WhenMouseOffButton;
			connect.OnLeftClick += (b, m) => OnJoinServer(serverInputBox.Input.InternalText, usernameInputBox.Input.InternalText);
			//buttons.Children.Add(connect);

			var back = new TextButton
			{
				Size = new UICoords(0, 30, 1, 0),
				//AnchorPoint = new Vector2(0, 1),
				//Position = new UICoords(10, -10, 0, 1f),
				Parent = buttons,
				Text = "BACK",
				Font = GameFonts.Arial14,
				BorderSize = 0,
				TextColor = Color.White,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,

				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			back.OnMouseEnter += WhenMouseOverButton;
			back.OnMouseExit += WhenMouseOffButton;
			back.OnLeftClick += (b, m) => Game.CurrentGameContext = Game.HomePageContext;
			//MultiplayerMenu.Children.Add(back);
		}


		public void Draw(SpriteBatch sb)
		{
			sb.Begin();
			MultiplayerPage.Draw(sb);
			sb.End();
		}

		public void Load()
		{
			if (MultiplayerPage == null)
				ConstructUIElements();
		}

		public void Unload()
		{
			//throw new NotImplementedException();
		}

		public void Update(GameTime gt)
		{
			MultiplayerPage.Update(gt);
		}
	}
}
