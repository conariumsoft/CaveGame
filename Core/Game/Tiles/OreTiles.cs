#if CLIENT
using CaveGame.Client;
using CaveGame.Core.Generic;
#endif
using DataManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Tiles
{
	#region Ores
	public class Ore : Tile, ILocalTileUpdate
	{
		public override byte Hardness => 12;
		public override Rectangle Quad => TileMap.Ore;
		private void DrawMask(GraphicsEngine gfx, int x, int y, Light3 color, int rotation, Rectangle quad, Color tilecolor)
		{
			var position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);
			var pixels4 = new Vector2(4, 4);
			gfx.Sprite(gfx.TileSheet, position + pixels4, quad, color.MultiplyAgainst(tilecolor), Rotation.FromDeg(rotation), new Vector2(4, 4), 1, SpriteEffects.None, 1);
		}

		private void DrawDirtMask(GraphicsEngine gfx, int x, int y, Light3 color, int rotation) => DrawMask(gfx, x, y, color, rotation, TileMap.DirtFading, Color.SaddleBrown);
		private void DrawStoneMask(GraphicsEngine gfx, int x, int y, Light3 color, int rot) => DrawMask(gfx, x, y, color, rot, TileMap.StoneFading, new Color(0.7f, 0.7f, 0.7f));

		public override void Draw(GraphicsEngine gfx, int x, int y, Light3 color)
		{
			var position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);

			gfx.Sprite(gfx.TileSheet, position, Quad, color.MultiplyAgainst(Color));

			//sb.End();

			if (TileState.Get(0)) // Top Dirt
				DrawDirtMask(gfx, x, y, color, 0);
			if (TileState.Get(1)) // Top Stone
				DrawStoneMask(gfx, x, y, color, 0);
			if (TileState.Get(2)) // Bottom Dirt
				DrawDirtMask(gfx, x, y, color, 180);
			if (TileState.Get(3)) // Bottom Stone
				DrawStoneMask(gfx, x, y, color, 180);

			if (TileState.Get(4)) // Left Dirt
				DrawDirtMask(gfx, x, y, color, 270);

			if (TileState.Get(5)) // Left Stone
				DrawStoneMask(gfx, x, y, color, 270);

			if (TileState.Get(6)) // Right Dirt
				DrawDirtMask(gfx, x, y, color, 90);

			if (TileState.Get(7)) // Right Stone
				DrawStoneMask(gfx, x, y, color, 90);

			//sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		}

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			var top = world.GetTile(x, y - 1);
			var bottom = world.GetTile(x, y + 1);
			var left = world.GetTile(x - 1, y);
			var right = world.GetTile(x + 1, y);

			byte val = 0;

			if (top is Dirt)
				val.Set(0, true);
			if (top is Stone)
				val.Set(1, true);
			if (bottom is Dirt)
				val.Set(2, true);
			if (bottom is Stone)
				val.Set(3, true);
			if (left is Dirt)
				val.Set(4, true);
			if (left is Stone)
				val.Set(5, true);
			if (right is Dirt)
				val.Set(6, true);
			if (right is Stone)
				val.Set(7, true);

			TileState = val;
		}
	}

	public class CopperOre : Ore
	{
		public override Color Color => new Color(1.0f, 0.45f, 0f);
	}
	public class LeadOre : Ore
	{
		public override Color Color => new Color(0.35f, 0.35f, 0.4f);
	}
	public class TinOre : Ore
	{
		public override Color Color => new Color(0.65f, 0.4f, 0.4f);
	}
	public class ChromiumOre : Ore {
		public override Color Color => new Color(0.8f, 0.8f, 0.8f);
	}
	public class AluminiumOre : Ore {
		public override Color Color => new Color(0.5f, 0.5f, 0.5f);
	}
	public class IronOre : Ore
	{
		public override Color Color => new Color(1.0f, 0.75f, 0.75f);
	}
	public class NickelOre : Ore {
		public override Color Color => new Color(0.9f, 0.8f, 0.75f);
	}
	public class CobaltOre : Ore
	{
		public override Color Color => new Color(0.3f, 0.3f, 1f);
	}
	public class GoldOre : Ore
	{
		public override Color Color => new Color(1f, 1f, 0.5f);
	}
	public class TitaniumOre : Ore {
		public override Color Color => new Color(1f, 1f, 1.0f);
	}

	public class ArsenicOre : Ore
    {
		public override Color Color => new Color(1f, 1f, 1.0f);
	}

	public class GalliumOre : Ore
    {
		public override Color Color => new Color(1f, 1f, 1.0f);
	}
	public class UraniumOre : Ore, ILightEmitter
	{
		public Light3 Light => new Light3(4, 8, 4);
		public override Color Color => new Color(0.35f, 0.8f, 0.35f);
	}

	#endregion
}
