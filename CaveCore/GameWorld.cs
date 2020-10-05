using CaveGame.Core.Entities;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CaveGame.Core
{
	public struct Light3: IEquatable<Light3>
	{
		public static Light3 Dark = new Light3(0, 0, 0);
		public static Light3 Ambience = new Light3(16, 16, 16);

		public byte Red;
		public byte Blue;
		public byte Green;

		public Light3(byte r, byte g, byte b)
		{
			Red = r;
			Green = g;
			Blue = b;
		}

		public bool Equals(Light3 other)
		{
			return (other.Red == Red && other.Blue == Blue && other.Green == Green);
		} 

		public Color MultiplyAgainst(Color col)
		{
			byte cRed = Math.Min(Red, (byte)15);
			byte cGreen = Math.Min(Green, (byte)15);
			byte cBlue = Math.Min(Blue, (byte)15);
			return new Color(
				(col.R/255.0f) * (cRed / 15.0f),
				(col.G / 255.0f) * (cGreen / 15.0f),
				(col.B / 255.0f) * (cBlue / 15.0f),
				col.A
			);
		}

		public override string ToString()
		{
			return string.Format("[{0} {1} {2}]", Red, Green, Blue);
		}

	}

	public interface IGameWorld
	{
		public List<IEntity> Entities { get; }
		public Tile GetTile(int x, int y);
		public void SetTile(int x, int y, Tile t);
		public void SetTileNetworkUpdated(int x, int y);
		public void DoUpdatePropogation(int x, int y);
		public void SetTileUpdated(int x, int y);

		public void Update(GameTime gt);
	}

	public abstract class World : IGameWorld
	{
		public ConcurrentDictionary<ChunkCoordinates, Chunk> Chunks;
		public List<IEntity> Entities { get; protected set; }

		public void SetTileNetworkUpdated(int x, int y)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = mod(x, Globals.ChunkSize);
			var tileY = mod(y, Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				chunk.NetworkUpdated[tileX, tileY] = true;
			}
		}

		public virtual void SetTile(int x, int y, Tile t)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = mod(x, Globals.ChunkSize);
			var tileY = mod(y, Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				chunk.SetTile(tileX, tileY, t);
			}
			DoUpdatePropogation(x, y);
		}

		public void SetTileUpdated(int x, int y)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = mod(x, Globals.ChunkSize);
			var tileY = mod(y, Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				chunk.SetTileUpdated(tileX, tileY);
			}
		}

		public void DoUpdatePropogation(int x, int y)
		{
			SetTileUpdated(x, y);
			SetTileUpdated(x, y + 1);
			SetTileUpdated(x, y - 1);
			SetTileUpdated(x + 1, y);
			SetTileUpdated(x - 1, y);
		}


		protected int mod(int x, int m)
		{
			return (x % m + m) % m;
		}

		public Tile GetTile(int x, int y)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = mod(x, Globals.ChunkSize);
			var tileY = mod(y, Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				//Debug.WriteLine(String.Format("{0} {1} {2} {3} {4} {5}", x, y, chunkX, chunkY, tileX, tileY));
				return chunk.GetTile(tileX, tileY);
			}
			return new Air();
		}

		public virtual void Update(GameTime gt) { }

		public World()
		{
			Entities = new List<IEntity>();
			Chunks = new ConcurrentDictionary<ChunkCoordinates, Chunk>();
		}
	}

	
}
