﻿using CaveGame.Core.Entities;
using CaveGame.Core.Tiles;
using CaveGame.Core.Walls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace CaveGame.Core
{
	

	public interface IGameWorld
	{
		public List<IEntity> Entities { get; }
		public Tile GetTile(int x, int y);
		public void SetTile(int x, int y, Tile t);

		public void GetTile(int x, int y, out Tile t);


		public Wall GetWall(int x, int y);
		public void SetWall(int x, int y, Wall w);
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

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

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

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

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

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

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

		public Tile GetTile(int x, int y)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				//Debug.WriteLine(String.Format("{0} {1} {2} {3} {4} {5}", x, y, chunkX, chunkY, tileX, tileY));
				return chunk.GetTile(tileX, tileY);
			}
			return new Tiles.Void();
		}

		public void GetTile(int x, int y, out Tile t)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				//Debug.WriteLine(String.Format("{0} {1} {2} {3} {4} {5}", x, y, chunkX, chunkY, tileX, tileY));
				t = chunk.GetTile(tileX, tileY);
			}
			t = new Tiles.Void();
		}

		public Wall GetWall(int x, int y)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				//Debug.WriteLine(String.Format("{0} {1} {2} {3} {4} {5}", x, y, chunkX, chunkY, tileX, tileY));
				return chunk.GetWall(tileX, tileY);
			}
			return new Walls.Void();
		}

		public virtual void SetWall(int x, int y, Wall w)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				chunk.SetWall(tileX, tileY, w);
			}
		}

		public virtual void Update(GameTime gt) { }

		public World()
		{
			Entities = new List<IEntity>();
			Chunks = new ConcurrentDictionary<ChunkCoordinates, Chunk>();
		}
	}

	
}