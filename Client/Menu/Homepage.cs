using CaveGame;
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

	public class HomePage : IGameContext
	{
		static string[] UpdateLogInfo =
		{
			">>Open Multiplayer Test",
			"<Version 2.1.0 - October 31st, 2k20",
			"",
			"Thank you for playing! Expect lots of bugs, weird issues, and missing content.",
			"",
			"Stay tuned for frequent updates and devlogs!",
			"",
			"If you'd like to get in touch, click the discord button to the left.",
			"",
			"~ Conarium Software"
		};


		public CaveGameGL Game { get; private set; }

		public bool Active { get; set; }

		Game IGameContext.Game => Game;

		UIRoot CurrentPage;
		UIRoot MainMenu;
		UIRoot SingleplayerMenu;
		UIRoot WorldCreationMenu;

		int buttonHeight = 40;

		TextButton spNewWorldBtn;

		public HomePage(CaveGameGL _game)
		{
			
			Game = _game;
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Begin();

			CurrentPage.Draw(sb);
			//sb.Draw(GameTextures.TitleScreen, title.AbsolutePosition, null, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
			sb.End();
		}

		Label title;
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

			title = new Label
			{
				BGColor = Color.Transparent,
				BorderColor = Color.Transparent,
				Size = new UICoords(220, -20, 0f, 0.1f),
				AnchorPoint = new Vector2(0f, 0f),
				Position = new UICoords(10, 10, 0f, 0.00f),
				Parent = MainMenu,
				TextColor = Color.White,
				Text = "CAVE GAME",
				Font = GameFonts.Arial30,
				BorderSize = 0,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
			};

			//MainMenu.Children.Add(title);

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
				Font = GameFonts.Arial10,
				TextWrap = false,
				TextYAlign = TextYAlignment.Bottom,
				TextXAlign = TextXAlignment.Left,
			};
			//MainMenu.Children.Add(copyright);

			Label version = new Label
			{
				BGColor = Color.Transparent,
				BorderColor = Color.Transparent,
				Size = new UICoords(200, 10, 0, 0),
				AnchorPoint = new Vector2(1f, 1f),
				Position = new UICoords(-10, 0, 1f, 1f),
				Parent = MainMenu,
				TextColor = Color.White,
				Text = "Multiplayer Open Beta (v2.1.0)",
				BorderSize = 0,
				Font = GameFonts.Arial10,
				TextWrap = false,
				TextYAlign = TextYAlignment.Bottom,
				TextXAlign = TextXAlignment.Right,
			};
			//MainMenu.Children.Add(version);

			UIRect buttonList = new UIRect
			{
				Size = new UICoords(220, 350, 0, 0f),
				AnchorPoint = new Vector2(0, 1),
				Position = new UICoords(10, -10, 0, 1f),
				Parent = MainMenu,
				BGColor = Color.Transparent,
			};

			UIListContainer buttons = new UIListContainer
			{
				Padding = 5,
				Parent = buttonList,
			};
			//buttonList.Children.Add(buttons);


			singleplayerButton = new TextButton
			{
				TextColor = Color.Gray,
				Text = "SINGLEPLAYER",
				Font = GameFonts.Arial14,
				Size = new UICoords(0, -10, 1f, 0.125f),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.05f, 0.05f, 0.05f),
				SelectedBGColor = new Color(0.05f, 0.05f, 0.05f),
			};
			//buttons.Children.Add(singleplayerButton);
			//singleplayerButton.OnLeftClick += (b, m) => CurrentPage = SingleplayerMenu;

			multiplayerButton = new TextButton
			{
				TextColor = Color.White,
				Text = "MULTIPLAYER",
				Font = GameFonts.Arial14,
				Size = new UICoords(0, -10, 1f, 0.125f),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			//buttons.Children.Add(multiplayerButton);
			multiplayerButton.OnLeftClick += (b, m) => Game.CurrentGameContext = Game.MultiplayerPageContext;
			//multiplayerButton.OnLeftClick += (b, m) => Game.CurrentGameContext = Game.InWorldContext;

			statsButton = new TextButton
			{
				TextColor = Color.Gray,
				Text = "STATISTICS",
				Font = GameFonts.Arial14,
				Size = new UICoords(0, -10, 1f, 0.125f),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.05f, 0.05f, 0.05f),
				SelectedBGColor = new Color(0.05f, 0.05f, 0.05f),
			};
			//buttons.Children.Add(statsButton);

			steamPageButton = new TextButton
			{
				TextColor = Color.White,
				Text = "STEAM WORKSHOP",
				Font = GameFonts.Arial14,
				Size = new UICoords(0, -10, 1f, 0.125f),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			//buttons.Children.Add(steamPageButton);
			steamPageButton.OnLeftClick += (btn, mouse) => CaveGame.Core.SystemUtil.OpenUrl(@"https://steamcommunity.com/app/1238250");

			discordButton = new TextButton
			{
				TextColor = Color.White,
				Text = "DISCORD COMMUNITY",
				Font = GameFonts.Arial14,
				Size = new UICoords(0, -10, 1f, 0.125f),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			//buttons.Children.Add(discordButton);
			discordButton.OnLeftClick += (btn, mouse) => CaveGame.Core.SystemUtil.OpenUrl(@"https://discord.gg/6mDmYqs");


			creditsButton = new TextButton
			{
				TextColor = Color.White,
				Text = "CREDITS",
				Font = GameFonts.Arial14,
				Size = new UICoords(0, -10, 1f, 0.125f),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			creditsButton.OnLeftClick += (btn, mouse) => Game.CurrentGameContext = Game.CreditsContext;
			//buttons.Children.Add(creditsButton);
			settingsButton = new TextButton
			{
				TextColor = Color.White,
				Text = "SETTINGS",
				Font = GameFonts.Arial14,
				Size = new UICoords(0, -10, 1f, 0.125f),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			settingsButton.OnLeftClick += (btn, mouse) => Game.CurrentGameContext = Game.SettingsContext;
			//buttons.Children.Add(settingsButton);

			quitButton = new TextButton
			{
				TextColor = Color.White,
				Text = "EXIT TO DESKTOP",
				Font = GameFonts.Arial14,
				Size = new UICoords(0, -10, 1f, 0.125f),
				Position = new UICoords(0, 30, 0, 0),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = buttons,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			//buttons.Children.Add(quitButton);
			quitButton.OnLeftClick += (btn, mouse) => Game.Exit();


			UIRect homeContent = new UIRect
			{
				Size = new UICoords(350, -70, 0, 1f),
				Position = new UICoords(-20, 20, 1, 0f),
				AnchorPoint = new Vector2(1, 0),
				Parent = MainMenu,
				BGColor = new Color(0.05f, 0.05f, 0.05f)*0.8f,
				BorderSize = 2,
				BorderColor = new Color(0.1f, 0.1f, 0.1f),
				BorderEnabled = true,
			};

			UIListContainer updateLog = new UIListContainer
			{
				Padding = 2,
				Parent = homeContent,
			};

			foreach(string text in UpdateLogInfo)
			{
				string displayedText = text;
				SpriteFont font = GameFonts.Arial12;
				int size = 20;
				if (text.StartsWith(">>"))
				{
					font = GameFonts.Arial16;
					size = 30;
					displayedText = text.Replace(">>", "");
				}
				else if (text.StartsWith(">"))
				{
					font = GameFonts.Arial14;
					size = 24;
					displayedText = text.Replace(">", "");
				}else if (text.StartsWith("<"))
				{
					font = GameFonts.Arial10;
					size = 10;
					displayedText = text.Replace("<", "");
				} else if (text.StartsWith("-"))
				{
					font = GameFonts.Arial10;
					size = 12;
					//displayedText = text.Replace("-", " ");
				}


				var label = new Label
				{
					TextColor = UITheme.SmallButtonTextColor,
					Text = displayedText,
					Font = font,
					Size = new UICoords(1, size, 1.0f, 0),
					BGColor = Color.Black * 0.0f,
					TextXAlign = TextXAlignment.Left,
					Parent = updateLog,
					TextWrap = true,
				};
			}

			//MainMenu.Children.Add(buttonList);
			//MainMenu.Children.Add(homeContent);
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
				Font = GameFonts.Arial20,
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
				Font = GameFonts.Arial10,
				BorderSize = 0,
				TextColor = Color.White,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,

				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			spNewWorldBtn.OnLeftClick += (x, y) => CurrentPage = WorldCreationMenu;
			SingleplayerMenu.Children.Add(spNewWorldBtn);

			var spGoBack = new TextButton
			{
				Size = new UICoords(180, 30, 0, 0),
				AnchorPoint = new Vector2(0, 1),
				Position = new UICoords(10, -10, 0, 1f),
				Parent = SingleplayerMenu,
				Text = "CANCEL",
				Font = GameFonts.Arial10,
				BorderSize = 0,
				TextColor = Color.White,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,

				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
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
				Font = GameFonts.Arial20,
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
				Font = GameFonts.Arial10,
				BorderSize = 0,
				TextColor = Color.White,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,

				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			WorldCreationMenu.Children.Add(create);

			var cancel = new TextButton
			{
				Size = new UICoords(180, 30, 0, 0),
				AnchorPoint = new Vector2(0, 1),
				Position = new UICoords(10, -10, 0, 1f),
				Parent = WorldCreationMenu,
				Text = "BACK",
				Font = GameFonts.Arial10,
				BorderSize = 0,
				TextColor = Color.White,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,

				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			cancel.OnLeftClick += (b, m) => CurrentPage = SingleplayerMenu;
			WorldCreationMenu.Children.Add(cancel);
		}

		public void Load()
		{

			

			ConstructMainMenu();
			ConstructSingleplayerMenu();
			ConstructWorldCreationMenu();

			CurrentPage = MainMenu;
		}

		public void Unload()
		{

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
