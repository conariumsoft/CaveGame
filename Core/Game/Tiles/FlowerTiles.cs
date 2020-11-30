using DataManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Tiles
{
	public class Flower : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
		public override byte Opacity => 1;
		public override byte Hardness => 1;
	}

	public class CryingLily : Tile, ILightEmitter, INonSolid, ITileUpdate, IWaterBreakable
	{
		public Light3 Light => new Light3(2, 4, 6);
		public override Rectangle Quad => TileMap.CryingLily;

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y - 1) is Mycelium))
				world.SetTile(x, y, new Air());


		}

		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			SpriteEffects effects = SpriteEffects.None;
			Vector2 position = new Vector2((Globals.TileSize * x) + (x % 3) - (y % 6), (Globals.TileSize * y));

			if ((x + y).Mod(2) == 0)
			{
				effects = SpriteEffects.FlipHorizontally;
			}
			GFX.Sprite(GFX.TileSheet, position, TileMap.CryingLily, color.MultiplyAgainst(Color), Rotation.Zero, Vector2.Zero, Vector2.One, effects, 0);
			//base.Draw(tilesheet, sb, x, y, color);
		}
	}
	public class HexenRoses : Flower
	{
		public override Rectangle Quad => TileMap.Flowers;
	}
	public class Bamboo { }
	public class Sugarcane { }
	public class WoodMushroom { }
	public class PinkOysterMushroom { }
	public class CubensisMushroom { }
	public class ChestnutMushroom { }



	public class BlackEyedSusan : Flower
	{
		public override Rectangle Quad => TileMap.BlackEyedSusan;
	}

	public class Chrysanthemum : Flower
	{
		public override Rectangle Quad => TileMap.Chrysanthemum;
	}
	public class ForgetMeNot : Flower
	{
		public override Rectangle Quad => TileMap.ForgetMeNot;
	}
	public class Honeysuckle : Flower
	{
		public override Rectangle Quad => TileMap.Honeysuckle;
	}
	public class Hydrangea : Flower
	{
		public override Rectangle Quad => TileMap.Hydrangea;
	}
	public class Lily : Flower
	{
		public override Rectangle Quad => TileMap.Lily;
	}
	public class Magnolia : Flower
	{
		public override Rectangle Quad => TileMap.Magnolia;
	}
	public class Orchid : Flower
	{
		public override Rectangle Quad => TileMap.Orchid;
	}
	public class Plumeria : Flower
	{
		public override Rectangle Quad => TileMap.Plumeria;

	}
	public class Tulip : Flower
	{
		public override Rectangle Quad => TileMap.Tulip;
	}
	public class Cornflower { }
	public class Begonia { }
	public class BleedingHeart { }
	public class Poppy : Flower
	{
		public override Rectangle Quad => TileMap.Poppy;
	}
}
