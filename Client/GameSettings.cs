using CaveGame.Core;
using CaveGame.Core.FileUtil;
using Microsoft.Xna.Framework.Input;
using System;
using System.Xml.Serialization;

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
	public class GameSettings : Configuration
	{
		public static SliderIndex<int>[] VolumeSliderOptions = SliderIndex<int>.GetIntArray(0, 100);

		public static SliderIndex<int>[] FramerateCapSliderOptions =
		{
			new SliderIndex<int>("Unlimited", 0),
			new SliderIndex<int>("240", 240),
			new SliderIndex<int>("144", 144),
			new SliderIndex<int>("120", 120),
			new SliderIndex<int>("90", 90),
			new SliderIndex<int>("60", 60),
			new SliderIndex<int>("30", 30),
		};

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

		public bool Fullscreen { get; set; }
		public bool Particles { get; set; }
		public int FPSLimit { get; set; }
		public int FPSLimitIndex { get; set; }
		public Keys MoveLeftKey { get; set; }
		public Keys MoveRightKey { get; set; }
		public Keys MoveDownKey { get; set; }
		public Keys MoveUpKey { get; set; }
		public Keys JumpKey { get; set; }
		public GameChatSize ChatSize { get; set; }
		public string TexturePackName { get; set; }

		public int MasterVolume { get; set; }
		public int MusicVolume { get; set; }
		public int SFXVolume { get; set; }
		public int MenuVolume { get; set; }
		public bool CameraShake { get; set; }

		public override void FillDefaults()
		{
			FPSLimit = 120;
			FPSLimitIndex = 3;
			Fullscreen = false;
			MoveDownKey = Keys.S;
			MoveUpKey = Keys.W;
			JumpKey = Keys.Space;
			MoveLeftKey = Keys.A;
			MoveRightKey = Keys.D;
			ChatSize = GameChatSize.Normal;
		}

	}
}
