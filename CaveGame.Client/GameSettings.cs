using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using CaveGame.Client.DesktopGL;
using CaveGame.Common;

namespace CaveGame.Client
{
	public enum FramerateLimiterOptions
	{
		None,
		Cap240,
		Cap144,
		Cap120,
		Cap90,
		Cap60,
		Cap30
	}

	public class SliderIndex<T>
	{
		public string Display;
		public T Value;

		public SliderIndex(string display, T val)
		{
			Display = display;
			Value = val;
		}


		public static SliderIndex<int>[] GetIntArray(int minimum, int maximum, int increment = 1)
		{

			SliderIndex<int>[] arr = new SliderIndex<int>[maximum-minimum];
			for (int i = 0; i<(maximum - minimum); i++)
			{
				arr[i] = new SliderIndex<int>( (minimum + (increment * i)).ToString(), minimum + (increment * i));
			}
			return arr;
		}
		public static SliderIndex<float>[] GetFloatArray(int minimum, int maximum, float increment)
		{
			SliderIndex<float>[] arr = new SliderIndex<float>[maximum - minimum];
			for (int i = 0; i < (maximum - minimum); i ++)
			{
				arr[i] = new SliderIndex<float>((minimum + (increment * i)).ToString(), minimum + (increment * i));
			}
			return arr;
		}
	}


	

	// Old name for game settings, backward compatibility
	[XmlRoot("XGameSettings")]
	public class GameSettings : ConfigFile
	{
		public static SliderIndex<int>[] VolumeSliderOptions = SliderIndex<int>.GetIntArray(0, 101);

		public static SliderIndex<GameChatSize>[] ChatSizeSliderOptions =
		{
			new SliderIndex<GameChatSize>("Large", GameChatSize.Large),
			new SliderIndex<GameChatSize>("Normal", GameChatSize.Normal),
			new SliderIndex<GameChatSize>("Small", GameChatSize.Small)
		};
		public static GameSettings CurrentSettings { get; set; }

		public GameSettings()
		{
			CurrentSettings = this;
			
		}

		[XmlIgnore]
		public CaveGameDesktopClient game;


		[XmlIgnore]
		private bool _fullscreen;
		private bool _particles;
		private bool _vsync;
		private int _fpslimit;
		private int _masterVolume;
		private int _musicVolume;
		private int _sfxVolume;


		public bool Fullscreen {
			get => _fullscreen;
			set  { 
				_fullscreen = value;
				game?.SetFullscreen(value);
			}
		}
		

		public bool Particles {
			get => _particles;
			set {
				_particles = value;
				
			}
		}

		public bool VSync
		{
			get => _vsync;
			set
			{
				_vsync = value;
				game?.SetVSync(value);
			}
		}

		
		public int FPSLimit {
			get => _fpslimit;
			set {
				_fpslimit = value;
				game?.SetFPSLimit(value);
			}
		}

		

		public int MasterVolume {
			get => _masterVolume;
			set {
				_masterVolume = value;
				AudioManager.MasterVolume = value / 100.0f;
			}
		}

		
		public int MusicVolume { get => _musicVolume;
			set
			{
				_musicVolume = value;
				AudioManager.MusicVolume = value / 100.0f;
			}
		}
		public int SFXVolume
		{
			get => _sfxVolume;
			set
			{
				_sfxVolume = value;
			}
		}

		public Keys MoveLeftKey { get; set; }
		public Keys MoveRightKey { get; set; }
		public Keys MoveDownKey { get; set; }
		public Keys MoveUpKey { get; set; }
		public Keys JumpKey { get; set; }
		public GameChatSize ChatSize { get; set; }
		public string TexturePackName { get; set; }
		public bool CameraShake { get; set; }

		public override void FillDefaults()
		{
			FPSLimit = 60;
			Fullscreen = false;
			MoveDownKey = Keys.S;
			MoveUpKey = Keys.W;
			JumpKey = Keys.Space;
			MoveLeftKey = Keys.A;
			MoveRightKey = Keys.D;
			ChatSize = GameChatSize.Normal;
			MasterVolume = 100;
			MusicVolume = 50;
			SFXVolume = 75;

		}

	}
}
