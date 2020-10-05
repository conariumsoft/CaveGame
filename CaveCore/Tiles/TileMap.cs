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
		public static Rectangle Leaves = Quad(7, 0);
		public static Rectangle Log = Quad(0, 2);
		public static Rectangle Vine = Quad(2, 2);

		public static Rectangle Brick = Quad(0, 1);
		public static Rectangle Ore = Quad(1, 1);
		

		public static Dictionary<string, Rectangle> Atlas = new Dictionary<string, Rectangle> {
			
		};
	}
}
