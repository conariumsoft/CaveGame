using Cave;
using CaveGame.Client.UI;
using CaveGame.Core.FileUtil;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace CaveGame.Client.Menu
{
	[Serializable]
	public class GameMenuPersistence: Configuration
	{
		public override void FillDefaults()
		{
			IPAddress = "";
			Username = "";
		}

		public string IPAddress;
		public string Username;

	}


	public class ServerKickedPage : IGameContext
	{
		public Game Game { get; private set; }

		public bool Active { get; set; }

		public string Message { get; set; }

		public void Draw(SpriteBatch sb)
		{

		}

		public void Load()
		{

		}

		public void Unload()
		{

		}

		public void Update(GameTime gt)
		{

		}
	}

	public class HomePage : IGameContext
	{

		GameMenuPersistence persistence;

		public CaveGameGL Game { get; private set; }

		ContentManager MenuContent;

		public bool Active { get; set; }

		Game IGameContext.Game => Game;

		UIRoot CurrentPage;

		UIRoot MainMenu;
		UIRoot SingleplayerMenu;
		UIRoot MultiplayerMenu;
		UIRoot WorldCreationMenu;
		UIRoot CreditsMenu;
		UIRoot StatsMenu;
		UIRoot SettingsMenu;

		TextButton spNewWorldBtn;


		// TODO: change this
		public static SoundEffect buttonBlipSFX;

		public HomePage(CaveGameGL _game)
		{
			persistence = Configuration.Load<GameMenuPersistence>("Mpmenu.xml");
			Game = _game;
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Begin();
			CurrentPage.Draw(sb);
			sb.End();
		}

		private void WhenMouseOverButton(TextButton b, MouseState m) {
			buttonBlipSFX?.Play(1.0f, 1, 0.0f);
		}
		private void WhenMouseOffButton(TextButton b, MouseState m)
		{
			buttonBlipSFX?.Play(0.8f, 1, 0.0f);
		}

		private void OpenUrl(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch
			{
				// hack because of this: https://github.com/dotnet/corefx/issues/10361
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Process.Start("xdg-open", url);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					Process.Start("open", url);
				}
				else
				{
					throw;
				}
			}
		}

		private void ConstructMainMenu()
		{
			MainMenu = new UIRoot(Game.GraphicsDevice);

			TextButton singleplayerButton;
			TextButton multiplayerButton;
			TextButton statsButton;
			TextButton steamPageButton;
			TextButton discordButton;
			TextButton creditsButton;
			TextButton settingsButton;
			TextButton quitButton;

			Label title = new Label
			{
				BGColor = Color.Transparent,
				BorderColor = Color.Transparent,
				Size = new UICoords(0, 0, 0.3f, 0.1f),
				AnchorPoint = new Vector2(0.5f, 0.5f),
				Position = new UICoords(0, 0, 0.5f, 0.05f),
				Parent = MainMenu,
				TextColor = Color.White,
				Text = "CAVE GAME",
				Font = Renderer.Arial20,
				BorderSize = 0,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
			};

			

			MainMenu.Children.Add(title);

			Label copyright = new Label
			{
				BGColor = Color.Transparent,
				BorderColor = Color.Transparent,
				Size = new UICoords(200, 10, 0, 0),
				AnchorPoint = new Vector2(0f, 1f),
				Position = new UICoords(10, 0, 0f, 1f),
				Parent = MainMenu,
				TextColor = Color.White,
				Text = "Copyright Conarium Software 2019-2020",
				BorderSize = 0,
				Font = Renderer.Arial10,
				TextWrap = false,
				TextYAlign = TextYAlignment.Bottom,
				TextXAlign = TextXAlignment.Left,
			};
			MainMenu.Children.Add(copyright);

			Label version = new Label
			{
				BGColor = Color.Transparent,
				BorderColor = Color.Transparent,
				Size = new UICoords(200, 10, 0, 0),
				AnchorPoint = new Vector2(1f, 1f),
				Position = new UICoords(-10, 0, 1f, 1f),
				Parent = MainMenu,
				TextColor = Color.White,
				Text = "Version 2.0.0 Alpha",
				BorderSize = 0,
				Font = Renderer.Arial10,
				TextWrap = false,
				TextYAlign = TextYAlignment.Bottom,
				TextXAlign = TextXAlignment.Right,
			};
			MainMenu.Children.Add(version);

			UIRect buttonList = new UIRect
			{
				Size = new UICoords(180, -20, 0, 0.9f),
				Position = new UICoords(10, 0, 0, 0.1f),
				Parent = MainMenu,
				BGColor = Color.DarkBlue,
			};

			UIListContainer buttons = new UIListContainer
			{
				Padding = 1,
				Parent = buttonList,
			};
			buttonList.Children.Add(buttons);


			singleplayerButton = new TextButton
			{
				TextColor = Color.White,
				Text = "SINGLEPLAYER",
				Font = Renderer.Arial10,
				Size = new UICoords(0, 30, 1f, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			buttons.Children.Add(singleplayerButton);
			singleplayerButton.OnLeftClick += (b, m) => CurrentPage = SingleplayerMenu;

			multiplayerButton = new TextButton
			{
				TextColor = Color.White,
				Text = "MULTIPLAYER",
				Font = Renderer.Arial10,
				Size = new UICoords(0, 30, 1f, 0),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			buttons.Children.Add(multiplayerButton);
			multiplayerButton.OnLeftClick += (b, m) => CurrentPage = MultiplayerMenu;
			//multiplayerButton.OnLeftClick += (b, m) => Game.CurrentGameContext = Game.InWorldContext;

			statsButton = new TextButton
			{
				TextColor = Color.White,
				Text = "STATISTICS",
				Font = Renderer.Arial10,
				Size = new UICoords(0, 30, 1f, 0),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			buttons.Children.Add(statsButton);

			steamPageButton = new TextButton
			{
				TextColor = Color.White,
				Text = "STEAM WORKSHOP",
				Font = Renderer.Arial10,
				Size = new UICoords(0, 30, 1f, 0),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			buttons.Children.Add(steamPageButton);
			steamPageButton.OnLeftClick += (btn, mouse) => OpenUrl(@"https://steamcommunity.com/app/1238250");

			discordButton = new TextButton
			{
				TextColor = Color.White,
				Text = "DISCORD COMMUNITY",
				Font = Renderer.Arial10,
				Size = new UICoords(0, 30, 1f, 0),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			buttons.Children.Add(discordButton);
			discordButton.OnLeftClick += (btn, mouse) => OpenUrl(@"https://discord.gg/6mDmYqs");


			creditsButton = new TextButton
			{
				TextColor = Color.White,
				Text = "CREDITS",
				Font = Renderer.Arial10,
				Size = new UICoords(0, 30, 1f, 0),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			buttons.Children.Add(creditsButton);

			settingsButton = new TextButton
			{
				TextColor = Color.White,
				Text = "SETTINGS",
				Font = Renderer.Arial10,
				Size = new UICoords(0, 30, 1f, 0),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			buttons.Children.Add(settingsButton);

			quitButton = new TextButton
			{
				TextColor = Color.White,
				Text = "EXIT TO DESKTOP",
				Font = Renderer.Arial10,
				Size = new UICoords(0, 30, 1f, 0),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			buttons.Children.Add(quitButton);
			quitButton.OnLeftClick += (btn, mouse) => Game.Exit();


			foreach (TextButton b in buttons.Children)
			{
				b.OnMouseEnter += WhenMouseOverButton;
				b.OnMouseExit += WhenMouseOffButton;
			}


			UIRect homeContent = new UIRect
			{
				Size = new UICoords(-210, -20, 1, 0.9f),
				Position = new UICoords(200, 0, 0, 0.1f),
				Parent = MainMenu,
				BGColor = Color.DarkBlue,
			};
			MainMenu.Children.Add(buttonList);
			MainMenu.Children.Add(homeContent);
		}
		private void ConstructSingleplayerMenu()
		{
			SingleplayerMenu = new UIRoot(Game.GraphicsDevice);

			Label title = new Label
			{
				BGColor = Color.Transparent,
				BorderColor = Color.Transparent,
				Size = new UICoords(0, 0, 0.3f, 0.1f),
				AnchorPoint = new Vector2(0.5f, 0.5f),
				Position = new UICoords(0, 0, 0.5f, 0.05f),
				Parent = SingleplayerMenu,
				TextColor = Color.White,
				Text = "SELECT A WORLD",
				Font = Renderer.Arial20,
				BorderSize = 0,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
			};

			SingleplayerMenu.Children.Add(title);

			var spNewWorldBtn = new TextButton
			{
				Size = new UICoords(180, 30, 0, 0),
				AnchorPoint = new Vector2(0, 0),
				Position = new UICoords(10, 0, 0, 0.1f),
				Parent = SingleplayerMenu,
				Text = "CREATE NEW WORLD",
				Font = Renderer.Arial10,
				BorderSize = 0,
				TextColor = Color.White,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,

				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			spNewWorldBtn.OnMouseEnter += WhenMouseOverButton;
			spNewWorldBtn.OnMouseExit += WhenMouseOffButton;
			spNewWorldBtn.OnLeftClick += (x, y) => CurrentPage = WorldCreationMenu;
			SingleplayerMenu.Children.Add(spNewWorldBtn);

			var spGoBack = new TextButton
			{
				Size = new UICoords(180, 30, 0, 0),
				AnchorPoint = new Vector2(0, 1),
				Position = new UICoords(10, -10, 0, 1f),
				Parent = SingleplayerMenu,
				Text = "CANCEL",
				Font = Renderer.Arial10,
				BorderSize = 0,
				TextColor = Color.White,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,

				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			spGoBack.OnMouseEnter += WhenMouseOverButton;
			spGoBack.OnMouseExit += WhenMouseOffButton;
			spGoBack.OnLeftClick += (b, m) => CurrentPage = MainMenu;
			SingleplayerMenu.Children.Add(spGoBack);


			UIRect savelist = new UIRect
			{
				Size = new UICoords(180, -10, 0, 0.7f),
				Position = new UICoords(10, 0, 0, 0.2f),
				Parent = SingleplayerMenu,
				BGColor = Color.DarkBlue,
			};

			UIListContainer buttons = new UIListContainer
			{
				Padding = 1,
				Parent = savelist,
				ExpandedHeight = 55,
				CompressedHeight = 30,
				ExpandSelected = true,
			};
			savelist.Children.Add(buttons);


			foreach (TextButton b in buttons.Children)
			{
				b.OnMouseEnter += WhenMouseOverButton;
				b.OnMouseExit += WhenMouseOffButton;
			}


			UIRect homeContent = new UIRect
			{
				Size = new UICoords(-210, -10, 1, 0.9f),
				Position = new UICoords(200, 0, 0, 0.1f),
				Parent = SingleplayerMenu,
				BGColor = Color.DarkBlue,
			};
			SingleplayerMenu.Children.Add(savelist);
			SingleplayerMenu.Children.Add(homeContent);
		}
		private void ConstructWorldCreationMenu()
		{

			WorldCreationMenu = new UIRoot(Game.GraphicsDevice);

			Label title = new Label
			{
				BGColor = Color.Transparent,
				BorderColor = Color.Transparent,
				Size = new UICoords(0, 0, 0.3f, 0.1f),
				AnchorPoint = new Vector2(0.5f, 0.5f),
				Position = new UICoords(0, 0, 0.5f, 0.05f),
				Parent = WorldCreationMenu,
				TextColor = Color.White,
				Text = "CREATE NEW WORLD",
				Font = Renderer.Arial20,
				BorderSize = 0,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
			};

			WorldCreationMenu.Children.Add(title);

			var create = new TextButton
			{
				Size = new UICoords(180, 30, 0, 0),
				AnchorPoint = new Vector2(0, 0),
				Position = new UICoords(10, 0, 0, 0.1f),
				Parent = WorldCreationMenu,
				Text = "CONFIRM AND CREATE",
				Font = Renderer.Arial10,
				BorderSize = 0,
				TextColor = Color.White,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,

				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			create.OnMouseEnter += WhenMouseOverButton;
			create.OnMouseExit += WhenMouseOffButton;
			WorldCreationMenu.Children.Add(create);

			var cancel = new TextButton
			{
				Size = new UICoords(180, 30, 0, 0),
				AnchorPoint = new Vector2(0, 1),
				Position = new UICoords(10, -10, 0, 1f),
				Parent = WorldCreationMenu,
				Text = "BACK",
				Font = Renderer.Arial10,
				BorderSize = 0,
				TextColor = Color.White,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,

				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			cancel.OnMouseEnter += WhenMouseOverButton;
			cancel.OnMouseExit += WhenMouseOffButton;
			cancel.OnLeftClick += (b, m) => CurrentPage = SingleplayerMenu;
			WorldCreationMenu.Children.Add(cancel);
		}

		private void OnJoinServer(string address, string username)
		{
			Game.CurrentGameContext = Game.InWorldContext;
			Game.InWorldContext.NetworkUsername = username;
			Game.InWorldContext.ConnectAddress = address;

			persistence.IPAddress = address;
			persistence.Username = username;
			persistence.Save();

		}

		private void ConstructMultiplayerMenu() {
			


			MultiplayerMenu = new UIRoot(Game.GraphicsDevice);

			Label title = new Label
			{
				BGColor = Color.Transparent,
				BorderColor = Color.Transparent,
				Size = new UICoords(0, 0, 0.3f, 0.1f),
				AnchorPoint = new Vector2(0.5f, 0.5f),
				Position = new UICoords(0, 0, 0.5f, 0.05f),
				Parent = MultiplayerMenu,
				TextColor = Color.White,
				Text = "MULTIPLAYER",
				Font = Renderer.Arial20,
				BorderSize = 0,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
			};

			MultiplayerMenu.Children.Add(title);

			UIRect buttonList = new UIRect
			{
				Size = new UICoords(220, -20, 0, 0.8f),
				Position = new UICoords(10, 0, 0, 0.1f),
				Parent = MultiplayerMenu,
				BGColor = Color.DarkBlue,
			};

			UIListContainer buttons = new UIListContainer
			{
				Padding = 1,
				Parent = buttonList,
			};
			buttonList.Children.Add(buttons);
			MultiplayerMenu.Children.Add(buttonList);

			var serverInputBox = new TextInputLabel
			{
				//	Size = new UICoords(200, 25, 0, 0),
				Size = new UICoords(0, 25, 1, 0),
				AnchorPoint = new Vector2(0, 0),
				//Position = new UICoords(20, 0, 0, 0.2f),
				Parent = buttons,
				BGColor = new Color(0.2f, 0.2f, 0.2f),
				BorderColor = Color.DarkBlue,
				//Provider = inputter,
				Font = Renderer.Arial10,
				BackgroundText = "Server Address",
				BackgroundTextColor = Color.Gray,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
			};
			serverInputBox.Input.inputBuffer = persistence.IPAddress;
			buttons.Children.Add(serverInputBox);

			

			var usernameInputBox = new TextInputLabel
			{
				Size = new UICoords(0, 25, 1, 0),
				AnchorPoint = new Vector2(0, 0),
			//	Position = new UICoords(20, 40, 0, 0.2f),
				Parent = buttons,
				//Text = "Test1",
				Font = Renderer.Arial10,
				BGColor = new Color(0.2f, 0.2f, 0.2f),
				BorderColor = Color.DarkBlue,
				BackgroundText = "Nickname",
				BackgroundTextColor = Color.Gray,
				//Provider = inputter2,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
			};
			usernameInputBox.Input.inputBuffer = persistence.Username;
			usernameInputBox.Input.BlacklistedCharacters.Add(' ');
			buttons.Children.Add(usernameInputBox);

			var connect = new TextButton
			{
				Size = new UICoords(0, 30, 1, 0),
				//AnchorPoint = new Vector2(0, 1),
				//Position = new UICoords(10, -10, 0, 1f),
				Parent = buttons,
				Text = "CONNECT",
				Font = Renderer.Arial10,
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
			buttons.Children.Add(connect);


			var back = new TextButton
			{
				Size = new UICoords(180, 30, 0, 0),
				AnchorPoint = new Vector2(0, 1),
				Position = new UICoords(10, -10, 0, 1f),
				Parent = MultiplayerMenu,
				Text = "BACK",
				Font = Renderer.Arial10,
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
			back.OnLeftClick += (b, m) => CurrentPage = MainMenu;
			MultiplayerMenu.Children.Add(back);
		}

		private void ConstructCreditsMenu() { }
		private void ConstructStatsMenu() { }
		private void ConstructSettingsMenu()
		{

		}

		public void Load()
		{
			MenuContent = new ContentManager(Game.Services, Game.Content.RootDirectory);

			buttonBlipSFX = MenuContent.Load<SoundEffect>("Sound/click1");

			ConstructMainMenu();
			ConstructSingleplayerMenu();
			ConstructWorldCreationMenu();
			ConstructMultiplayerMenu();


			CurrentPage = MainMenu;
		}

		public void Unload()
		{
			MenuContent.Unload();
		}


		public void Update(GameTime gt)
		{

			CurrentPage.Update(gt);

			KeyboardState keyboard = Keyboard.GetState();

			if (CurrentPage == SingleplayerMenu)
			{
				if (keyboard.IsKeyDown(Keys.Escape))
				{
					CurrentPage = MainMenu;
				}
			}
		}
	}
}
