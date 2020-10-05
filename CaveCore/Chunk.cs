using CaveGame.Core.Noise;
using CaveGame.Core.Tiles;
using CaveGame.Core.Walls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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
					SetTile(x, y, new Air());
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
						SetTile(x, y, new Air());
					}
					else if (depth - 1 <= 0)
					{
						SetTile(x, y, new Grass());
					}
					else if (depth <= 5)
					{
						SetTile(x, y, new Dirt());
					}
					else {
						var noise = simplex.Noise(curX / 4.0f, curY / 4.0f)*30;
						if (noise+depth > 30.5)
						{
							SetTile(x, y, new Stone());
						}
						else
						{
							
							SetTile(x, y, new Dirt());
						}
					}

					// caves
					if (depth >= 0)
					{
						// jagged caves
						var cavetiny = octave.Noise2D(curX / 5.0f, curY / 5.0f) * 0.4f;
						var cave1 = (octave.Noise2D(curX / 30.0f, curY / 30.0f));
						var cave2 = (octave.Noise2D((curX+11) / 200.0f, (curY+50) / 200.0f) * 0.6f) + (cave1*1.8f) - 0.4f + (cavetiny);
						if (cave1 > 0.8f)
						{
							SetTile(x, y, new Air());
						}
						if (cavetiny > 0.7f)
						{
							SetTile(x, y, new Air());
						}
						if (cave2 > -0.15f && cave2 < 0.12f)
						{
							SetTile(x, y, new Air());
						}

						if (cavetiny > 0.15f)
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
			if (Tiles[x,y] == null || Tiles[x, y].ID != t.ID)
			{
				Tiles[x, y] = t;
				NetworkUpdated[x, y] = true;
				UpdateRenderBuffer = true;
			}
		}

		public Tile GetTile(int x, int y)
		{
			return Tiles[x, y];
		}

		public void Draw(Texture2D tilesheet, GraphicsDevice device, SpriteBatch sb)
		{
			if (Chunk.RefreshedThisFrame == true)
				return;
			if (RenderBuffer == null || UpdateRenderBuffer == true)
			{
				Chunk.RefreshedThisFrame = true;
				// cock and ball torture
				UpdateRenderBuffer = false;
				RenderBuffer = new RenderTarget2D(device, ChunkSize * Globals.TileSize, ChunkSize * Globals.TileSize);

				device.SetRenderTarget(RenderBuffer);

				device.Clear(Color.Black * 0f);

				sb.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);

				for (int x = 0; x < ChunkSize; x++)
					for (int y = 0; y < ChunkSize; y++)
						GetTile(x, y).Draw(tilesheet, sb, x, y, Lights[x,y]);

				sb.End();
				device.SetRenderTarget(null);
			}
		}
	}
}
