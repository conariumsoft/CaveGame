using CaveGame.Core;
using CaveGame.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using CaveGame.Client.Menu;
using CaveGame.Client.UI;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using CaveGame.Core.FileUtil;
using Steamworks;

namespace CaveGame.Client
{
	public static class GameGlobals
	{
		public static int Width;
		public static int Height;
		public static GraphicsDevice GraphicsDevice;
	}

	public class CaveGameGL : Game
	{
		public static XGameSettings GameSettings { get; set; }

		public IGameContext CurrentGameContext { get; set; }
		private IGameContext PreviousGameContext { get; set; }

		#region Game States
		public GameClient InWorldContext;
		public Menu.HomePage HomePageContext;
		public Menu.Multiplayer MultiplayerPageContext;
		public Menu.Settings SettingsContext;
		public Menu.TimeoutMenu TimeoutContext;
		public Menu.Credits CreditsContext;
		#endregion

		public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
		public SpriteBatch SpriteBatch { get; private set; }
		#region GameComponents
		public CommandBar Console { get; private set; }
		public FPSGraph FPSGraph { get; private set; }
		public FrameCounter FPSCounter { get; private set; }
		#endregion

		public static float ClickTimer;

		public SteamManager SteamManager { get; private set; }

		public void OnSetFPSLimit(int limit)
		{
			if (limit == 0)
			{
				this.IsFixedTimeStep = false;
			} else
			{
				this.IsFixedTimeStep = true;
				this.TargetElapsedTime = TimeSpan.FromSeconds(1d / (double)limit);
			}
		}

		public void OnSetFullscreen(bool full)
		{

			this.GraphicsDeviceManager.IsFullScreen = full;
			if (full == true)
			{
				GraphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
				GraphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			}
			else
			{
				GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
				GraphicsDeviceManager.PreferredBackBufferHeight = 720;
			}
			GraphicsDeviceManager.ApplyChanges();
		}


		public CaveGameGL()
		{
			GameSettings = Configuration.Load<XGameSettings>("settings.xml", true);
			SteamManager = new SteamManager(this);
			
			//bool statsSuccess = SteamUserStats.RequestCurrentStats();
			GraphicsDeviceManager = new GraphicsDeviceManager(this) 
			{
				PreferredBackBufferWidth = 1280,
				PreferredBackBufferHeight = 720,
				SynchronizeWithVerticalRetrace = false,
				IsFullScreen = false,
				PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
			};
			
			//IsFixedTimeStep = true;
			//TargetElapsedTime = TimeSpan.FromSeconds(1d / 144d);
			IsMouseVisible = true;
			//TargetElapsedTime = TimeSpan.FromSeconds(1d / (double)GameSettingsData.FPSSlider[CaveGameGL.GameSettings.FPSCapIndex].Value);
			Content.RootDirectory = "Content";
			Window.AllowUserResizing = true;
			Window.AllowAltF4 = true;

			OnSetFPSLimit(GameSettings.FPSLimit);
		}


		void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			GameGlobals.Width = Window.ClientBounds.Width;
			GameGlobals.Height = Window.ClientBounds.Height;
			if (InWorldContext != null)
			{
				InWorldContext.Camera.Bounds = Window.ClientBounds;
				InWorldContext.Camera._screenSize = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);
			}
			
		}

		protected void CommandBarEvent(CommandBar sender, Command command, params string[] args)
		{
			//InWorldContext.myPlayer.God = !InWorldContext.myPlayer.God;
			if (command.Keyword == "god")
			{
				InWorldContext.myPlayer.God = !InWorldContext.myPlayer.God;
				return;
			}
			if (command.Keyword == "teleport")
			{

				if (args.Length < 2)
				{
					sender.Out("Please provide a valid coordinate!", Color.Red);

					return;
				}

				bool successX = Int32.TryParse(args[0], out int x);

				if (!successX)
				{
					sender.Out(String.Format("Invalid parameter: X {0}", args[0]), Color.Red);
					// TODO: yell at player
					return;
				}

				bool successY = Int32.TryParse(args[1], out int y);

				if (!successY)
				{
					sender.Out("Invalid parameter: Y", Color.Red);
					// TODO: yell at player
					return;
				}

				if (InWorldContext.myPlayer != null) {
					InWorldContext.myPlayer.NextPosition = new Vector2(x, y);
				}
			}
		}
		protected override void Initialize()
		{
			Window.TextInput += TextInputManager.OnTextInput;
			
			Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

			
			OnSetFullscreen(GameSettings.Fullscreen);

			Console = new CommandBar(this);
			Console.BindCommandInformation(new Command("test", "it does a thing", new List<string>{"argA", "argB", "argC"}));
			Console.BindCommandInformation(new Command("teleport", "", new List<string> { "x", "y" }));
			Console.BindCommandInformation(new Command("god", "AAOKSOKADFOS", new List<string> {}));
			Console.Handler += CommandBarEvent;
			Components.Add(Console);
			

			FPSCounter = new FrameCounter(this);
			Components.Add(FPSCounter);


			FPSGraph = new FPSGraph(this, 25, 500);
			Components.Add(FPSGraph);

			SteamManager.Initialize();
			Components.Add(SteamManager);

			GameGlobals.Width = Window.ClientBounds.Width;
			GameGlobals.Height = Window.ClientBounds.Height;
			GameGlobals.GraphicsDevice = GraphicsDeviceManager.GraphicsDevice;
			base.Initialize();
		}

		private void CreateGameStates()
		{
			HomePageContext = new HomePage(this);
			InWorldContext = new GameClient(this);
			CreditsContext = new Credits(this);
			MultiplayerPageContext = new Multiplayer(this);
			TimeoutContext = new TimeoutMenu(this);
			SettingsContext = new Settings(this);
			Window.TextInput += SettingsContext.OnTextInput;
			CurrentGameContext = HomePageContext;
		}

		protected override void LoadContent()
		{
			SpriteBatch = new SpriteBatch(GraphicsDevice);
			Renderer.Initialize(this);

			GameFonts.LoadAssets(Content);
			GameSounds.LoadAssets(Content);
			GameTextures.LoadAssets(GraphicsDevice);
			ItemTextures.LoadAssets(GraphicsDevice);
			CreateGameStates();
		}

		float splashTimer = 3;

		protected override void Update(GameTime gameTime)
		{
			splashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

			if (CurrentGameContext != PreviousGameContext && PreviousGameContext != null)
			{
				PreviousGameContext.Unload();
				PreviousGameContext.Active = false;
			}

			if (CurrentGameContext.Active == false)
			{
				CurrentGameContext.Load();
				CurrentGameContext.Active = true;
			}

			CurrentGameContext.Update(gameTime);

			PreviousGameContext = CurrentGameContext;

			ClickTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
			base.Update(gameTime);
		}

		private void DrawDebugOverlay()
		{
			SpriteBatch.Begin();
			SpriteBatch.Print(Color.White, new Vector2(2, 0), String.Format("fps: {0} ", Math.Floor(FPSCounter.GetAverageFramerate())));
			SpriteBatch.End();
		}


		protected override void OnExiting(object sender, EventArgs args)
		{
			GameSettings.Save();
			InWorldContext.Disconnect();
			
			SteamManager.Shutdown();
			base.OnExiting(sender, args);
		}

		private void DrawSplash()
		{
			
			GraphicsDevice.Clear(Color.Black);

			SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
			Vector2 center = new Vector2(GameGlobals.Width / 2.0f, GameGlobals.Height / 2.0f);
			Vector2 origin = new Vector2(GameTextures.EyeOfHorus.Width / 2.0f, GameTextures.EyeOfHorus.Height / 2.0f);
			float scale = 4;
			SpriteBatch.Draw(GameTextures.EyeOfHorus, center-new Vector2(0, (float)Math.Sin(splashTimer*2)*10), null, Color.White, 0, origin, scale, SpriteEffects.None, 0);
			SpriteBatch.Print(GameFonts.Arial30, Color.White, center - new Vector2(220, -100), "CONARIUM SOFTWARE");
			SpriteBatch.End();

		}

		protected override void Draw(GameTime gameTime)
		{
			if (!GameTextures.ContentLoaded)
				return;

			if (splashTimer > 0)
			{
				DrawSplash();
				return;
			}

			GraphicsDevice.Clear(Color.Black);

			SpriteBatch.Begin();

			Vector2 center = new Vector2(GameGlobals.Width / 2.0f, GameGlobals.Height / 2.0f);
			Vector2 origin = new Vector2(GameTextures.BG.Width / 2.0f, GameTextures.BG.Height / 2.0f);
			float scale = GameGlobals.Width / (float)GameTextures.BG.Width;
			SpriteBatch.Draw(GameTextures.BG, center, null, Color.White, 0, origin, scale, SpriteEffects.None, 0);
			SpriteBatch.End();

			if (CurrentGameContext.Active == true)
			{
				CurrentGameContext.Draw(SpriteBatch);
			}
			FPSGraph.Draw(SpriteBatch);
			DrawDebugOverlay();

			Console.Draw(SpriteBatch);
			base.Draw(gameTime);
		}
	}
}

