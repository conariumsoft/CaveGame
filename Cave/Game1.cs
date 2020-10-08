﻿using CaveGame.Core;
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

namespace CaveGame.Client
{



	public static class GameShaders
	{

		public static Effect aaaaaaaaaaa { get; private set; }
		
		public static void LoadAssets(ContentManager Content)
		{
			aaaaaaaaaaa = Content.Load<Effect>("Shaders/idkwhatthisisgoingtodo");
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


			StructureMetadata meta = new StructureMetadata
			{
				Width = 32,
				Height = 16,
				Author = "jms",
				Name = "Dungon1",
				Notes = "ff",
			};
			StructureFile str = new StructureFile(meta);
			Layer brug = new Layer(str) { LayerID = "Brug" };
			str.Layers.Add(brug);
			brug.Tiles = new Core.Tiles.Tile[32, 16];
			for (int x = 0; x <32;x++)
			{
				for (int y = 0; y < 16;y++)
				{
					brug.Tiles[x, y] = new Air();
				}
			}
			brug.Walls = new Core.Walls.Wall[32, 16];
			for (int x = 0; x < 32; x++)
			{
				for (int y = 0; y <16; y++)
				{
					brug.Walls[x, y] = new Core.Walls.Air();
				}
			}
			brug.Tiles[5, 5] = new Stone();
			brug.Tiles[5, 5] = new Stone();
			brug.Tiles[5, 6] = new Stone();
			brug.Walls[10, 10] = new Core.Walls.Stone();
			brug.Walls[1, 1] = new Core.Walls.Stone();
			str.Save();
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
			GameShaders.LoadAssets(Content);
			

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

