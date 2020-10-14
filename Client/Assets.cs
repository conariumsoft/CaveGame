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

	public class TextureWrap {
		public string Name;
		public string File;

		public TextureWrap(string name, string file)
		{
			Name = name;
			File = file;
		}
	}

	public static class ItemTextures
	{
		public static Texture2D Bomb;
		public static Texture2D Bong;
		public static Texture2D Arrow;
		public static Texture2D Bucket;
		public static Texture2D BigPickaxe;
		public static Texture2D Helmet;
		public static Texture2D Chestplate;
		public static Texture2D Sword;
		public static Texture2D WalLScraper;
		public static Texture2D PickaxeNew;
		public static Texture2D Scroll;
		public static Texture2D Dynamite;
		public static Texture2D Workbench;
		public static Texture2D Potion;
		public static Texture2D Jetpack;
		public static Texture2D Door;
		public static Texture2D ForestPainting;
		public static Texture2D Ingot;
		public static Texture2D Leggings;

		

		public static void LoadAssets(ContentManager mgr) 
		{
			Texture2D Load(string file)
			{
				return mgr.Load<Texture2D>(file);
			}

			Arrow = Load("Items/arrow");
			Bomb = Load("Items/bomb");
			Bong = Load("Items/bong");
			Ingot = Load("Items/ingot");


		}

		/*public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

		private static string[] Items =
		{
			"Items/arrow",
			"Items/bomb",

		};
		public static Texture2D Arrow;

		public static void LoadAssets(ContentManager Content)
		{
			foreach(string item in Items)
			{
				Texture2D loaded = Content.Load<Texture2D>(item);
				Textures.Add(loaded.Name.Replace("Items/", ""), loaded);
			}
		}*/
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
