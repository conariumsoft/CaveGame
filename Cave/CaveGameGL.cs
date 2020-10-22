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

	public class CaveGameGL : Game
	{

		public IGameContext CurrentGameContext { get; set; }
		private IGameContext PreviousGameContext { get; set; }

		#region Game States
		public GameClient InWorldContext;
		public Menu.HomePage HomePageContext;
		public Menu.Multiplayer MultiplayerPageContext;

		public Menu.TimeoutMenu TimeoutContext;
		public Menu.Credits CreditsContext;
		#endregion

		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		#region GameComponents
		public CommandBar Console { get; private set; }
		public FPSGraph FPSGraph { get; private set; }
		public FrameCounter FPSCounter { get; private set; }
		#endregion

		public static float ClickTimer;



		public SteamManager SteamManager { get; private set; }
		
		public CaveGameGL()
		{
			SteamManager = new SteamManager(this);
			
			bool statsSuccess = SteamUserStats.RequestCurrentStats();
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
			


			Console = new CommandBar(this);
			Console.BindCommandInformation(new Command("test", "it does a thing", new List<string>{"argA", "argB", "argC"}));
			Console.BindCommandInformation(new Command("teleport", "", new List<string> { "x", "y" }));
			Console.BindCommandInformation(new Command("god", "AAOKSOKADFOS", new List<string> {"n"}));
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
			GameGlobals.GraphicsDevice = graphics.GraphicsDevice;
			base.Initialize();
		}

		private void CreateGameStates()
		{
			HomePageContext = new HomePage(this);
			InWorldContext = new GameClient(this);
			CreditsContext = new Credits(this);
			MultiplayerPageContext = new Multiplayer(this);
			TimeoutContext = new TimeoutMenu(this);
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

			spriteBatch.Begin();

			Vector2 center = new Vector2(GameGlobals.Width / 2.0f, GameGlobals.Height / 2.0f);
			Vector2 origin = new Vector2(GameTextures.BG.Width / 2.0f, GameTextures.BG.Height / 2.0f);
			float scale = GameGlobals.Width / (float)GameTextures.BG.Width;
			spriteBatch.Draw(GameTextures.BG, center, null, Color.White, 0, origin, scale, SpriteEffects.None, 0);
			spriteBatch.End();

			if (CurrentGameContext.Active == true)
			{
				CurrentGameContext.Draw(spriteBatch);
			}
			FPSGraph.Draw(spriteBatch);
			DrawDebugOverlay();

			Console.Draw(spriteBatch);
			base.Draw(gameTime);
		}
	}
}

