using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
	public struct Coordinates6D : IEquatable<Coordinates6D>
	{


		public int WorldX { get; set; }
		public int WorldY { get; set; }
		public int ChunkX { get; set; }
		public int ChunkY { get; set; }
		public int TileX { get; set; }
		public int TileY { get; set; }

		public static Coordinates6D FromWorld(int wx, int wy)
		{
			int chunkX = (int)Math.Floor((double)wx / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)wy / Globals.ChunkSize);

			var tileX = wx.Mod(Globals.ChunkSize);
			var tileY = wy.Mod(Globals.ChunkSize);

			return new Coordinates6D
			{
				ChunkX = chunkX,
				ChunkY = chunkY,
				TileX = tileX,
				TileY = tileY,
				WorldX = wx,
				WorldY = wy
			};
		}

		public static Coordinates6D FromQuad(int cx, int cy, int tx, int ty)
		{
			return new Coordinates6D
			{
				ChunkX = cx,
				ChunkY = cy,
				TileX = tx,
				TileY = ty,
				WorldX = (cx * Globals.ChunkSize) + tx,
				WorldY = (cy * Globals.ChunkSize) + ty
			};
		}
		public bool Equals(Coordinates6D other)
		{
			return (other.WorldX == WorldX && other.WorldX == WorldY);
		}

		public override int GetHashCode()
		{
			var hash = 42069;
			hash = hash * -666 + WorldX.GetHashCode();
			hash = hash * 9929 + WorldY.GetHashCode();
			return hash;
		}
		// No LINQ for performance reasons



	}

	public struct ChunkCoordinates: IEquatable<ChunkCoordinates>
	{
		public int X { get; set; }
		public int Y { get; set; }

		public ChunkCoordinates(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static Vector2 ToVector2(ChunkCoordinates coords)
		{
			return new Vector2(coords.X, coords.Y);
		}

		public bool Equals(ChunkCoordinates other)
		{
			return (other.X == X && other.Y == Y);
		}

		public override int GetHashCode()
		{
			var hash = 42069;
			hash = hash * -666 + X.GetHashCode();
			hash = hash * 9929 + Y.GetHashCode();
			return hash;
		}


	}

}
