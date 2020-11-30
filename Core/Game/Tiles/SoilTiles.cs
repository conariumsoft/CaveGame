using CaveGame.Core.Game.Tiles;
using CaveGame.Core.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Tiles
{

	public class Soil : Tile, ISoil
	{
		public override byte Hardness => 2;
		public override Rectangle Quad => TileMap.Soil;
	}
	public class Dirt : Soil
	{
		public override Color Color => Color.SaddleBrown;
	}
	public class Mud : Soil
	{
		public override Color Color => new Color(0.3f, 0.1f, 0.1f);
	}
	public class Sand : Tile, ITileUpdate
	{
		public override Rectangle Quad => TileMap.Sand;
        public override Color Color => new Color(0.9f, 0.9f, 0.5f);

        public void TileUpdate(IGameWorld world, int x, int y)
        {
            if (world.IsTile<INonSolid>(x, y+1))
            {
				if (world.IsTile<ILiquid>(x, y+1))
                {
					world.SetTile(x, y, world.GetTile(x, y + 1));
                } else
                {
					world.SetTile(x, y, new Air());
				}
				
				world.SetTile(x, y + 1, new Sand());
            }
        }

       public override void Draw(GraphicsEngine gfx, int x, int y, Light3 color)
        {
			var rngval = RNGIntMap[x, y];
			if (rngval % 2 == 0)
            {
				gfx.Sprite(
					gfx.TileSheet,
					new Vector2(x * Globals.TileSize, y * Globals.TileSize),
					Quad, color.MultiplyAgainst(Color), Rotation.Zero,
					Vector2.Zero, 1, SpriteEffects.None, 0
				);
			} else if (rngval % 2 == 1)
            {
				gfx.Sprite(
					gfx.TileSheet,
					new Vector2(x * Globals.TileSize, y * Globals.TileSize),
					Quad, color.MultiplyAgainst(Color), Rotation.Zero,
					Vector2.Zero, 1, SpriteEffects.FlipVertically, 0
				);
			} else
            {
				gfx.Sprite(
					gfx.TileSheet,
					new Vector2(x * Globals.TileSize, y * Globals.TileSize),
					Quad, color.MultiplyAgainst(Color), Rotation.Zero,
					Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0
				);
			}
        }

    }
	public class Snow : Tile
	{
		public override Rectangle Quad => TileMap.Soil;
		public override Color Color => new Color(0.92f, 0.92f, 0.92f);
		public override byte Hardness => 3;
	}
	public class Clay : Soil
	{
		public override Color Color => new Color(0.6f, 0.2f, 0.2f);
	}
}
