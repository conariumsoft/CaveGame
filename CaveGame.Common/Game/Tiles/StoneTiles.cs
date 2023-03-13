
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using CaveGame.Common.Extensions;

namespace CaveGame.Common.Game.Tiles
{
	public class Stone : Tile, ILocalTileUpdate
	{
		public override Color Color => new Color(0.7f, 0.7f, 0.7f);
		public override Rectangle Quad => TileMap.Stone;
		public override byte Hardness => 4;

		private void DrawMask(GraphicsEngine GFX, int x, int y, Light3 color, int rotation, Rectangle quad, Color tilecolor)
		{
			//if (GameGlobals.GraphicsDevice != null)
			//{
			var position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);
			var pixels4 = new Vector2(4, 4);

			GFX.Sprite(GFX.TileSheet, position + pixels4, quad, color.MultiplyAgainst(tilecolor), Rotation.FromDeg(rotation), new Vector2(4, 4), 1, SpriteEffects.None, 1);
			//	}
		}

		private void DrawDirtMask(GraphicsEngine GFX, int x, int y, Light3 color, int rotation)
		{
			DrawMask(GFX, x, y, color, rotation, TileMap.DirtFading, Color.SaddleBrown);
		}


		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			var position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);

			GFX.Sprite(GFX.TileSheet, position, Quad, color.MultiplyAgainst(Color));

			if (TileState.Get(0)) // Top Dirt
				DrawDirtMask(GFX, x, y, color, 0);
			if (TileState.Get(1)) // Bottom Dirt
				DrawDirtMask(GFX, x, y, color, 180);
			if (TileState.Get(2)) // Left Dirt
				DrawDirtMask(GFX, x, y, color, 270);
			if (TileState.Get(3)) // Right Dirt
				DrawDirtMask(GFX, x, y, color, 90);
		}


		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			var top = world.GetTile(x, y - 1);
			var bottom = world.GetTile(x, y + 1);
			var left = world.GetTile(x - 1, y);
			var right = world.GetTile(x + 1, y);

			byte val = 0;

			if (top is Dirt || top is Grass || top is Mycelium)
				val.Set(0, true);
			if (bottom is Dirt || bottom is Grass || bottom is Mycelium)
				val.Set(1, true);
			if (left is Dirt || left is Grass || left is Mycelium)
				val.Set(2, true);
			if (right is Dirt || right is Grass || right is Mycelium)
				val.Set(3, true);

			if (val != TileState)
			{
				TileState = val;
				world.DoUpdatePropogation(x, y);
			}

		}
	}
	public class Sandstone : Tile
	{
		public override byte Hardness => 5;
		public override Color Color => new Color(1.0f, 1.0f, 0.3f);
		public override Rectangle Quad => TileMap.Stone;

	}
	public class Glass : Tile, ILocalTileUpdate
	{
		public override byte Opacity => 0;
		public override byte Hardness => 3;
		public override Color Color => Color.White;
		public override Rectangle Quad => TileMap.Glass;

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			bool emptytop = IsEmpty(world, x, y - 1);
			bool emptybottom = IsEmpty(world, x, y + 1);
			bool emptyleft = IsEmpty(world, x - 1, y);
			bool emptyright = IsEmpty(world, x + 1, y);

			byte newNumber = TileState;
			newNumber.Set(3, emptyright);
			newNumber.Set(2, emptybottom);
			newNumber.Set(1, emptyleft);
			newNumber.Set(0, emptytop);

			if (TileState != newNumber)
			{
				TileState = newNumber;
				world.DoUpdatePropogation(x, y);
			}
		}

		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			var gx = TileMap.Glass.X;
			var gy = TileMap.Glass.Y;
			var top = new Rectangle(gx, gy, 8, 1);
			var left = new Rectangle(gx, gy, 1, 8);
			var bottom = new Rectangle(gx, gy + 7, 8, 1);
			var right = new Rectangle(gx + 7, gy, 1, 8);

			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);


			GFX.Sprite(GFX.TileSheet, position, TileMap.GlassBG, color.MultiplyAgainst(Color));

			if (TileState.Get(3)) // Right
				GFX.Sprite(GFX.TileSheet, position + new Vector2(7, 0), right, color.MultiplyAgainst(Color));

			if (TileState.Get(2)) // Bottom
				GFX.Sprite(GFX.TileSheet, position + new Vector2(0, 7), bottom, color.MultiplyAgainst(Color));

			if (TileState.Get(1)) // Left
				GFX.Sprite(GFX.TileSheet, position, left, color.MultiplyAgainst(Color));

			if (TileState.Get(0)) // Top
				GFX.Sprite(GFX.TileSheet, position, top, color.MultiplyAgainst(Color));
		}
		//sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, y * Globals.TileSize), TileMap.Soil, color.MultiplyAgainst(Color.SaddleBrown));

	}

	public class Brick : Tile, ILocalTileUpdate
	{
		public override Rectangle Quad => TileMap.Brick;
		public override byte Hardness => 12;
		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			//sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, y * Globals.TileSize), TileMap.Soil, color.MultiplyAgainst(Color.SaddleBrown));
			var TL = new Rectangle(0, Globals.TileSize, 4, 4);
			var TR = new Rectangle(4, Globals.TileSize, 4, 4);
			var BL = new Rectangle(0, Globals.TileSize + 4, 4, 4);
			var BR = new Rectangle(4, Globals.TileSize + 4, 4, 4);

			var RTL = new Rectangle(9 * Globals.TileSize, Globals.TileSize, 4, 4);
			var RTR = new Rectangle(9 * Globals.TileSize + 4, Globals.TileSize, 4, 4);
			var RBL = new Rectangle(9 * Globals.TileSize, Globals.TileSize + 4, 4, 4);
			var RBR = new Rectangle(9 * Globals.TileSize + 4, Globals.TileSize + 4, 4, 4);

			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);


			if (TileState.Get(3)) // BottomRight
				GFX.Sprite(GFX.TileSheet, position + new Vector2(4, 4), RBR, color.MultiplyAgainst(Color));
			else
				GFX.Sprite(GFX.TileSheet, position + new Vector2(4, 4), BR, color.MultiplyAgainst(Color));

			if (TileState.Get(2)) // BottomLeft
				GFX.Sprite(GFX.TileSheet, position + new Vector2(0, 4), RBL, color.MultiplyAgainst(Color));
			else
				GFX.Sprite(GFX.TileSheet, position + new Vector2(0, 4), BL, color.MultiplyAgainst(Color));

			if (TileState.Get(1)) // TopLeft
				GFX.Sprite(GFX.TileSheet, position, RTL, color.MultiplyAgainst(Color));
			else
				GFX.Sprite(GFX.TileSheet, position, TL, color.MultiplyAgainst(Color));

			if (TileState.Get(0)) // TopRight
				GFX.Sprite(GFX.TileSheet, position + new Vector2(4, 0), RTR, color.MultiplyAgainst(Color));
			else
				GFX.Sprite(GFX.TileSheet, position + new Vector2(4, 0), TR, color.MultiplyAgainst(Color));

		}

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			bool planetop = IsEmpty(world, x, y - 1);
			bool planebottom = IsEmpty(world, x, y + 1);
			bool planeleft = IsEmpty(world, x - 1, y);
			bool planeright = IsEmpty(world, x + 1, y);

			byte newNumber = TileState;
			newNumber.Set(3, planebottom && planeright);
			newNumber.Set(2, planeleft && planebottom);
			newNumber.Set(1, planeleft && planetop);
			newNumber.Set(0, planeright && planetop);

			if (TileState != newNumber)
			{
				TileState = newNumber;
				world.DoUpdatePropogation(x, y);
			}
		}
	}
	public class StoneBrick : Brick
	{
		public override Color Color => new Color(0.7f, 0.7f, 0.7f);
        
    }
	public class ClayBrick : Brick
	{
		public override Rectangle Quad => TileMap.RedBrick;

		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			//sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, y * Globals.TileSize), TileMap.Soil, color.MultiplyAgainst(Color.SaddleBrown));
			var TL = new Rectangle(0, (Globals.TileSize * 3), 4, 4);
			var TR = new Rectangle(4, (Globals.TileSize * 3), 4, 4);
			var BL = new Rectangle(0, (Globals.TileSize * 3) + 4, 4, 4);
			var BR = new Rectangle(4, (Globals.TileSize * 3) + 4, 4, 4);

			var RTL = new Rectangle(10 * Globals.TileSize, Globals.TileSize, 4, 4);
			var RTR = new Rectangle(10 * Globals.TileSize + 4, Globals.TileSize, 4, 4);
			var RBL = new Rectangle(10 * Globals.TileSize, Globals.TileSize + 4, 4, 4);
			var RBR = new Rectangle(10 * Globals.TileSize + 4, Globals.TileSize + 4, 4, 4);

			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);


			if (TileState.Get(3)) // BottomRight
				GFX.Sprite(GFX.TileSheet, position + new Vector2(4, 4), RBR, color.MultiplyAgainst(Color));
			else
				GFX.Sprite(GFX.TileSheet, position + new Vector2(4, 4), BR, color.MultiplyAgainst(Color));

			if (TileState.Get(2)) // BottomLeft
				GFX.Sprite(GFX.TileSheet, position + new Vector2(0, 4), RBL, color.MultiplyAgainst(Color));
			else
				GFX.Sprite(GFX.TileSheet, position + new Vector2(0, 4), BL, color.MultiplyAgainst(Color));

			if (TileState.Get(1)) // TopLeft
				GFX.Sprite(GFX.TileSheet, position, RTL, color.MultiplyAgainst(Color));
			else
				GFX.Sprite(GFX.TileSheet, position, TL, color.MultiplyAgainst(Color));

			if (TileState.Get(0)) // TopRight
				GFX.Sprite(GFX.TileSheet, position + new Vector2(4, 0), RTR, color.MultiplyAgainst(Color));
			else
				GFX.Sprite(GFX.TileSheet, position + new Vector2(4, 0), TR, color.MultiplyAgainst(Color));


		}
	}
	public class MudBrick : Brick
	{

		public override Color Color => new Color(0.3f, 0.1f, 0.1f);
	}
	public class SandBrick : Brick
	{
		public override Color Color => new Color(0.8f, 0.8f, 0.3f);
	}
	public class Cobblestone { }

	public class Limestone { }
	public class Obsidian : Tile
	{
		public override Color Color => new Color(0.5f, 0.1f, 0.5f);
		public override Rectangle Quad => TileMap.StoneLight; // TODO: custom texture
		public override byte Hardness => 20;
	}
	public class Brimstone { }
	public class Ice : Tile
	{
		public override Rectangle Quad => TileMap.Soil; // TODO: custom texture
		public override Color Color => new Color(0.8f, 0.8f, 1.0f);
		public override byte Hardness => 6;
	}
	public class IceBrick : Brick
	{
		public override Color Color => new Color(0.8f, 0.8f, 1.0f);
	}
	public class MossyStone : Tile
	{
		public override Color Color => new Color(0.7f, 0.7f, 0.7f);
		public override byte Hardness => 6;
		public override Rectangle Quad => TileMap.StoneMossy;
	}
	public class MossyStoneBrick : Tile
	{

		public override byte Hardness => 8;
		public override Rectangle Quad => TileMap.MossyBrick;
		public override Color Color => new Color(0.7f, 0.7f, 0.7f);
	}
	public class CarvedStoneBrick : Tile
	{
		public override byte Hardness => 12;
		public override Rectangle Quad => TileMap.Carved;
		public override Color Color => new Color(0.7f, 0.7f, 0.7f);
	}
	public class CarvedSandBrick : Tile
	{
		public override byte Hardness => 12;
		public override Rectangle Quad => TileMap.Carved;
		public override Color Color => new Color(0.8f, 0.8f, 0.0f);
	}
	public class CubedStone : Tile
	{
		public override byte Hardness => 16;
		public override Rectangle Quad => TileMap.StoneCubes;
		public override Color Color => new Color(0.7f, 0.7f, 0.7f);
	}
	public class CubedSandstone : Tile
	{
		public override byte Hardness => 16;
		public override Rectangle Quad => TileMap.StoneCubes;
		public override Color Color => new Color(0.8f, 0.8f, 0.0f);
	}
}
