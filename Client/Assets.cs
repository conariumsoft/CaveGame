using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Text;

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
}
