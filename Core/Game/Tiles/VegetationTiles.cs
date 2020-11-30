using DataManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Tiles
{

	public class BlueOysterMushroom : Tile, ILightEmitter, INonSolid, ITileUpdate, IWaterBreakable, ILocalTileUpdate
	{
		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is Mycelium))
				world.SetTile(x, y, new Air());
		}

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{

		}

		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);
			if (TileState == 0) // Upright
				GFX.Sprite(GFX.TileSheet, position, Quad, color.MultiplyAgainst(Color), Rotation.Zero, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
			if (TileState == 1) // Right
				GFX.Sprite(GFX.TileSheet, position, Quad, color.MultiplyAgainst(Color), Rotation.FromDeg(90), Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
			if (TileState == 2) // Bottom
				GFX.Sprite(GFX.TileSheet, position, Quad, color.MultiplyAgainst(Color), Rotation.FromDeg(180), Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
			if (TileState == 3) // Left
				GFX.Sprite(GFX.TileSheet, position, Quad, color.MultiplyAgainst(Color), Rotation.FromDeg(270), Vector2.Zero, Vector2.One, SpriteEffects.None, 0);

			//base.Draw(tilesheet, sb, x, y, color);
		}

		public Light3 Light => new Light3(2, 3, 3);

	}
	public class Cactus { }
	public class CactusFlower { }
	public class Vine : Tile, IRandomTick, INonSolid, ITileUpdate, IWaterBreakable
	{

		public override Rectangle Quad => TileMap.Vine;
		public override byte Hardness => 1;
		public override byte Opacity => 2;
		public override Color Color => Color.Green;

		public void RandomTick(IGameWorld world, int x, int y)
		{
			var below = world.GetTile(x, y + 1);
			if (below is Air)
			{
				world.SetTile(x, y + 1, new Vine());
			}
		}

		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			Vector2 position = new Vector2((x * Globals.TileSize) + (x.Mod(4)), (y * Globals.TileSize));

			GFX.Sprite(GFX.TileSheet, position, Quad, color.MultiplyAgainst(Color));
		}

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			var above = world.GetTile(x, y - 1);
			if (!(above is Vine || above is Grass || above is Mycelium))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class Tallgrass : Tile, INonSolid, ITileUpdate, IWaterBreakable, IRandomTick
	{
		public override byte Hardness => 1;
		public override Rectangle Quad => TileMap.TallGrass;
		public override Color Color => Color.Green;
		public override byte Opacity => 1;

		public void RandomTick(IGameWorld world, int x, int y)
		{
			if (RNG.NextDouble() > 0.95f)
				world.SetTile(x, y, new Air()); // grass sometimes randomly dies
		}

		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);
			Rectangle quad = TileMap.TallGrass;

			int state = ((int)x).Mod(3);

			if (state == 2)
				quad = TileMap.TallGrass2;
			if (state == 1)
				quad = TileMap.TallGrass3;

			GFX.Sprite(GFX.TileSheet, position, quad, color.MultiplyAgainst(Color));
			//base.Draw(tilesheet, sb, x, y, color);
		}

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			TileState++;
			var below = world.GetTile(x, y + 1);
			if (!(below is Grass))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class BlueTallgrass : Tile, INonSolid, ITileUpdate, IWaterBreakable, IRandomTick
	{
		public override byte Opacity => 1;
		public override byte Hardness => 1;
		public override Rectangle Quad => TileMap.TallGrass;
		public override Color Color => Color.Blue;

		public void RandomTick(IGameWorld world, int x, int y)
		{
			if (RNG.NextDouble() > 0.95f)
				world.SetTile(x, y, new Air()); // grass sometimes randomly dies
		}

		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);
			Rectangle quad = TileMap.TallGrass;

			int state = ((int)x).Mod(3);

			if (state == 2)
				quad = TileMap.TallGrass2;
			if (state == 1)
				quad = TileMap.TallGrass3;

			GFX.Sprite(GFX.TileSheet, position, quad, color.MultiplyAgainst(Color));
			//base.Draw(tilesheet, sb, x, y, color);
		}

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			TileState++;
			var below = world.GetTile(x, y + 1);
			if (!(below is Mycelium))
			{
				world.SetTile(x, y, new Air());
			}
		}


	}
}
