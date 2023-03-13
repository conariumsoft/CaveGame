using CaveGame.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using CaveGame.Client.Menu;
using CaveGame.Client.UI;
using System.IO;
using Microsoft.Xna.Framework.Input;
using System.Globalization;
using System.Threading.Tasks;
using CaveGame.Common.Game.Items;
using CaveGame.Common.Game.Inventory;
using CaveGame.Common.Extensions;
using CaveGame.Common.Network;
using CaveGame.Common.World;
using CaveGame.Server;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace CaveGame.Client.DesktopGL;

public class CaveGameDesktopClient : Microsoft.Xna.Framework.Game
{
	// TODO: If running local server, shutdown server when player leaves the world


	public GameSettings Settings { get; set; }


	public Settings SettingsContext { get; private set; }

	public IGameContext CurrentGameContext { get; set; }
	private IGameContext PreviousGameContext { get; set; }
	public GameClient GameClientContext { get; private set; }
	public MenuManager MenuContext { get; private set; }
	public GraphicsEngine GraphicsEngine { get; private set; }
	public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

	public CommandBar Console { get; private set; } // TOOD: Change Name of CommandBar class to Console
	public FrameCounter FpsCounter { get; private set; }
	public Splash Splash { get; private set; }
	public Steam Steam { get; private set; }
	public static float ClickTimer { get; set; }

	#region GameSettingMethods
	// these functions don't actually toggle the associated flags
	// they are just for applying changes to game state
	// they should be triggered automatically by setting the flag in Game.Settings
	public void SetFpsLimit(int limit)
	{
		if (limit == 0)
		{
			IsFixedTimeStep = false;
		}
		else
		{
			IsFixedTimeStep = true;
			TargetElapsedTime = TimeSpan.FromSeconds(1d / limit);
		}
	}
	public void SetChatSize(GameChatSize size) { }
	public void SetFullscreen(bool full)
	{

		GraphicsDeviceManager.IsFullScreen = full;
		if (full)
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
	public void SetVSync(bool vsync)
	{
		this.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = vsync;
	}
	#endregion

	// join local (singleplayer server
	public void EnterLocalGame(WorldMetadata meta)
	{
		var serverConf = new ServerConfig
		{
			Port = 40269, // singleplayer server uses slightly different port
			World = meta.Name,
			ServerName = $"LocalServer [{meta.Name}] ",
			ServerMOTD = "Singleplayer game world.",
		};
		var worldMetadata = meta;
		LocalServer server = new LocalServer(serverConf, worldMetadata);
		server.Output = Console;
		Task.Factory.StartNew(server.Start);

		StartClient(Steam.SteamUsername, "127.0.0.1:40269");
		CurrentGameContext = GameClientContext;
		GameClientContext.OnShutdown += server.Shutdown;
	}



	public void StartClient(string userName, string address)
	{
		GameClientContext?.Dispose();
		GameClientContext = new GameClient(this, new AuthDetails
		{
			UserName = userName,
			ServerAddress = address
		});

		CurrentGameContext = GameClientContext;
	}
	public CaveGameDesktopClient()
	{
		Logger.LogInfo("Creating CaveGame Desktop Client");
		IsMouseVisible = true;
		Content.RootDirectory = "Assets";
		Window.AllowUserResizing = true;
		Window.AllowAltF4 = true;

		Logger.LogInfo("Loading Steamworks Library");
		Steam = new Steam(this);

		Logger.LogInfo("Creating Graphics Window");
		GraphicsDeviceManager = new GraphicsDeviceManager(this)
		{
			PreferredBackBufferWidth = 1280,
			PreferredBackBufferHeight = 720,
			SynchronizeWithVerticalRetrace = false,
			IsFullScreen = false,
			PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
		};

		GraphicsEngine = new GraphicsEngine();
		Splash = new Splash();

		FpsCounter = new FrameCounter(this);
		Components.Add(FpsCounter);

		// Initialize settings
		Logger.LogInfo("Loading Game Settings");
		this.LoadSettings();
		
		// TODO: Queue Textures for loading
		// TODO: Queue Sounds for loading
		// TODO: Queue Fonts for loading
		// TODO: Queue Scripts for loading
		
		
		// TODO: Implement loading+unloading schema for per-context assets?
		
	}

	private void LoadSettings()
	{
		Settings = new GameSettings();
		Settings.OnMasterVolumeChanged += (_, e) => AudioManager.MasterVolume = e.NewValue / 100.0f;
		Settings.OnAmbienceVolumeChanged += null;
		Settings.OnMusicVolumeChanged += null;
		Settings.OnSfxVolumeChanged += null;
		Settings.OnFullscreenStateChanged += (_, e) => SetFullscreen(e.NewValue);
		Settings.OnFullscreenResolutionChanged += null;
		Settings.OnVSyncEnabledChanged += (_, e) => SetVSync(e.NewValue);
		Settings.OnFpsLimitChanged += (_, e) => SetFpsLimit(e.NewValue);
		Settings.OnCameraShakeEnabledChanged += null;
		Settings.OnUiScaleChanged += null;
		Settings.OnParticlesEnabledChanged += null;
		//Settings.OnUIScaleChanged += (o, e) => return;
		Settings.LoadGameSettings();

	}

	// Update graphics engine's known window size
	void Window_ClientSizeChanged(object sender, EventArgs e) => GraphicsEngine.WindowSize = Window.ClientBounds.Size.ToVector2();


	#region GameConsole Commands
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
			GameClientContext.Disconnect();

		}
		else
		{
			sender.Out("Not connected to a server!", Color.Red);

		}
	}
	private void OnScreenshot(CommandBar sender, Command command, params string[] args)
	{
		if (args.Length > 0)
		{
			TakeScreenshot(args[0]);
			sender.Out("Screenshot taken: " + args[0] + ".png");
		}
		else
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
			sender.Out("Time of day (hours): " + GameClientContext.World.TimeOfDay);
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
		int amount = 1;
		if (args.Length == 2)
		{
			Int32.TryParse(args[1], out amount);
		}
		if (args.Length > 0)
		{
			bool success = Item.TryFromName(args[0], out Item item);
			if (success)
			{
				GameClientContext.MyPlayer.Inventory.AddItem(new ItemStack
				{
					Item = item,
					Quantity = amount
				});
				return;
			}

		}
		sender.Out("No item found with matching name!");
	}

	private void CmdForceCrash(CommandBar sender, Command command, params string[] args)
	{
		string text = (args.Length > 0) ? args[0] : "";
		throw new ApplicationException($"Forced Crash: {text}");
	}
	#endregion

	public void GoToMainMenu()
	{
		CurrentGameContext = MenuContext;
		MenuContext.CurrentPage = MenuContext.Pages["mainmenu"];
	}
	public void GoToTimeoutPage(string timeout)
	{
		CurrentGameContext = MenuContext;
		MenuContext.CurrentPage = MenuContext.Pages["timeoutmenu"];
		MenuContext.TimeoutMessage = timeout;
	}

	private void InitCommands(CommandBar console)
	{
		// epic new .NET 5 feature
		Command[] commands =
		{
			new("teleport", "", new List<string>
			{
				"x",
				"y"
			}, OnTeleportCommand),
			new("god", "", new List<string>(), OnGodCommand), new("disconnect", "", new List<string>(), OnDisconnectCommand), new("connect", "", new List<string>(), OnDisconnectCommand),
			new("screenshot", "", new List<string>(), OnScreenshot), new("time", "Set/Get time of day", new List<string>
			{
				"time"
			}, CmdTimeCommand),
			new("sv_summon", "Summon an entity", new List<string>
			{
				"entityid, xpos, ypos, metadatastring"
			}, CmdRequestSummonEntity),
			new("gimme", "Gives you an item", new List<string>
			{
				"itemid",
				"amount"
			}, CmdRequestItemstack),
			new("crash", "Forces the game to implode", new List<string>
			{
				"fake_reason"
			}, CmdForceCrash),
		};
		commands.ForEach(c => console.RegisterCommand(c));
	}


	protected override void Initialize()
	{
		// Setup Game Window
		Window.TextInput += TextInputManager.OnTextInput;
		Window.ClientSizeChanged += Window_ClientSizeChanged;
		//SetFullscreen(Settings.Fullscreen);

		// Create Game Components
		Console = new CommandBar(this);
		InitCommands(Console); // Inject commands
		GameConsole.SetInstance(Console); // set Global console
		Components.Add(Console);

		// init steam
		Steam.Initialize();
		Components.Add(Steam);

		base.Initialize();
	}


	protected override void LoadContent()
	{
		// TODO: Reload Hard-Requirements as far as concerns Content here

		// Queue Loadable Content Asynchronously

		// Load Sounds
		// can be automated at a further point
		Logger.LogInfo("Loading Music");
		AudioManager.RegisterSong("hey_bella", Content.Load<Song>("Sound/mu/hey_bella"));
		AudioManager.RegisterSong("mithril_ocean", Content.Load<Song>("Sound/mu/mithril_ocean"));
		AudioManager.RegisterSong("cliff", Content.Load<Song>("Sound/mu/cliff"));
		AudioManager.RegisterSong("big_brother", Content.Load<Song>("Sound/mu/big_brother"));
		Logger.LogInfo("Loading SFX");
		AudioManager.RegisterEffect("click1", Content.Load<SoundEffect>("Sound/click1"));
		//AudioManager.RegisterEffect("ambient_lava", Content.Load<SoundEffect>("sound/ambient/lava"));
		AudioManager.RegisterEffect("door", Content.Load<SoundEffect>("Sound/door"));

		
		GameSounds.LoadAssets(Content);
		
		AudioManager.PlaySong("hey_bella");

		// Init content within graphics engine
		Logger.LogInfo("Initialize Graphics Device");
		GraphicsEngine.ContentManager = Content;
		GraphicsEngine.GraphicsDevice = GraphicsDevice;
		GraphicsEngine.GraphicsDeviceManager = GraphicsDeviceManager;

		GraphicsEngine.Initialize();
		Logger.LogInfo("Loading Textures");
		GraphicsEngine.LoadAssets(GraphicsDevice); // begin texture loading routine
		// MY COCK HURTS

		// game menu contexts
		MenuContext = new MenuManager(this);
		GameClientContext = new GameClient(this);
		SettingsContext = new Settings(this);

		// bind text input handler
		Window.TextInput += SettingsContext.OnTextInput;

		CurrentGameContext = MenuContext; // set me
	}

	/// <summary>
	/// Takes a Screenshot of the game window, and saves it to disk as a .png
	/// </summary>
	/// <param name="filename">File name to save the screenshot as. Defaults to current timestamp.</param>
	public void TakeScreenshot(string filename = "")
	{
		
		
			bool previousVsync = Settings.VSync;
			SetVSync(true);
			Color[] colors = new Color[GraphicsDevice.Viewport.Width * GraphicsDevice.Viewport.Height];
			GraphicsDevice.GetBackBufferData(colors);
			using (Texture2D tex2D = new Texture2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height))
			{

				Directory.CreateDirectory("Screenshots");
				tex2D.SetData(colors);
				if (string.IsNullOrEmpty(filename))
				{
					filename = Path.Combine("Screenshots", DateTime.Now.ToFileTime() + ".png");
				}
				using (FileStream stream = File.Create(filename))
				{
					Logger.LogInfo($"Screenshot saved as {filename}");
					tex2D.SaveAsPng(stream, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
				}
			}
			SetVSync(previousVsync);
		
		
	}

	KeyboardState previousKeyboardState = Keyboard.GetState();
	protected override void Update(GameTime gameTime)
	{

		// update graphics information
		GraphicsEngine.WindowSize = Window.ClientBounds.Size.ToVector2();
		GraphicsEngine.Update(gameTime); // graphics engine state


		// if asset loading thread is not yet finished
		if (!GraphicsEngine.ContentLoaded)
			return; // don't bother

		if (Splash.SplashActive)
			Splash.Update(gameTime);


		// take screenshot
		KeyboardState currentKeyboardState = Keyboard.GetState();
		if (currentKeyboardState.IsKeyDown(Keys.F5) && !previousKeyboardState.IsKeyDown(Keys.F5))
			TakeScreenshot();


		// gamestate management
		// if context has changed, unload previous context
		if (CurrentGameContext != PreviousGameContext && PreviousGameContext != null)
		{
			PreviousGameContext.Unload();
			PreviousGameContext.Active = false;
		}

		// if current context is not loaded, initialize
		if (CurrentGameContext.Active == false)
		{
			CurrentGameContext.Load();
			CurrentGameContext.Active = true;
		}

		// update context
		CurrentGameContext.Update(gameTime);
		PreviousGameContext = CurrentGameContext;

		ClickTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
		base.Update(gameTime);
	}


	// main debug screen?
	// not game-dependent
	private void DrawDebugOverlay()
	{
		GraphicsEngine.Begin();
		GraphicsEngine.Text(String.Format("fps: {0} ", Math.Floor(FpsCounter.GetAverageFramerate())), new Vector2(2, 0));
		GraphicsEngine.End();
	}


	protected override void OnExiting(object sender, EventArgs args)
	{
		Settings.Save();
		GameClientContext.Disconnect();

		Steam.Shutdown();
		base.OnExiting(sender, args);
	}

	private void DrawLoadingBar(GraphicsEngine GFX)
	{
		if (!GFX.FontsLoaded)
			return;

		float frac = (float)GFX.LoadedTextures / (float)GFX.TotalTextures;
		string text = $"Loading: {GFX.LoadedTextures} of {GFX.TotalTextures} ({(int)(frac * 100)}%)";
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
			center - new Vector2(barLength / 2.0f, 0),
			new Vector2(barLength, barHeight)
		);
		GFX.Rect(
			Color.White,
			center - new Vector2(barLength / 2.0f, 0),
			new Vector2(barLength * frac, barHeight)
		);


		GFX.End();
	}

	private void DrawGameBackgroundGraphic(GraphicsEngine GFX)
	{
		// draw game BG at screen center

		Vector2 center = GraphicsEngine.WindowSize / 2.0f;
		Vector2 diff = (Mouse.GetState().Position.ToVector2() - center) / (center.Length() / 2.0f);
		Vector2 origin = new Vector2(GraphicsEngine.BG.Width, GraphicsEngine.BG.Height) / 2.0f;
		float horizscale = GraphicsEngine.WindowSize.X / (float)GraphicsEngine.BG.Width;
		float vertscale = GraphicsEngine.WindowSize.Y / (float)GraphicsEngine.BG.Height;
		float scale = Math.Max(horizscale, vertscale);

		// TODO: Move this into menu context?
		// reset graphics state
		GraphicsEngine.Clear(Color.Black);
		GraphicsEngine.Begin();
		GraphicsEngine.Sprite(GraphicsEngine.BG, center - diff, null, Color.White, Rotation.Zero, origin, scale, SpriteEffects.None, 0);
		GraphicsEngine.End();
	}

	protected override void Draw(GameTime gameTime)
	{

		// draw loading bar if still loading
		if (!GraphicsEngine.ContentLoaded)
		{

			DrawLoadingBar(GraphicsEngine);

			return;
		}

		// draw splash screen if active
		if (Splash.SplashActive)
		{

			Splash.Draw(GraphicsEngine);

			return;
		}


		DrawGameBackgroundGraphic(GraphicsEngine);

		// let current context draw
		if (CurrentGameContext.Active == true)
			CurrentGameContext.Draw(GraphicsEngine);
		// debug screen
		DrawDebugOverlay();

		Console.Draw(GraphicsEngine);
		base.Draw(gameTime);
	}
}
