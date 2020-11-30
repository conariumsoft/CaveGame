using CaveGame;
using CaveGame.Client.UI;
using CaveGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using NLua;
using System.IO;
using System.Diagnostics;
using DataManagement;
using CaveGame.Core.LuaInterop;

namespace CaveGame.Client.Menu
{
	public static class EnumExtensions
    {
		public static int ToInt(this GameChatSize e)
        {
			return (int)e;
        }
    }
	public class MenuManager : IGameContext
	{

		public CaveGameGL Game { get; private set; }

		public bool Active { get; set; }

		Microsoft.Xna.Framework.Game IGameContext.Game => Game;

		public Dictionary<string, UIRoot> Pages { get; set; }

		private UIRoot _currentPage;
		public UIRoot CurrentPage {
			get => _currentPage;
			set
            {
				_currentPage?.OnUnload.Invoke(null);
				_currentPage = value;
				_currentPage.OnLoad.Invoke(null);
            }
		}

		public string TimeoutMessage { get; set; }


		public MenuManager(CaveGameGL _game)
		{
			Pages = new Dictionary<string, UIRoot>();
			Game = _game;
		}

		public void Draw(GraphicsEngine GFX)
		{
			GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap);
			CurrentPage?.Draw(GFX);
			GFX.End();
		}


		private void LoadMenuScripts()
        {
			Lua luastate = new Lua();
			luastate.LoadCLRPackage();
			luastate["_G.game"] = Game;
			luastate["_G.menumanager"] = this;
			//luastate["buttonlist"] = buttons;
			luastate["_G.script"] = luastate;
			luastate.DoString(LuaSnippets.UtilityFunctions);
			luastate.DoFile(Path.Combine("Assets", "Scripts", "menu.lua"));
		}


		// Temporary solution until I figure out generics within lua
		public Slider<SliderIndex<int>> GetFPSCapSlider()
        {
			return new UI.Slider<SliderIndex<int>>
			{
				DataSet = GameSettings.FramerateCapSliderOptions,
				Size = new UICoords(0, 25, 0.5f, 0),
				AnchorPoint = new Vector2(0.0f, 0.0f),
				UnselectedBGColor = new Color(0.6f, 0.6f, 0.6f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Scrubber = new Scrubber{ Width = 20 },
				BGColor = new Color(0.25f, 0.25f, 0.25f),
			};
		}

		public Slider<SliderIndex<GameChatSize>> GetChatSizeSlider()
        {
			return new UI.Slider<SliderIndex<GameChatSize>>
			{
				DataSet = GameSettings.ChatSizeSliderOptions,
				Size = new UICoords(0, 25, 0.5f, 0),
				AnchorPoint = new Vector2(0.0f, 0.0f),
				UnselectedBGColor = new Color(0.6f, 0.6f, 0.6f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Scrubber = new Scrubber { Width = 20 },
				BGColor = new Color(0.25f, 0.25f, 0.25f),
			};
		}
/*
		private void ConstructSingleplayerMenu()
		{
			SingleplayerMenu = new UIRoot();

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

			WorldCreationMenu = new UIRoot();

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
*/
		public void Load()
		{

			LoadMenuScripts();

			//ConstructSingleplayerMenu();
			//ConstructWorldCreationMenu();

			//CurrentPage = MainMenu;
		}

		public void Unload()
		{

		}


		public void Update(GameTime gt)
		{

			CurrentPage?.Update(gt);
		}
	}
}
