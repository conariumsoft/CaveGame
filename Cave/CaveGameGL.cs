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
using CaveGame.Core.Tiles;
using CaveGame.Core.Generic;
using Steamworks;

namespace CaveGame.Client
{
	public static class GameGlobals
	{
		public static int Width;
		public static int Height;
		public static GraphicsDevice GraphicsDevice;
	}

	public class DelayedTaskContainer
	{
		List<DelayedTask> tasks;

		public DelayedTaskContainer()
		{
			tasks = new List<DelayedTask>();
		}

		public void Update(GameTime gt)
		{
			foreach (var task in tasks)
				task.Update(gt);
		}

		public void Add(DelayedTask task)
		{
			tasks.Add(task);
		}
	}

	public class CaveGameGL : Game
	{

		public IGameContext CurrentGameContext { get; set; }
		private IGameContext PreviousGameContext { get; set; }

		#region Game States
		public HomePage HomePageContext;
		public GameClient InWorldContext;
		public ServerKickedPage ServerKickedContext;
		public Credits CreditsContext;
		#endregion

		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		#region GameComponents
		public CommandBar Console { get; private set; }
		public FPSGraph FPSGraph { get; private set; }
		public FrameCounter FPSCounter { get; private set; }
		#endregion

		public static float ClickTimer;


		DelayedTaskContainer taskManager;


		#region Steam Callbacks
		private void Steam_OnOverlayActivated(Steamworks.GameOverlayActivated_t pCallback)
		{

		}
		private void Steam_OnShutdown(Steamworks.SteamShutdown_t callback) { }
		private void Steam_OnScreenshotRequested(Steamworks.ScreenshotRequested_t callback) { }
		private void Steam_OnUserStatsReceived(Steamworks.UserStatsReceived_t callback) { }
		private void Steam_OnUserStatsStored(UserStatsStored_t param)
		{

		}
		private void Steam_OnAchievementsStored(UserAchievementStored_t param)
		{
		}
		protected Steamworks.Callback<Steamworks.GameOverlayActivated_t> m_OverlayActivated;
		#endregion

		private void Steam_LoadAchievements()
		{
			foreach(Achievement ach in Achievements.List)
			{
				bool ret = SteamUserStats.GetAchievement(ach.SteamAcheivementID, out ach.Achieved);

				if (ret)
				{
					ach.Name = SteamUserStats.GetAchievementDisplayAttribute(ach.SteamAcheivementID, "name");
					ach.Description = SteamUserStats.GetAchievementDisplayAttribute(ach.SteamAcheivementID, "desc");
				}
			}
		}

		private void Steam_EnsureDLLExists()
		{
			try
			{
				if (Steamworks.SteamAPI.RestartAppIfNecessary((Steamworks.AppId_t)1238250))
				{
					Debug.WriteLine("Steam restarting?");
					Exit();
					//return false;
				}
			} catch (System.DllNotFoundException e)
			{
				Debug.WriteLine("Missing steam_api64.dll");
				throw new Exception("Missing steam_api64.dll");
			}
		}

		private void Steam_InitAPI()
		{
			if (!Steamworks.SteamAPI.Init())
			{
				throw new Exception("Steam API Failed to initialize!");
			}
		}
		public CaveGameGL()
		{
			Steam_EnsureDLLExists();
			Steam_InitAPI();

			

			m_OverlayActivated = Steamworks.Callback<Steamworks.GameOverlayActivated_t>.Create(Steam_OnOverlayActivated);
			Steamworks.Callback<Steamworks.SteamShutdown_t>.Create(Steam_OnShutdown);
			Steamworks.Callback<Steamworks.ScreenshotRequested_t>.Create(Steam_OnScreenshotRequested);
			Steamworks.Callback<Steamworks.UserStatsReceived_t>.Create(Steam_OnUserStatsReceived);
			Steamworks.Callback<Steamworks.UserStatsStored_t>.Create(Steam_OnUserStatsStored);
			Steamworks.Callback<Steamworks.UserAchievementStored_t>.Create(Steam_OnAchievementsStored);

			bool statsSuccess = SteamUserStats.RequestCurrentStats();


			taskManager = new DelayedTaskContainer();

			taskManager.Add(new DelayedTask(() => Steamworks.SteamAPI.RunCallbacks(), 1 / 20.0f));

			graphics = new GraphicsDeviceManager(this) 
			{
				PreferredBackBufferWidth = 1280,
				PreferredBackBufferHeight = 720,
				SynchronizeWithVerticalRetrace = false,
				IsFullScreen = false,
				PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
			};
			IsFixedTimeStep = false;
			IsMouseVisible = true;
			Content.RootDirectory = "Content";
			Window.AllowUserResizing = true;
			Window.AllowAltF4 = true;


		}


		void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
			graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
			graphics.ApplyChanges();
			GameGlobals.Width = Window.ClientBounds.Width;
			GameGlobals.Height = Window.ClientBounds.Height;
			InWorldContext.Camera.Bounds = Window.ClientBounds;
			InWorldContext.Camera._screenSize = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);
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
			#region CommandBar Setup
			Console = new CommandBar(this);
			Components.Add(Console);
		//	Window.TextInput += Console.OnTextInput;
			

			Console.BindCommandInformation(new Command("test", "it does a thing", new List<string>{"argA", "argB", "argC"}));
			Console.BindCommandInformation(new Command("teleport", "", new List<string> { "x", "y" }));
			Console.BindCommandInformation(new Command("god", "AAOKSOKADFOS", new List<string> {"n"}));
			Console.Handler += CommandBarEvent;

			#endregion

			FPSCounter = new FrameCounter(this);
			FPSGraph = new FPSGraph(this, 25, 500);
			Components.Add(FPSCounter);
			Components.Add(FPSGraph);

			GameGlobals.Width = Window.ClientBounds.Width;
			GameGlobals.Height = Window.ClientBounds.Height;
			GameGlobals.GraphicsDevice = graphics.GraphicsDevice;
			base.Initialize();
		}

		private void CreateGameStates()
		{
			HomePageContext = new HomePage(this);
			InWorldContext = new GameClient(this);
			CreditsContext = new Credits(this);

			CurrentGameContext = HomePageContext;
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			Renderer.Initialize(this);

			GameFonts.LoadAssets(Content);
			GameSounds.LoadAssets(Content);
			GameTextures.LoadAssets(Content);
			ItemTextures.LoadAssets(Content);
			CreateGameStates();
		}

		

		protected override void Update(GameTime gameTime)
		{
			taskManager.Update(gameTime);

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
			spriteBatch.Begin();
			spriteBatch.Print(Color.White, new Vector2(2, 0), String.Format("fps: {0} ", Math.Floor(FPSCounter.GetAverageFramerate())));
			spriteBatch.End();
		}


		protected override void OnExiting(object sender, EventArgs args)
		{
			InWorldContext.Disconnect();
			SteamAPI.Shutdown();
			base.OnExiting(sender, args);
		}



		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			if (CurrentGameContext.Active == true)
			{
				CurrentGameContext.Draw(spriteBatch);
			}
			FPSGraph.Draw(spriteBatch);
			DrawDebugOverlay();

			spriteBatch.Begin();
			spriteBatch.Draw(ItemTextures.Bomb, new Vector2(4, 4), Color.White);
			spriteBatch.End();

			Console.Draw(spriteBatch);
			base.Draw(gameTime);
		}
	}
}

