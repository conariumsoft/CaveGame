#if CLIENT
using CaveGame.Client;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Tiles
{

	public abstract class BaseTorch : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public override Color Color => FlameColor;
		public virtual Color FlameColor => Color.White;
		public override byte Hardness => 1;
		public override byte Opacity => 0;
		public override Rectangle Quad => TileMap.Torch;

		public static Rectangle[] AnimStates =
		{
			new Rectangle(Globals.TileSize*11, Globals.TileSize*5, Globals.TileSize, Globals.TileSize),
			new Rectangle(Globals.TileSize*12, Globals.TileSize*5, Globals.TileSize, Globals.TileSize),
			new Rectangle(Globals.TileSize*13, Globals.TileSize*5, Globals.TileSize, Globals.TileSize),
			new Rectangle(Globals.TileSize*14, Globals.TileSize*5, Globals.TileSize, Globals.TileSize),
		};

		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			Vector2 position = new Vector2(Globals.TileSize * x, Globals.TileSize * y);


			GFX.Sprite(GFX.TileSheet, position, TileMap.Torch, color.MultiplyAgainst(Color.White));

#if CLIENT
			GFX.Rect(FlameColor * 0.5f, position + new Vector2(2, -1), new Vector2(4, 4));
			GFX.Rect(FlameColor * 1.2f, position + new Vector2(3, 0), new Vector2(2, 2));
#endif
		}


		public void TileUpdate(IGameWorld world, int x, int y)
		{
			// States:
			// 1 - SupportedBelow
			// 2 - SupportedLeft
			// 3 - SupportedRight
			// 4 - SupportedBehind


			bool supportedBelow = (world.GetTile(x, y + 1) is INonSolid);

			bool supportedLeft = (world.GetTile(x - 1, y) is INonSolid);

			bool supportedRight = (world.GetTile(x + 1, y) is INonSolid);
			bool supportedWall = !(world.GetWall(x, y) is INonSolid);

			if (TileState == 0)
			{
				if (supportedBelow)
					TileState = 1;
				if (supportedLeft)
					TileState = 2;
				if (supportedRight)
					TileState = 3;
				if (supportedWall)
					TileState = 4;
			}

			if (TileState == 0)
			{
				world.SetTile(x, y, new Air()); // TODO: Prevent Placement
			}

			if (TileState == 1 && !supportedBelow)
				world.SetTile(x, y, new Air()); // TODO: Drop Tile;

			if (TileState == 2 && !supportedLeft)
				world.SetTile(x, y, new Air()); // TODO: Drop Tile;

			if (TileState == 3 && !supportedRight)
				world.SetTile(x, y, new Air()); // TODO: Drop Tile;

			if (TileState == 4 && !supportedWall)
				world.SetTile(x, y, new Air()); // TODO: Drop Tile;
		}

	}
	public class Torch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(255, 230, 200);
		public override Color FlameColor => new Color(1, 1, 0.8f);
	}
	public class WhiteTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(255, 255, 255);
		public override Color FlameColor => new Color(1, 1, 1f);
	}
	public class GreenTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(0, 255, 0);
		public override Color FlameColor => new Color(0.1f, 1, 0.1f);
	}
	public class RedTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(255, 0, 0);
		public override Color FlameColor => new Color(1, 0.1f, 0.1f);
	}
	public class BlueTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(0, 0, 255);
		public override Color FlameColor => new Color(0.2f, 0.2f, 1.0f);
	}
}
