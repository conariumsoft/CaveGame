using CaveGame.Core.Noise;
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

		public RenderTarget2D RenderBuffer;

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
			FillNoise();
			NetworkUpdated = new bool[ChunkSize, ChunkSize];
			NetworkUpdated.Initialize();
		//	TileUpdate.Initialize();

		}


		public void SetTileUpdated(int x, int y)
		{
			TileUpdate[x, y] = true;
			UpdateRenderBuffer = true;
		}


		public void FillNoise()
		{
			var simplex = new SimplexNoise();
			var octave = new OctaveNoise(5, 4);

			for (int x = 0; x < Chunk.ChunkSize; x++)
			{
				for (int y = 0; y < Chunk.ChunkSize; y++)
				{
					int curX = ((Coordinates.X * Globals.ChunkSize) + x);
					int curY = ((Coordinates.Y * Globals.ChunkSize) + y);


					var surface = octave.Noise2D(curX / 20.0, curY / 20.0)*10 + (octave.Noise2D(curX / 201.0, curY / 199.0) * 40);

					var depth = (curY - surface - 15);

					if (depth < 0)
					{
						SetTile(x, y, new Tiles.Air());
					}
					else if (depth <= 1.5f)
					{
						SetTile(x, y, new Tiles.Grass());
					}
					else if (depth <= 5)
					{
						SetTile(x, y, new Tiles.Dirt());
						SetWall(x, y, new Walls.Dirt());
					}
					else {
						SetWall(x, y, new Walls.Dirt());
						var noise = simplex.Noise(curX / 4.0f, curY / 4.0f)*30;
						if (noise+depth > 30.5)
						{
							SetTile(x, y, new Tiles.Stone());
							
						}
						else
						{
							
							SetTile(x, y, new Tiles.Dirt());
						}
					}

					// caves
					if (depth >= 0)
					{
						// jagged caves
						var cavetiny = octave.Noise2D(curX / 5.0f, curY / 5.0f) * 0.5f;
						var cave1 = (octave.Noise2D(curX / 30.0f, curY / 30.0f));
						var cave2 = (octave.Noise2D((curX + 11) / 200.0f, (curY + 50) / 200.0f) * 0.6f) + (cave1 * 1.5f) - 0.3f + (cavetiny);
						if (cave1 > 0.8f)
						{
							SetTile(x, y, new Tiles.Air());
						}
						if (cavetiny > 0.8f)
						{
							SetTile(x, y, new Tiles.Air());
						}
						if (cave2 > -0.08f && cave2 < 0.08f)
						{
							SetTile(x, y, new Tiles.Air());
						}

						if (cavetiny > 0.18f)
						{
							SetTile(x, y, new Water { TileState = 8 });
							//TileUpdate[x, y] = true;
						}
					}
				}
			}
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
			if (Walls[x, y] == null || Walls[x, y].Equals(w) == false)
			{
				Walls[x, y] = w;
				NetworkUpdated[x, y] = true; // TODO: Create WallReplicate and TileReplicate queues
				UpdateRenderBuffer = true; 
			}
		}

		public void Draw(Texture2D tilesheet, GraphicsDevice device, SpriteBatch sb)
		{

			Chunk.RefreshedThisFrame = true;
				// cock and ball torture
			UpdateRenderBuffer = false;
			if (RenderBuffer == null)
				RenderBuffer = new RenderTarget2D(device, ChunkSize * Globals.TileSize, ChunkSize * Globals.TileSize);

			device.SetRenderTarget(RenderBuffer);

			device.Clear(Color.Black * 0f);

			sb.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
			Tile tile;
			Wall wall;
			for (int x = 0; x < ChunkSize; x++)
			{
				for (int y = 0; y < ChunkSize; y++)
				{
					tile = GetTile(x, y);
					wall = GetWall(x, y);
					if (wall.ID > 0)
					{
						wall.Draw(tilesheet, sb, x, y, Lights[x, y]);
					}
					if (tile.ID > 0)
					{
						tile.Draw(tilesheet, sb, x, y, Lights[x, y]);

						
					}

					

				}
			}
				
			sb.End();
			device.SetRenderTarget(null);
		}
	}
}
