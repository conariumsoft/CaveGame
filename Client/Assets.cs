using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
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
		public static bool ContentLoaded = false;

		public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

		public static Texture2D Player => Textures["Entities/player.png"];
		public static Texture2D TitleScreen => Textures["TitleScreen.png"];
		public static Texture2D EyeOfHorus => Textures["csoft.png"];
		public static Texture2D ParticleSet => Textures["particles.png"];
		public static Texture2D TileSheet => Textures["tilesheet.png"];
		public static Texture2D BG => Textures["bg.png"];


		public static void LoadAssets(GraphicsDevice graphicsDevice)
		{
			foreach (var tex in Directory.GetFiles("Assets/Textures/", "*.png"))
			{
				Texture2D loaded = AssetLoader.LoadTexture(graphicsDevice, tex);
				Textures.Add(tex.Replace("Assets/Textures/", ""), loaded);
			}
			foreach (var tex in Directory.GetFiles("Assets/Entities/", "*.png"))
			{
				Texture2D loaded = AssetLoader.LoadTexture(graphicsDevice, tex);
				Textures.Add(tex.Replace("Assets/", ""), loaded);
			}
			ContentLoaded = true;
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

	public static class AssetLoader
	{
		public static Texture2D FromStream(GraphicsDevice GraphicsProcessor, Stream DataStream)
		{

			Texture2D Asset = Texture2D.FromStream(GraphicsProcessor, DataStream);
			// Fix Alpha Premultiply
			Color[] data = new Color[Asset.Width * Asset.Height];
			Asset.GetData(data);
			for (int i = 0; i != data.Length; ++i)
				data[i] = Color.FromNonPremultiplied(data[i].ToVector4());
			Asset.SetData(data);
			return Asset;
		}

		public static Texture2D LoadTexture(GraphicsDevice device, string filepath)
		{
			return FromStream(device, TitleContainer.OpenStream(filepath));
		}


	}


	public static class ItemTextures
	{
		public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

		private static TextureMeta[] texturelist =
		{
			//new TextureMeta("Arrow",	   "Assets/Items/arrow.png"),
			//new TextureMeta("Bomb",		   "Assets/Items/bomb.png"),
			//new TextureMeta("Bong",		   "Assets/Items/bong.png"),
			////new TextureMeta("Ingot",	   "Assets/Items/ingot.png"),
			//new TextureMeta("BigPickaxe",  "Assets/Items/bigpickaxe"),
		//	new TextureMeta("WallScraper", "Assets/Items/wallscraper"),
		};

		public static void LoadAssets(GraphicsDevice graphicsDevice)
		{
			foreach (var tex in Directory.GetFiles("Assets/Items/", "*.png"))
			{
				Texture2D loaded = AssetLoader.LoadTexture(graphicsDevice, tex);
				Textures.Add(tex.Replace("Assets/Items/", ""), loaded);
			}

			// if you want to hardcode textures like a dumbass
			foreach (TextureMeta textureMeta in texturelist)
			{
				Texture2D loaded = AssetLoader.LoadTexture(graphicsDevice, textureMeta.FilePath);
				Textures.Add(textureMeta.Name, loaded);
			}
		}

		public static Texture2D Bomb		=> Textures["bomb.png"];
		public static Texture2D Bong		=> Textures["bong.png"];
		public static Texture2D Arrow		=> Textures["arrow.png"];
		public static Texture2D Bucket		=> Textures["bucket.png"];
		public static Texture2D BigPickaxe	=> Textures["bigpickaxe.png"];
		public static Texture2D Helmet		=> Textures["helmet.png"];
		public static Texture2D Chestplate  => Textures["chestplate.png"];
		public static Texture2D Sword       => Textures["sword.png"];
		public static Texture2D WallScraper => Textures["wallscraper.png"];
		public static Texture2D PickaxeNew  => Textures["pickaxenew.png"];
		public static Texture2D Scroll		=> Textures["scroll.png"];
		public static Texture2D Dynamite	=> Textures["dynamite.png"];
		public static Texture2D Workbench	=> Textures["workbench.png"];
		public static Texture2D Potion		=> Textures["potion.png"];
		public static Texture2D Jetpack		=> Textures["jetpack.png"];
		public static Texture2D Door		=> Textures["door.png"];
		public static Texture2D ForestPainting=>Textures["forestpainting.png"];
		public static Texture2D Ingot		=> Textures["ingot.png"];
		public static Texture2D Leggings	=> Textures["leggings.png"];
		public static Texture2D Furnace => Textures["furnace.png"];
	}

	public struct TextureMeta
	{
		public string Name { get; private set; }
		public string FilePath { get; private set; }
		public TextureMeta(string name, string file)
		{
			Name = name;
			FilePath = file;
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
