﻿using CaveGame.Core.Noise;
using CaveGame.Core.Tiles;
using CaveGame.Core.Walls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CaveGame.Core
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
		public bool[,] NetworkUpdated;
		public bool[,] TileUpdate;
		public Wall[,] Walls;
		public ChunkCoordinates Coordinates;
		public bool UpdateRenderBuffer = true;

		public RenderTarget2D ForegroundRenderBuffer;
		public RenderTarget2D BackgroundRenderBuffer;

		public Chunk(int X, int Y)
		{
			Coordinates = new ChunkCoordinates(X, Y);
			NetworkUpdated = new bool[ChunkSize, ChunkSize];
			Tiles = new Tile[ChunkSize, ChunkSize];
			TileUpdate = new bool[ChunkSize, ChunkSize];
			Walls = new Wall[ChunkSize, ChunkSize];
			Lights = new Light3[ChunkSize, ChunkSize];
	

			for (int x = 0; x < ChunkSize; x++)
			{
				for (int y = 0; y < ChunkSize; y++)
				{
					SetTile(x, y, new Tiles.Air());
					SetWall(x, y, new Walls.Air());
				}
			}
			
			
		}

		public void ClearUpdateQueue()
		{
			NetworkUpdated = new bool[ChunkSize, ChunkSize];
			TileUpdate = new bool[ChunkSize, ChunkSize];
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
					Tiles[x, y].Serialize(ref data, dIndex);
					dIndex += 4;
				}
			}
			// load walls
			for (int x = 0; x < Chunk.ChunkSize; x++)
			{
				for (int y = 0; y < Globals.ChunkSize; y++)
				{
					Walls[x, y].Serialize(ref data, dIndex);
					dIndex += 4;
				}
			}
			return data;
		}


		public void SetTileUpdated(int x, int y)
		{
			TileUpdate[x, y] = true;
			UpdateRenderBuffer = true;
		}

		public void SetTile(int x, int y, Tile t)
		{
			//Debug.WriteLine("TT " + t.TileState);
			if (Tiles[x,y] == null || Tiles[x, y].Equals(t) == false)
			{
				Tiles[x, y] = t;
				NetworkUpdated[x, y] = true; // TODO: Create WallReplicate and TileReplicate queues
				UpdateRenderBuffer = true;
			}
		}

		public Tile GetTile(int x, int y)
		{
			return Tiles[x, y];
		}

		public Wall GetWall(int x, int y) {
			return Walls[x, y];
		}

		public void SetWall(int x, int y, Wall w) {
		//	if (Walls[x, y] == null)
		//	{
				Walls[x, y] = w;
				NetworkUpdated[x, y] = true; // TODO: Create WallReplicate and TileReplicate queues
				UpdateRenderBuffer = true; 
		//	}
		}

		private void DrawForegroundBuffer(Texture2D tilesheet, GraphicsDevice device, SpriteBatch sb)
		{
			if (ForegroundRenderBuffer == null)
				ForegroundRenderBuffer = new RenderTarget2D(device, ChunkSize * Globals.TileSize, ChunkSize * Globals.TileSize);

			device.SetRenderTarget(ForegroundRenderBuffer);
			device.Clear(Color.Black * 0f);

			sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			Tile tile;

			for (int x = 0; x < ChunkSize; x++)
			{
				for(int y = 0; y<ChunkSize; y++)
				{
					tile = GetTile(x, y);
					if (tile.ID > 0)
					{
						tile.Draw(tilesheet, sb, x, y, Lights[x, y]);
					}
				}
			}
			sb.End();
			device.SetRenderTarget(null);
		}

		private void DrawBackgroundBuffer(Texture2D tilesheet, GraphicsDevice device, SpriteBatch sb)
		{
			if (BackgroundRenderBuffer == null)
				BackgroundRenderBuffer = new RenderTarget2D(device, ChunkSize * Globals.TileSize, ChunkSize * Globals.TileSize);

			device.SetRenderTarget(BackgroundRenderBuffer);
			device.Clear(Color.Black * 0f);

			sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			Wall wall;

			for (int x = 0; x < ChunkSize; x++)
			{
				for (int y = 0; y < ChunkSize; y++)
				{
					wall = GetWall(x, y);
					if (wall.ID > 0)
					{
						wall.Draw(tilesheet, sb, x, y, Lights[x, y]);
					}
				}
			}
			sb.End();
			device.SetRenderTarget(null);
		}

		public void Draw(Texture2D tilesheet, GraphicsDevice device, SpriteBatch sb)
		{

			Chunk.RefreshedThisFrame = true;
				// cock and ball torture
			UpdateRenderBuffer = false;
			DrawBackgroundBuffer(tilesheet, device, sb);
			DrawForegroundBuffer(tilesheet, device, sb);
		}
	}
}