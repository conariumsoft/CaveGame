using CaveGame.Core.FileUtil;
using Microsoft.Xna.Framework.Input;
using System;

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

	public class FramerateSetting
	{
		public FramerateLimiterOptions Selected { get; set; }
		public int FramerateLimit { get; set; }
		public string DisplayString { get; }
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
	}


	[Serializable]
	public class XGameSettings : Configuration
	{

		public bool Fullscreen;
		public bool Particles;
		public int FPSCapIndex;
		public int FPSLimit
		{
			get
			{
				return GameSettingsData.FPSSlider[FPSCapIndex].Value;
			}
		}

		public override void FillDefaults()
		{
			Fullscreen = false;
			FPSCapIndex = 4;
			MoveDownKey = Keys.S;
			MoveUpKey = Keys.W;
			JumpKey = Keys.Space;
			MoveLeftKey = Keys.A;
			MoveRightKey = Keys.D;
			ChatSize = GameChatSize.Normal;
		}

		public Keys MoveLeftKey;
		public Keys MoveRightKey;
		public Keys MoveDownKey;
		public Keys MoveUpKey;
		public Keys JumpKey;

		public GameChatSize ChatSize;

	}

	public static class GameSettingsData
	{
		public static SliderIndex<int>[] FPSSlider =
		{
			new SliderIndex<int>("Unlimited", 0),
			new SliderIndex<int>("240", 240),
			new SliderIndex<int>("144", 144),
			new SliderIndex<int>("120", 120),
			new SliderIndex<int>("90", 90),
			new SliderIndex<int>("60", 60),
			new SliderIndex<int>("30", 30),
		};

		public static SliderIndex<GameChatSize>[] ChatSizeSlider =
		{
			new SliderIndex<GameChatSize>("Large", GameChatSize.Large),
			new SliderIndex<GameChatSize>("Normal", GameChatSize.Normal),
			new SliderIndex<GameChatSize>("Small", GameChatSize.Small)
		};
	}
}
