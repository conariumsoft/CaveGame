using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
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
