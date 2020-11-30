using CaveGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using CaveGame.Client.Menu;
using CaveGame.Client.UI;
using CaveGame.Core.FileUtil;
using System.IO;
using Microsoft.Xna.Framework.Input;
using System.Globalization;
using CaveGame.Core.Network;
using CaveGame.Core.Generic;

namespace CaveGame.Client
{
	public class CaveGameGL : Microsoft.Xna.Framework.Game
	{
		public GameSettings GameSettings { get; set; }

		public IGameContext CurrentGameContext { get; set; }
		private IGameContext PreviousGameContext { get; set; }

		#region Game States
		public GameClient GameClientContext;
		public Menu.MenuManager MenuContext;
		public Menu.Settings SettingsContext;
		#endregion

		public GraphicsEngine GraphicsEngine { get; private set; }
		public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
		public SpriteBatch SpriteBatch { get; private set; }
		#region GameComponents
		public CommandBar Console { get; private set; }
		public FrameCounter FPSCounter { get; private set; }
		public Splash Splash { get; private set; }
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

		public void OnSetChatSize(GameChatSize size) {}

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
			GameSettings = Configuration.Load<GameSettings>("settings.xml", true);
			SteamManager = new SteamManager(this);
			

			GraphicsDeviceManager = new GraphicsDeviceManager(this) 
			{
				PreferredBackBufferWidth = 1280,
				PreferredBackBufferHeight = 720,
				SynchronizeWithVerticalRetrace = false,
				IsFullScreen = false,
				PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
			};


			IsMouseVisible = true;
			Content.RootDirectory = "Content";
			Window.AllowUserResizing = true;
			Window.AllowAltF4 = true;

			GraphicsEngine = new GraphicsEngine();

			
			Splash = new Splash();

#if DEBUG
			GraphicsEngine.LoadingDelay = 0.05f;
			Splash.SplashTimer = 5f;
#endif
			OnSetFPSLimit(GameSettings.FPSLimit);
		}

		void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			GraphicsEngine.WindowSize = Window.ClientBounds.Size.ToVector2();
			if (GameClientContext != null)
			{
				GameClientContext.Camera.Bounds = Window.ClientBounds;
				GameClientContext.Camera._screenSize = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);
			}
			
		}

		private void OnTeleportCommand(CommandBar sender, Command command, params string[] args)
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

			if (GameClientContext.MyPlayer != null)
			{
				GameClientContext.MyPlayer.NextPosition = new Vector2(x, y);
			}
		}
		private void OnGodCommand(CommandBar sender, Command command, params string[] args)
		{
			GameClientContext.MyPlayer.God = !GameClientContext.MyPlayer.God;
		}
		private void OnDisconnectCommand(CommandBar sender, Command command, params string[] args)
		{
			if (CurrentGameContext == GameClientContext)
			{
				GameClientContext.OverrideDisconnect();

			} else
			{
				sender.Out("Not connected to a server!", Color.Red);
				return;
			}
		}
		private void OnScreenshot(CommandBar sender, Command command, params string[] args)
		{
			if (args.Length>0)
			{
				TakeScreenshot(args[0]);
				sender.Out("Screenshot taken: "+args[0]+".png");
			} else
			{
				TakeScreenshot();
				sender.Out("Screenshot taken!");
			}
			
		}
		private void SendAdminCommand(string command, params string[] args) => GameClientContext.Send(new AdminCommandPacket(command, args, GameClientContext.MyPlayer.EntityNetworkID));
		private void CmdTimeCommand(CommandBar sender, Command command, params string[] args)
        {
			if (args.Length > 0)
				GameClientContext.World.TimeOfDay = float.Parse(args[0], CultureInfo.InvariantCulture.NumberFormat);
			else
				sender.Out("Time of day (hours): " + GameClientContext.World.TimeOfDay.ToString());
		}
		private void CmdRequestSummonEntity(CommandBar sender, Command command, params string[] args)
        {
			if (args.Length == 0) // give list of entities
            {
				sender.Out("Syntax: sv_summon <entity_id> <xpos> <ypos> <metadata>");
				sender.Out("Entity ID list:");
				sender.Out("itemstack, wurmhole");
			}
			else
            {
				
				SendAdminCommand(command.Keyword, args);
			}
				
        }
		private void CmdRequestItemstack(CommandBar sender, Command command, params string[] args)
        {
			if (args.Length == 0)
            {

            } 
			else
				SendAdminCommand(command.Keyword, args);
		}

		protected override void Initialize()
		{
			Window.TextInput += TextInputManager.OnTextInput;
			Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
			OnSetFullscreen(GameSettings.Fullscreen);


			Console = new CommandBar(this);

			// epic new .NET 5 feature
			Command[] commands =
			{
				new ("teleport", "", new List<string> { "x", "y" }, OnTeleportCommand),
				new ("god", "", new List<string> {}, OnGodCommand),
				new ("disconnect", "", new List<string> { }, OnDisconnectCommand),
				new ("connect", "", new List<string> { }, OnDisconnectCommand),
				new ("screenshot", "", new List<string> { }, OnScreenshot),
				new ("time", "Set/Get time of day", new List<string> { "time" }, CmdTimeCommand),
				new ("sv_summon", "Summon an entity", new List<string>{"entityid, xpos, ypos, metadatastring" }, CmdRequestSummonEntity)
				
			};
			foreach (var command in commands)
				Console.BindCommandInformation(command);

			Components.Add(Console);
			
			FPSCounter = new FrameCounter(this);
			Components.Add(FPSCounter);


			SteamManager.Initialize();
			Components.Add(SteamManager);

			base.Initialize();
		}


		private void CreateGameStates()
		{
			MenuContext = new MenuManager(this);
			GameClientContext = new GameClient(this);
			SettingsContext = new Settings(this);
			Window.TextInput += SettingsContext.OnTextInput;
			CurrentGameContext = MenuContext;
		}

		protected override void LoadContent()
		{
			GameSounds.LoadAssets(Content);

			GraphicsEngine.ContentManager = Content;
			GraphicsEngine.GraphicsDevice = GraphicsDevice;
			GraphicsEngine.GraphicsDeviceManager = GraphicsDeviceManager;

			GraphicsEngine.Initialize();
			GraphicsEngine.LoadAssets(GraphicsDevice);

			//GameTextures.LoadAssets(GraphicsDevice);
			//ItemTextures.LoadAssets(GraphicsDevice);
			CreateGameStates();
		}


		public void TakeScreenshot(string filename = "")
		{
			bool wasEnabled = Console.Enabled;
			Console.Enabled = false;

			Color[] colors = new Color[GraphicsDevice.Viewport.Width * GraphicsDevice.Viewport.Height];

			GraphicsDevice.GetBackBufferData<Color>(colors);

			using (Texture2D tex2D = new Texture2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height))
			{

				Directory.CreateDirectory("Screenshots");
				tex2D.SetData<Color>(colors);
				if (string.IsNullOrEmpty(filename))
				{
					filename = $"Screenshots\\{DateTime.Now.ToFileTime()}.png";
				}
				using (FileStream stream = File.Create(filename))
				{
					tex2D.SaveAsPng(stream, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
				}
			}
			Console.Enabled = wasEnabled;
		}

		KeyboardState previousKB = Keyboard.GetState();
		protected override void Update(GameTime gameTime)
		{
			// update graphics information
			GraphicsEngine.WindowSize = Window.ClientBounds.Size.ToVector2();
			GraphicsEngine.Update(gameTime);

			if (!GraphicsEngine.ContentLoaded)
				return;

			if (Splash.SplashActive)
            {
				Splash.Update(gameTime);
				//return;
            }

			KeyboardState currentKB = Keyboard.GetState();
			if (currentKB.IsKeyDown(Keys.F5) && !previousKB.IsKeyDown(Keys.F5)) {
				TakeScreenshot();
			}

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
			GraphicsEngine.Begin();
			GraphicsEngine.Text(String.Format("fps: {0} ", Math.Floor(FPSCounter.GetAverageFramerate())), new Vector2(2, 0));
			GraphicsEngine.End();
		}


		protected override void OnExiting(object sender, EventArgs args)
		{
			GameSettings.Save();
			GameClientContext.Disconnect();
			
			SteamManager.Shutdown();
			base.OnExiting(sender, args);
		}

		private void DrawLoadingBar(GraphicsEngine GFX)
        {
			if (!GFX.FontsLoaded)
				return;

			float frac = (float)GFX.LoadedTextures / (float)GFX.TotalTextures;
			string text = String.Format("Loading: {0} of {1} ({2}%)", GFX.LoadedTextures, GFX.TotalTextures, (int)(frac*100));
			GFX.GraphicsDevice.Clear(Color.Black);
			GFX.Begin();
			GFX.Text(GFX.Fonts.Arial20, text, GFX.WindowSize / 2.0f, Color.White, TextXAlignment.Center, TextYAlignment.Center);

			float barY = (GFX.WindowSize.Y / 2.0f) + 20.0f;
			float barX = GFX.WindowSize.X / 2.0f;
			float barLength = GFX.WindowSize.X / 3.0f;
			float barHeight = 10.0f;

			Vector2 center = new Vector2(barX, barY);
			
			GFX.Rect(
				Color.Gray, 
				center - new Vector2(barLength/2.0f, 0), 
				new Vector2(barLength, barHeight)
			);
			GFX.Rect(
				Color.White,
				center - new Vector2(barLength / 2.0f, 0),
				new Vector2(barLength*frac, barHeight)
			);
			
		
			GFX.End();
        }

		protected override void Draw(GameTime gameTime)
		{
			if (!GraphicsEngine.ContentLoaded)
            {
				DrawLoadingBar(GraphicsEngine);
				return;
			}
			if (Splash.SplashActive)
            {
				Splash.Draw(GraphicsEngine);
				return;
            }

			GraphicsEngine.Clear(Color.Black);
			GraphicsEngine.Begin();

			Vector2 center = GraphicsEngine.WindowSize / 2.0f;
			Vector2 origin = new Vector2(GraphicsEngine.BG.Width, GraphicsEngine.BG.Height) / 2.0f;
			float horizscale = GraphicsEngine.WindowSize.X / (float)GraphicsEngine.BG.Width;
			float vertscale = GraphicsEngine.WindowSize.Y / (float)GraphicsEngine.BG.Height;
			float scale = Math.Max(horizscale, vertscale);


			GraphicsEngine.Sprite(GraphicsEngine.BG, center, null, Color.White, Rotation.Zero, origin, scale, SpriteEffects.None, 0);
			GraphicsEngine.End();

			if (CurrentGameContext.Active == true)
				CurrentGameContext.Draw(GraphicsEngine);
			DrawDebugOverlay();

			Console.Draw(GraphicsEngine);
			base.Draw(gameTime);
		}
	}
}

