﻿#if CLIENT
using CaveGame.Client;
#endif 
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaveGame.Core.Walls
{
	public enum WallID : byte
	{
		Void = 255,
		Air = 0,
		Stone = 1, 
		Dirt, 
		RockyDirt, 
		OakPlank, 
		StoneBrick, 
		ClayBrick,
		SandstoneBrick,
		MossyStone, CarvedStoneBrick, CarvedSandstoneBrick,
		MossyStoneBrick, CubedStone, CubedSandstone, Sandstone
	}

	public class WDef : ILightEmitter // TileData
	{
		public byte ID { get; set; }
		public Color Color { get; set; }
		public Rectangle Quad { get; set; }
		public byte Opacity { get; set; }
		public byte Hardness { get; set; }
		public Light3 Light { get; set; }
		public string Namespace { get; set; }

		public WDef()
		{
			Color = Color.White;
			Hardness = 4;
			Opacity = 0;
			Namespace = "CaveGame";
		}
	}

	public static class WallDefinitions
	{
		public static WDef Void		  = new WDef { };
		public static WDef Air		  = new WDef {
			Hardness = 0, Opacity = 0, Quad = TileMap.Default };
		public static WDef Dirt		  = new WDef { 
			Hardness = 2, Quad = TileMap.Soil, Color = new Color(40, 20, 5) 
		};
		public static WDef Stone	  = new WDef {
			Hardness = 5, Quad = TileMap.Stone, Color = TileDefinitions.Stone.Color*0.5f
		};
		public static WDef RockyDirt  = new WDef {
			Hardness = 5, Quad = TileMap.StoneSpot, Color = TileDefinitions.Dirt.Color.Sub(Wall.BGDarken) 
		};
		public static WDef OakPlank	  = new WDef { 
			Hardness = 5, Quad = TileMap.Plank, Color = TileDefinitions.OakPlank.Color*0.7f
		};
		public static WDef ClayBrick  = new WDef {
			Hardness = 5, Quad = TileMap.Brick, Color = TileDefinitions.ClayBrick.Color*0.5f
		};
		public static WDef StoneBrick = new WDef { 
			Hardness = 5, Quad = TileMap.Brick, Color = new Color(0.4f, 0.4f, 0.4f)
		};
		public static WDef Sandstone = new WDef
		{
			Hardness = 5,
			Quad = TileMap.Stone,
			Color = new Color(0.3f, 0.3f, 0.0f),
		};
		public static WDef SandstoneBrick = new WDef {
			Hardness = 5,
			Quad = TileMap.Brick,
			Color = new Color(0.3f, 0.3f, 0.0f)
		};
		public static WDef MossyStone = new WDef
		{
			Hardness = 5,
			Quad = TileMap.StoneMossy,
			Color = new Color(0.3f, 0.3f, 0.3f)
		};
		public static WDef CarvedStoneBrick = new WDef
		{
			Hardness = 5,
			Quad = TileMap.Carved,
			Color = new Color(0.3f, 0.3f, 0.3f)
		};
		public static WDef CarvedSandstoneBrick = new WDef
		{
			Hardness = 5,
			Quad = TileMap.Carved,
			Color = new Color(0.3f, 0.3f, 0.0f)
		};
		public static WDef MossyStoneBrick = new WDef
		{
			Hardness = 5,
			Quad = TileMap.MossyBrick,
			Color = new Color(0.3f, 0.3f, 0.3f)
		};
		public static WDef CubedStone = new WDef
		{
			Color = new Color(0.3f, 0.3f, 0.3f),
			Quad = TileMap.StoneCubes,
			Hardness = 5,
		};
		public static WDef CubedSandstone = new WDef
		{
			Color = new Color(0.3f, 0.3f, 0.0f),
			Quad = TileMap.StoneCubes,
			Hardness = 5,
		};
	}

	public class Wall
	{
		public static Color BGDarken = new Color(92, 92, 92);

#if CLIENT
		public static Texture2D Tilesheet = GameTextures.TileSheet;
#else
		public static Texture2D Tilesheet;
#endif

		public static Random RNG = new Random();

		public byte Opacity => data.Opacity;
		public Color Color => data.Color;
		public Rectangle Quad => data.Quad;
		public byte Hardness => data.Hardness;
		public byte ID
		{
			get
			{
				var name = this.GetType().Name;
				return (byte)Enum.Parse(typeof(WallID), name);
			}
		}
		public string Namespace => data.Namespace;
		public string WallName => this.GetType().Name;

		private WDef data;

		public byte Damage { get; set; }

		public Wall(WDef wdata)
		{
			data = wdata;
		}

		public virtual byte[] Serialize()
		{
			byte[] serializedTile = new byte[4];

			serializedTile[0] = ID;
			serializedTile[1] = Damage;
			serializedTile[2] = 0; // reserved for future uses
			serializedTile[3] = 0; // reserved for future uses

			return serializedTile;
		}

		public virtual void Serialize(ref byte[] datagram, int pushIndex)
		{
			datagram[0 + pushIndex] = ID;
			datagram[1 + pushIndex] = Damage;
			datagram[2 + pushIndex] = 0; // reserved for future uses
			datagram[3 + pushIndex] = 0; // reserved for future uses
		}

		public static Wall Deserialize(ref byte[] datagram, int pullIndex)
		{
			Wall w = Wall.FromID(datagram[pullIndex]);
			w.Damage = datagram[pullIndex+1];
			return w;
		}

		public static byte IDOf<T>()
		{
			var type = typeof(T);
			byte id = (byte)Enum.Parse(typeof(WallID), type.Name);
			return id;
		}

		public static Wall FromID(byte t)
		{
			var basetype = typeof(Wall);
			var types = basetype.Assembly.GetTypes().Where(type => type.IsSubclassOf(basetype));


			foreach (var type in types)
			{
				bool exists = Enum.TryParse(type.Name, out WallID id);
				if (exists && id == (WallID)t)
					return (Wall)type.GetConstructor(Type.EmptyTypes).Invoke(null);
			}
			throw new Exception(String.Format("WallID not valid! {0}", t));
		}

		public virtual void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			sb.Draw(
				tilesheet, 
				new Vector2(x * Globals.TileSize, y * Globals.TileSize), 
				Quad, color.MultiplyAgainst(Color)
			);
		}
	}


	public class Void : Wall
	{
		public Void(): base(WallDefinitions.Void) { }
		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			
		}
	}
	public class Air : Wall, INonSolid {
		public Air() : base(WallDefinitions.Air) { }
		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{

		}
	}

	public abstract class RockWall : Wall
	{
		public RockWall(WDef def) : base(def) { }
		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			Rectangle quad = TileMap.BGBrickTL;

			if (x.Mod(2) == 0 && y.Mod(2) == 0)
				quad = TileMap.BGStoneTL;
			if (x.Mod(2) == 1 && y.Mod(2) == 0)
				quad = TileMap.BGStoneTR;
			if (x.Mod(2) == 0 && y.Mod(2) == 1)
				quad = TileMap.BGStoneBL;
			if (x.Mod(2) == 1 && y.Mod(2) == 1)
				quad = TileMap.BGStoneBR;
			//base.Dquad = TileMap.BGStoneBL;raw(tilesheet, sb, x, y, color);

			sb.Draw(
				tilesheet,
				new Vector2(x * Globals.TileSize, y * Globals.TileSize),
				quad, color.MultiplyAgainst(Color)
			);
		}
	}

	public class Stone : RockWall {
		public Stone() : base(WallDefinitions.Stone) { }
	}
	public class Sandstone : RockWall
	{
		public Sandstone() : base(WallDefinitions.Sandstone) { }
	}
	public class Dirt : Wall {
		public Dirt() : base(WallDefinitions.Dirt) { }
	}
	public class RockyDirt : Wall {
		public RockyDirt() : base(WallDefinitions.RockyDirt) { }
	}
	public class OakPlank : Wall {
		public OakPlank() : base(WallDefinitions.OakPlank) { }
	}

	public abstract class Brick : Wall
	{
		public Brick(WDef def) : base(def) { }
		
		

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			Rectangle quad = TileMap.BGBrickTL;

			if (x.Mod(2) == 0 && y.Mod(3) == 0)
				quad = TileMap.BGBrickTL;
			if (x.Mod(2) == 1 && y.Mod(3) == 0)
				quad = TileMap.BGBrickTR;
			if (x.Mod(2) == 0 && y.Mod(3) == 1)
				quad = TileMap.BGBrickML;
			if (x.Mod(2) == 1 && y.Mod(3) == 1)
				quad = TileMap.BGBrickMR;
			if (x.Mod(2) == 0 && y.Mod(3) == 2)
				quad = TileMap.BGBrickBL;
			if (x.Mod(2) == 1 && y.Mod(3) == 2)
				quad = TileMap.BGBrickBR;

			//base.Draw(tilesheet, sb, x, y, color);

			sb.Draw(
				tilesheet,
				new Vector2(x * Globals.TileSize, y * Globals.TileSize),
				quad, color.MultiplyAgainst(Color)
			);
		}
	}

	public class StoneBrick : Brick {

		public StoneBrick() : base(WallDefinitions.StoneBrick) { }
	}
	public class ClayBrick : Brick
	{

		public ClayBrick() : base(WallDefinitions.ClayBrick) { }
	}
	public class SandstoneBrick: Brick
	{
		public SandstoneBrick() : base(WallDefinitions.SandstoneBrick) { }
	}
	public class MossyStone : Wall
	{
		public MossyStone() : base(WallDefinitions.MossyStone) { }
	}
	public class MossyStoneBrick : Wall
	{
		public MossyStoneBrick() : base(WallDefinitions.MossyStoneBrick) { }
	}
	public class CarvedStoneBrick : Wall
	{
		public CarvedStoneBrick() : base(WallDefinitions.CarvedStoneBrick) { }
	}
	public class CarvedSandstoneBrick : Wall
	{
		public CarvedSandstoneBrick() : base(WallDefinitions.CarvedSandstoneBrick) { }
	}
	public class CubedStone : Wall
	{
		public CubedStone() : base(WallDefinitions.CubedStone) { }
	}
	public class CubedSandstone : Wall
	{
		public CubedSandstone(): base(WallDefinitions.CubedSandstone) { }
	}
}