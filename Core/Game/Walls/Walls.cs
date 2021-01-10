#if CLIENT
using CaveGame.Client;
#endif 
using CaveGame.Core.Game.Tiles;
using DataManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaveGame.Core.Game.Walls
{
	public enum WallID : short
	{
		Void = 255,
		Air = 0,
		Stone = 1, 
		Dirt, 
		OakPlank, 
		RedwoodPlank,
		PinePlank,
		EbonyPlank,
		StoneBrick, 
		ClayBrick,
		SandstoneBrick,
		MossyStone, 
		CarvedStoneBrick, 
		CarvedSandstoneBrick,
		MossyStoneBrick, 
		CubedStone, 
		CubedSandstone,
		Sandstone
	}

	public class Wall
	{
		public virtual byte Opacity => 12;
		public virtual Color Color => Color.Gray;
		public virtual Rectangle Quad => TileMap.Default;
		public virtual byte Hardness => 2;
		public virtual string Namespace => "CaveGame";
		public virtual string WallName => this.GetType().Name;

		public virtual short ID => (short) Enum.Parse(typeof(WallID), GetType().Name);

		public static Color BGDarken = new Color(92, 92, 92);


		public static Random RNG = new Random();

		public byte Damage { get; set; }

		public byte[] Serialize()
		{
			byte[] serializedTile = new byte[4];
			Encode(ref serializedTile, 0);
			return serializedTile;
		}

		public void Serialize(ref byte[] datagram, int pushindex) => Encode(ref datagram, pushindex);

		public virtual void Encode(ref byte[] datagram, int pushIndex)
		{
			datagram.WriteShort(pushIndex+0, ID);
			datagram[2 + pushIndex] = Damage;
			datagram[3 + pushIndex] = 0; // reserved for future uses
		}

		public virtual void Decode(ref byte[] datagram, int pullIndex) {}

		public static Wall Deserialize(ref byte[] datagram, int pullIndex)
		{
			Wall w = Wall.FromID(datagram.ReadShort(pullIndex));
			w.Decode(ref datagram, pullIndex);
			//w.Damage = datagram[pullIndex+1];
			return w;
		}

		public static byte IDOf<T>()
		{
			var type = typeof(T);
			byte id = (byte)Enum.Parse(typeof(WallID), type.Name);
			return id;
		}

		public static Wall FromID(short t)
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

		public virtual void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			GFX.Sprite(
				GFX.TileSheet, 
				new Vector2(x * Globals.TileSize, y * Globals.TileSize), 
				Quad, color.MultiplyAgainst(Color)
			);
		}

        internal static Wall FromName(string name)
        {
			var basetype = typeof(Wall);
			var types = basetype.Assembly.GetTypes().Where(type => type.IsSubclassOf(basetype));


			foreach (var type in types)
			{
				if (name == type.Name)
					return (Wall)type.GetConstructor(Type.EmptyTypes).Invoke(null);
			}
			throw new Exception("ID not valid!");
		}
    }


	public class Void : Wall
	{
		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color) { }
	}
	public class Air : Wall, INonSolid {
		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color) { }
	}

	public abstract class RockWall : Wall
	{
        public override byte Hardness => 5;
        public override Rectangle Quad => TileMap.Stone;
		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
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

			GFX.Sprite(
				GFX.TileSheet,
				new Vector2(x * Globals.TileSize, y * Globals.TileSize),
				quad, color.MultiplyAgainst(Color)
			);
		}
	}

	public class Stone : RockWall {
       
        public override Color Color => new Color(0.4f, 0.4f, 0.4f);
    }
	public class Sandstone : RockWall
	{
        public override Color Color => new Color(0.3f, 0.3f, 0.0f);
    }
	public class Dirt : Wall {
        public override Color Color => new Color(40, 20, 5);
        public override Rectangle Quad => TileMap.Soil;

    }
	public class OakPlank : Wall {
        public override Rectangle Quad => TileMap.Plank;
        public override Color Color => new Color(0.5f, 0.3f, 0.15f);
    }
	public class RedwoodPlank : Wall
    {
		public override Rectangle Quad => TileMap.Plank;
        public override Color Color => new Color(100, 50, 25);
    }
	public class PinePlank : Wall
    {
		public override Rectangle Quad => TileMap.Plank;
		public override Color Color => new Color(90, 40, 55);
    }
	public class EbonyPlank : Wall
    {
		public override Rectangle Quad => TileMap.Plank;
        public override Color Color => new Color(30, 30, 30);
    }

	public abstract class Brick : Wall
	{
        public override byte Hardness => 5;
        public override Rectangle Quad => TileMap.Brick;



        public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
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

			GFX.Sprite(
				GFX.TileSheet,
				new Vector2(x * Globals.TileSize, y * Globals.TileSize),
				quad, color.MultiplyAgainst(Color)
			);
		}
	}

	public class StoneBrick : Brick
	{
        public override Color Color => new Color(0.4f, 0.4f, 0.4f);
    }
	public class ClayBrick : Brick
	{
        public override Color Color => new Color(0.65f, 0.65f, 0.65f);
    }
	public class SandstoneBrick: Brick
	{
        public override Color Color => new Color(0.3f, 0.3f, 0.0f);
    }
	public class MossyStone : Wall
	{
        public override Rectangle Quad => TileMap.StoneMossy;
        public override Color Color => new Color(0.3f, 0.3f, 0.3f);
    }
	public class MossyStoneBrick : Wall
	{
        public override Color Color => new Color(0.3f, 0.3f, 0.3f);
        public override Rectangle Quad => TileMap.MossyBrick;
    }
	public class CarvedStoneBrick : Wall
	{
        public override Rectangle Quad => TileMap.Carved;
        public override Color Color => new Color(0.3f, 0.3f, 0.3f);
    }
	public class CarvedSandstoneBrick : Wall
	{
		public override Rectangle Quad => TileMap.Carved;
		public override Color Color => new Color(0.3f, 0.3f, 0.0f);
	}
	public class CubedStone : Wall
	{
		public override Rectangle Quad => TileMap.StoneCubes;
		public override Color Color => new Color(0.3f, 0.3f, 0.3f);
	}
	public class CubedSandstone : Wall
	{
		public override Color Color => new Color(0.3f, 0.3f, 0.0f);
		public override Rectangle Quad => TileMap.StoneCubes;
    }
}
