using CaveGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CaveGame.Core.Game.Tiles
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
		public static Rectangle Glass = Quad(7, 1);
		public static Rectangle GlassBG = Quad(8, 1);

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
		public static Rectangle Sapling = Quad(4, 2);
		public static Rectangle Flowers = Quad(5, 2);
		public static Rectangle BlueMushroom = Quad(6, 2);
		public static Rectangle RedMushroom = Quad(7, 2);
		public static Rectangle Poppy = Quad(8, 2);
		public static Rectangle CryingLilyTop = Quad(9, 2);
		public static Rectangle CryingLilyBottom = Quad(9, 3);
		public static Rectangle CryingLily = Quad(9, 2, 1, 2);
		public static Rectangle TallGrass = Quad(8, 5);
		public static Rectangle TallGrass2 = Quad(9, 5);
		public static Rectangle TallGrass3 = Quad(10, 5);

		public static Rectangle SmallMushroom = Quad(8, 3);
		public static Rectangle Reeds = Quad(9, 4);
		public static Rectangle ReedsTop = Quad(8, 4);

		public static Rectangle BMPY_WoodGrain = Quad(0, 4);
		public static Rectangle BMPY_Stone = Quad(3, 3);
		public static Rectangle BMPY_DarkStone = Quad(1, 4);
		public static Rectangle RedBrick = Quad(0, 3);

		public static Rectangle BGBrickTL = Quad(11, 1);
		public static Rectangle BGBrickTR = Quad(12, 1);
		public static Rectangle BGBrickML = Quad(11, 2);
		public static Rectangle BGBrickMR = Quad(12, 2);
		public static Rectangle BGBrickBL = Quad(11, 3);
		public static Rectangle BGBrickBR = Quad(12, 3);

		public static Rectangle BGStoneTL = Quad(13, 1);
		public static Rectangle BGStoneTR = Quad(14, 1);
		public static Rectangle BGStoneBL = Quad(13, 2);
		public static Rectangle BGStoneBR = Quad(14, 2);

		public static Rectangle BGMossyBrickTL = Quad(11, 4);
		public static Rectangle BGMossyBrickTR = Quad(12, 4);
		public static Rectangle BGMossyBrickML = Quad(11, 5);
		public static Rectangle BGMossyBrickMR = Quad(12, 5);
		public static Rectangle BGMossyBrickBL = Quad(11, 6);
		public static Rectangle BGMossyBrickBR = Quad(12, 6);

		public static Rectangle Pot = Quad(7, 3);
		public static Rectangle Obsidian = Quad(10, 1);

		public static Rectangle Cactus = Quad(0, 8);
		public static Rectangle CactusFlowering = Quad(1, 8);
		public static Rectangle CactusTop = Quad(2, 8);

		public static Rectangle LeadPipeCorner = Quad(3, 8);
		public static Rectangle LeadPipe = Quad(4, 8);
		public static Rectangle CopperPipeCorner = Quad(5, 8);
		public static Rectangle CopperPipe = Quad(6, 8);

		// Flowers contributed by Human
		public static Rectangle BlackEyedSusan = Quad(4, 4);
		public static Rectangle Chrysanthemum = Quad(5, 4);
		public static Rectangle ForgetMeNot = Quad(6, 4);
		public static Rectangle Honeysuckle = Quad(4, 3);
		public static Rectangle Hydrangea = Quad(5, 3);
		public static Rectangle Lily = Quad(4, 5);
		public static Rectangle Magnolia = Quad(5, 5);
		public static Rectangle Orchid = Quad(6, 5);
		public static Rectangle Plumeria = Quad(7, 5);
		public static Rectangle Tulip = Quad(3, 1);



		public static Dictionary<string, Rectangle> Atlas = new Dictionary<string, Rectangle> {
			
		};
	}
}
