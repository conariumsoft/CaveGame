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

namespace CaveGame.Client
{

	public static class GameFonts
	{

		public static SpriteFont Arial8 { get; private set; }
		public static SpriteFont Arial10 { get; private set; }
		public static SpriteFont Arial12 { get; private set; }
		public static SpriteFont Arial14 { get; private set; }
		public static SpriteFont Arial16 { get; private set; }
		public static SpriteFont Arial20 { get; private set; }
		public static SpriteFont Arial30 { get; private set; }


		public static SpriteFont Arial10Italic { get; private set; }

		public static SpriteFont Consolas10 { get; private set; }
		public static SpriteFont Consolas12 { get; private set; }

		public static SpriteFont ComicSans10 { get; private set; }


		public static void LoadAssets(ContentManager Content)
		{
			Arial8 = Content.Load<SpriteFont>("Fonts/Arial8");
			Arial10 = Content.Load<SpriteFont>("Fonts/Arial10");
			Arial12 = Content.Load<SpriteFont>("Fonts/Arial12");
			Arial14 = Content.Load<SpriteFont>("Fonts/Arial14");
			Arial16 = Content.Load<SpriteFont>("Fonts/Arial16");
			Arial20 = Content.Load<SpriteFont>("Fonts/Arial20");
			Arial30 = Content.Load<SpriteFont>("Fonts/Arial30");

			Arial10Italic = Content.Load<SpriteFont>("Fonts/Arial10Italic");

			Consolas10 = Content.Load<SpriteFont>("Fonts/Consolas10");
			Consolas12 = Content.Load<SpriteFont>("Fonts/Consolas12");

			ComicSans10 = Content.Load<SpriteFont>("Fonts/ComicSans10");
		}
	}

	public static class GameTextures
	{
		public static Texture2D Player { get; private set; }

		public static Texture2D EyeOfHorus { get; private set; }
		public static Texture2D ParticleSet { get; private set; }
		public static Texture2D TileSheet { get; private set; }

		public static void LoadAssets(ContentManager Content)
		{
			Player = Content.Load<Texture2D>("Entities/player");
			EyeOfHorus = Content.Load<Texture2D>("Textures/csoft");
			ParticleSet = Content.Load<Texture2D>("Textures/particles");
			TileSheet = Content.Load<Texture2D>("Textures/tilesheet");
		}
	}

	public static class GameSounds
	{
		public static SoundEffect MenuBlip { get; private set; }
		public static SoundEffect MenuBlip2 { get; private set; }

		public static Song AmbientLava { get; private set; }
		public static Song AmbientBirds1 { get; private set; }
		public static Song AmbientBirds2 { get; private set; }
		public static Song AmbientBirds3 { get; private set; }
		public static Song AmbientBirds4 { get; private set; }
		public static Song AmbientBirds5 { get; private set; }
		public static Song AmbientBirds6 { get; private set; }
		public static Song AmbientBirds7 { get; private set; }

		public static Song AmbientCreepy1 { get; private set; }
		public static Song AmbientCreepy2 { get; private set; }
		public static Song AmbientCreepy3 { get; private set; }
		public static Song AmbientCreepy4 { get; private set; }

		public static void LoadAssets(ContentManager Content)
		{
			MenuBlip = Content.Load<SoundEffect>("Sound/click1");
			MenuBlip2 = Content.Load<SoundEffect>("Sound/menu1");

			AmbientLava = Content.Load<Song>("Sound/ambient/lava");
			AmbientBirds1 = Content.Load<Song>("Sound/ambient/birds1");
			AmbientBirds2 = Content.Load<Song>("Sound/ambient/birds2");
			AmbientBirds3 = Content.Load<Song>("Sound/ambient/birds3");
			AmbientBirds4 = Content.Load<Song>("Sound/ambient/birds4");
			AmbientBirds5 = Content.Load<Song>("Sound/ambient/birds5");
			AmbientBirds6 = Content.Load<Song>("Sound/ambient/birds6");
			AmbientBirds7 = Content.Load<Song>("Sound/ambient/birds7");

			AmbientCreepy1 = Content.Load<Song>("Sound/ambient/birds1");
			AmbientBirds7 = Content.Load<Song>("Sound/ambient/birds1");
		}
	}

	public static class GameGlobals
	{
		public static int Width;
		public static int Height;
		public static GraphicsDevice GraphicsDevice;
	}

	public class CaveGameGL : Game
	{


		public IGameContext CurrentGameContext { get; set; }
		private IGameContext PreviousGameContext { get;  set; }
		

		public HomePage HomePageContext;
		public GameClient InWorldContext;
		public ServerKickedPage ServerKickedContext;

		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		#region GameComponents
		public CommandBar Console { get; private set; }
		public FPSGraph FPSGraph { get; private set; }
		public FrameCounter FPSCounter { get; private set; }
		#endregion

		public static float ClickTimer;

		public SteamManager SteamManager;

		public CaveGameGL()
		{
			Steamworks.SteamAPI.Init();
			SteamManager = new SteamManager(this);
			Components.Add(SteamManager);

			//Steamworks.SteamAPI.Init();
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

		private void OnSteamOverlayOpened() {}
		private void OnSteamOverlayClosed() { }


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

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			Renderer.Initialize(this);

			GameFonts.LoadAssets(Content);
			GameSounds.LoadAssets(Content);
			GameTextures.LoadAssets(Content);
			

			HomePageContext = new HomePage(this);
			//HomePageContext.Load();
			InWorldContext = new GameClient(this);
			CurrentGameContext = HomePageContext;
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
			
			Console.Draw(spriteBatch);
			base.Draw(gameTime);
		}
	}
}

