using CaveGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Tiles
{
	public static class TileMap
	{

		private static Rectangle Quad(int qx, int qy, int qw = 1, int qh = 1)
		{
			return new Rectangle(qx * Globals.TileSize, qy * Globals.TileSize, qw * Globals.TileSize, qh * Globals.TileSize);
		}

		public static Rectangle Default = Quad(0, 0);
		public static Rectangle Stone = Quad(1, 0);
		public static Rectangle Plank = Quad(2, 0);
		public static Rectangle Soil = Quad(3, 0);
		public static Rectangle MossyBrick = Quad(4, 0);
		public static Rectangle Paneling = Quad(5, 0);
		public static Rectangle OpaqueLeaves = Quad(6, 0);
		public static Rectangle Leaves = Quad(7, 0);
		public static Rectangle PlankRounded = Quad(8, 0);
		public static Rectangle StoneSpot = Quad(9, 0);
		public static Rectangle StoneMossy = Quad(10, 0);
		public static Rectangle StoneLight = Quad(11, 0);
		public static Rectangle StoneCubes = Quad(12, 0);
		public static Rectangle Carved = Quad(13, 0);
		public static Rectangle SideBrick = Quad(14, 0);
		public static Rectangle BrickStep = Quad(15, 0);
		public static Rectangle Log = Quad(0, 2);
		public static Rectangle Vine = Quad(2, 2);

		public static Rectangle Brick = Quad(0, 1);
		public static Rectangle Ore = Quad(1, 1);
		public static Rectangle TNT = Quad(2, 1);

		public static Rectangle Cobweb = Quad(6, 3);

		public static Rectangle StoneFading = Quad(7, 6);
		public static Rectangle DirtFading = Quad(7, 7);

		public static Rectangle Torch = Quad(11, 7);
		public static Rectangle SideTorch = Quad(12, 7);
		public static Rectangle WallTorch = Quad(13, 7);
		public static Rectangle Platform = Quad(1, 2);
		public static Rectangle Ladder = Quad(1, 3);
		public static Rectangle Rope = Quad(3, 2);
		public static Rectangle TallGrass = Quad(8, 5);

		public static Rectangle BMPY_LongBrick = Quad(0, 3);
		public static Rectangle BMPY_WoodGrain = Quad(0, 4);
		public static Rectangle BMPY_Stone = Quad(3, 3);
		public static Rectangle BMPY_DarkStone = Quad(1, 4);


		public static Dictionary<string, Rectangle> Atlas = new Dictionary<string, Rectangle> {
			
		};
	}
}
