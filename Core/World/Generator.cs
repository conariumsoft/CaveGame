using CaveGame.Core.Noise;
using CaveGame.Core.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CaveGame.Core
{
	public enum SurfaceBiome
	{
		Plains,
		Mountains,
		Desert,
		Forest,
	}

	public enum RegionBiome
	{

	}

	public enum SubsurfaceBiome
	{

	}

	public abstract class Biome
	{
		public Biome(int seed)
		{

		}

		public abstract float GetHeight(int x);
	}

	public class Generator
	{
		int[] biomemap;

		public int Seed { get; set; }

		Random terrainRNG;

		public Generator()
		{
			terrainRNG = new Random(Seed);

			biomemap = new int[256];
			int previous = 0;
			for (int i = 0; i < 256; i++)
			{
				
				if (terrainRNG.NextDouble() > 0.5)
				{
					biomemap[i] = terrainRNG.Next(4);
					previous = biomemap[i];
				}else
				{
					biomemap[i] = previous;
				}
					
			}

		}

		private SurfaceBiome GetSurfaceBiome(int x)
		{
			int dj = (int)Math.Floor(x / 16.0f);

			//Debug.Write(dj);
			return (SurfaceBiome)biomemap[dj.Mod(256)];
		}

		private float GetDesertHeight(int x)
		{
			return 0;
		}
		private float GetForestHeight(int x)
		{
			return 0;
		}

		private float GetMountainHeight(int x)
		{
			return 0;
		}

		public void HeightPass(ref Chunk chunk)
		{

			var simplex = new SimplexNoise();
			var octave = new OctaveNoise(5, 4);

			for (int x = 0; x < Chunk.ChunkSize; x++)
			{
				for (int y = 0; y < Chunk.ChunkSize; y++)
				{

					int curX = ((chunk.Coordinates.X * Globals.ChunkSize) + x);
					int curY = ((chunk.Coordinates.Y * Globals.ChunkSize) + y);


					var surface = octave.Noise2D(curX / 20.0, curY / 20.0) * 10 + (octave.Noise2D(curX / 201.0, curY / 199.0) * 40);

					var depth = (curY - surface - 15);
					SurfaceBiome biome = GetSurfaceBiome(x);

				/*	if (biome == SurfaceBiome.Mountains)
					{
						if (depth < 0)
						{
							chunk.SetTile(x, y, new Tiles.Air());
						}
						else
						{
							chunk.SetTile(x, y, new Tiles.Stone());
						}
					}
					if (biome == SurfaceBiome.Forest)
					{
						if (depth < 0)
						{
							chunk.SetTile(x, y, new Tiles.Air());
						}
						else
						{
							chunk.SetTile(x, y, new Tiles.Mud());
						}
					}

					if (biome == SurfaceBiome.Desert)
					{
						if (depth < 0)
						{
							chunk.SetTile(x, y, new Tiles.Air());
						}
						else if (depth <= 1.5f)
						{
							chunk.SetTile(x, y, new Tiles.Sand());
						}
						else if (depth <= 5)
						{
							chunk.SetTile(x, y, new Tiles.Sand());
							chunk.SetWall(x, y, new Walls.Sandstone());
						}
						else
						{
							chunk.SetWall(x, y, new Walls.Sandstone());
							var noise = simplex.Noise(curX / 4.0f, curY / 4.0f) * 30;
							if (noise + depth > 30.5)
								chunk.SetTile(x, y, new Tiles.Sandstone());
							else
								chunk.SetTile(x, y, new Tiles.Sand());
						}
					}*/
					
					//if (biome == SurfaceBiome.Plains)
					//{
						if (depth < 0)
						{
							chunk.SetTile(x, y, new Tiles.Air());
						}
						else if (depth <= 1.5f)
						{
							chunk.SetTile(x, y, new Tiles.Grass());
						}
						else if (depth <= 5)
						{
							chunk.SetTile(x, y, new Tiles.Dirt());
							chunk.SetWall(x, y, new Walls.Dirt());
						}
						else
						{
							chunk.SetWall(x, y, new Walls.Stone());
							var noise = simplex.Noise(curX / 4.0f, curY / 4.0f) * 30;
							if (noise + depth > 30.5)
								chunk.SetTile(x, y, new Tiles.Stone());
							else
								chunk.SetTile(x, y, new Tiles.Dirt());
						}
					//}
					
					if (depth > 100)
					{
						float noise = simplex.Noise((curX - 999) / 400.0f, (curY + 420) / 400.0f);
						float n2 = (float)octave.Noise2D((curX - 999) / 4.0f, (curY + 420) / 4.0f);
						if (noise > 0.75f)
						{
							if (n2 < 0.10f)
								chunk.SetTile(x, y, new Mycelium());
							else
								chunk.SetTile(x, y, new Air());
						}
					}

					if (depth > 0)
					{
						if (simplex.Noise(curX / 50.0f, (curY / 60.0f) + 4848) > 0.65f)
							chunk.SetTile(x, y, new Clay());

						if (simplex.Noise((curX + 444) / 45.0f, (curY / 22.0f) + 458) > 0.75f)
							chunk.SetTile(x, y, new Granite());
					}

					// caves
					if (depth >= 0)
					{



						// jagged caves
						var cavetiny = octave.Noise2D(curX / 5.0f, curY / 5.0f) * 0.5f;
						var cave1 = (octave.Noise2D(curX / 30.0f, curY / 30.0f));
						var cave2 = (octave.Noise2D((curX + 11) / 200.0f, (curY + 50) / 200.0f) * 0.6f) + (cave1 * 1.5f) - 0.3f + (cavetiny);
						if (cave1 > 0.75f)
						{
							chunk.SetTile(x, y, new Tiles.Air());
						}
						if (cavetiny > 0.8f)
						{
							chunk.SetTile(x, y, new Tiles.Air());
						}
						if (cave2 > -0.08f && cave2 < 0.08f)
						{
							chunk.SetTile(x, y, new Tiles.Air());
						}

						if (depth > 50 && chunk.GetTile(x, y) is Tiles.Air && simplex.Noise(curX / 5.0f, curY / 8.0f) > 0.98f)
						{

							chunk.SetTile(x, y, new Cobweb());
						}


						if (cavetiny > 0.2f)
						{

							chunk.SetTile(x, y, new Water { TileState = 8 });
							//TileUpdate[x, y] = true;
						}

						if (depth > 150)
						{
							// lava tubes
							if (simplex.Noise( (curX + 444) / 100.0f, curY / 100.0f) > 0.8f)
							{
								chunk.SetTile(x, y, new Lava { TileState = 8 });
							}
							if (simplex.Noise(curX / 400.0f, curY / 400.0f) > 0.7f)
							{
								if (chunk.GetTile(x, y) is Air)
									chunk.SetTile(x, y, new Lava { TileState = 8 });
								//TileUpdate[x, y] = true;
							}
						}
					}
				}
			}
		}
		
		public void StructurePass(IGameWorld world, int x, int y)
		{
			if (terrainRNG.Next(64) == 2 && world.GetTile(x, y) is Tiles.Grass && world.GetTile(x, y - 1) is Tiles.Air)
			{


				for (int dx = -3; dx <= 3; dx++)
				{
					for (int dy = -3; dy <= 3; dy++)
					{
						world.SetTile(dx + x, dy + y - 7, new Tiles.Leaves());
					}
				}
				world.SetTile(x, y - 1, new Tiles.OakLog());
				world.SetTile(x, y - 2, new Tiles.OakLog());
				world.SetTile(x, y - 3, new Tiles.OakLog());
				world.SetTile(x, y - 4, new Tiles.OakLog());
				world.SetTile(x, y - 5, new Tiles.OakLog());

				//world.SetTile(x, y - 1, new Stone());
			}
		}
	}
}
