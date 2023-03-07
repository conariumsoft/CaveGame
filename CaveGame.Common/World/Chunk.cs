using CaveGame.Common.Noise;
using CaveGame.Common.Game.Tiles;
using CaveGame.Common.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CaveGame.Common.Game.Walls;
using System.Collections.Concurrent;

namespace CaveGame.Common
{
	public class Chunk
	{
		public static bool RefreshedThisFrame = false;

		public bool TerrainPassCompleted;
		public bool DecorationPassCompleted;
		public bool DungeonPassCompleted;

		public static int ChunkSize = Globals.ChunkSize;

		public Light3[,] Lights;
		public Tile[,] Tiles;
		public Wall[,] Walls;
		public ChunkCoordinates Coordinates;
		public bool WallBufferNeedsRedrawn = true;
		public bool TileBufferNeedsRedrawn = false;

		public RenderTarget2D TileRenderBuffer { get; set; }
		public RenderTarget2D WallRenderBuffer { get; set; }


		public Chunk(int X, int Y)
		{
			Coordinates = new ChunkCoordinates(X, Y);

			Tiles = new Tile[ChunkSize, ChunkSize];
			Walls = new Wall[ChunkSize, ChunkSize];
			Lights = new Light3[ChunkSize, ChunkSize];
	

			for (int x = 0; x < ChunkSize; x++)
			{
				for (int y = 0; y < ChunkSize; y++)
				{
					SetTile(x, y, new Game.Tiles.Air());
					SetWall(x, y, new Game.Walls.Air());
				}
			}
		}
		bool disposed;
		~Chunk() => Dispose(false);

		public void Dispose() => Dispose(true);

		protected void Dispose(bool disposing)
        {
			if (disposed)
				return;
			if (disposing)
            {
				TileRenderBuffer?.Dispose();
				WallRenderBuffer?.Dispose();
			}
        }

		public void FromData(byte[] data)
		{
			int dIndex = 0;

			byte header = data[0];

			TerrainPassCompleted = header.Get(0);
			DungeonPassCompleted = header.Get(1);
			DecorationPassCompleted = header.Get(2);

			dIndex++;
			// load tiles
			for (int x = 0; x<Chunk.ChunkSize; x++)
			{
				for(int y = 0; y<Globals.ChunkSize; y++)
				{
					Tiles[x, y] = Tile.Deserialize(ref data, dIndex);
					dIndex += 4;
				}
			}
			// load walls
			for (int x = 0; x < Chunk.ChunkSize; x++)
			{
				for (int y = 0; y < Globals.ChunkSize; y++)
				{
					Walls[x, y] = Wall.Deserialize(ref data, dIndex);
					dIndex += 4;
				}
			}
		}

		public byte[] ToData()
		{
			byte[] data = new byte[4096*2 + 1];
			int dIndex = 0;

			byte header = 0;
			header.Set(0, TerrainPassCompleted);
			header.Set(1, DungeonPassCompleted);
			header.Set(2, DecorationPassCompleted);
			data[0] = header;
			dIndex++;
			// load tiles
			for (int x = 0; x < Chunk.ChunkSize; x++)
			{
				for (int y = 0; y < Globals.ChunkSize; y++)
				{
					Tiles[x, y].Encode(ref data, dIndex);
					dIndex += 4;
				}
			}
			// load walls
			for (int x = 0; x < Chunk.ChunkSize; x++)
			{
				for (int y = 0; y < Globals.ChunkSize; y++)
				{
					Walls[x, y].Encode(ref data, dIndex);
					dIndex += 4;
				}
			}
			return data;
		}

		public Tile GetTile(int x, int y)=> Tiles[x, y];
		public void SetTile(int x, int y, Tile t)
		{
			Tiles[x, y] = t;
			TileBufferNeedsRedrawn = true;
		}
		public Wall GetWall(int x, int y) => Walls[x, y];
		public void SetWall(int x, int y, Wall w)
		{
			Walls[x, y] = w;
			WallBufferNeedsRedrawn = true;
		}
		public void RedrawTileBuffer(GraphicsEngine GFX)
        {
			if (TileRenderBuffer == null)
				TileRenderBuffer = new RenderTarget2D(GFX.GraphicsDevice, ChunkSize * Globals.TileSize, ChunkSize * Globals.TileSize);


			GFX.GraphicsDevice.SetRenderTarget(TileRenderBuffer);
			GFX.Clear(Color.Black * 0f);

			GFX.Begin(SpriteSortMode.Immediate);
			Tile tile;

			for (int x = 0; x < ChunkSize; x++)
			{
				for (int y = 0; y < ChunkSize; y++)
				{
					tile = GetTile(x, y);
					if (tile.ID > 0)
						tile.Draw(GFX, x, y, Lights[x, y]);
				}
			}
			GFX.End();
			GFX.GraphicsDevice.SetRenderTarget(null);
		}

		public void RedrawWallBuffer(GraphicsEngine GFX)
        {
			if (WallRenderBuffer == null)
				WallRenderBuffer = new RenderTarget2D(GFX.GraphicsDevice, ChunkSize * Globals.TileSize, ChunkSize * Globals.TileSize);

			GFX.GraphicsDevice.SetRenderTarget(WallRenderBuffer);
			GFX.Clear(Color.Black * 0f);

			GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			Wall wall;

			for (int x = 0; x < ChunkSize; x++)
			{
				for (int y = 0; y < ChunkSize; y++)
				{
					wall = GetWall(x, y);
					if (wall.ID > 0)
						wall.Draw(GFX, x, y, Lights[x, y]);
				}
			}
			GFX.End();
			GFX.GraphicsDevice.SetRenderTarget(null);
		}


	}
}
