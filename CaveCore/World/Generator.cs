using CaveGame.Core.Noise;
using CaveGame.Core.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
	public enum SurfaceBiome
	{
		Forest,
		Plains,
		ColdForest,
		RainForest,
		Swamp,
		Mountains,
		SnowyMountains,
		Desert,
		DesertMountains,
		Redwoods,
		Wasteland,
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
			for (int i = 0; i < 128; i++)
			{
				
				if (terrainRNG.NextDouble() > 0.5)
				{
					biomemap[i] = terrainRNG.Next(8);
					previous = biomemap[i];
				}else
				{
					biomemap[i] = previous;
				}
					
			}

		}

		private SurfaceBiome GetSurfaceBiome(int x)
		{
			int dj = (int)Math.Floor(x / 256.0);


			return (SurfaceBiome)biomemap[dj.Mod(256)];
		}


		public void HeightPass(ref Chunk chunk)
		{

			var simplex = new SimplexNoise();
			var octave = new OctaveNoise(5, 4);

			for (int x = 0; x < Chunk.ChunkSize; x++)
			{
				for (int y = 0; y < Chunk.ChunkSize; y++)
				{

					SurfaceBiome biome = GetSurfaceBiome(x);
					int curX = ((chunk.Coordinates.X * Globals.ChunkSize) + x);
					int curY = ((chunk.Coordinates.Y * Globals.ChunkSize) + y);


					var surface = octave.Noise2D(curX / 20.0, curY / 20.0) * 10 + (octave.Noise2D(curX / 201.0, curY / 199.0) * 40);

					var depth = (curY - surface - 15);

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
						if (cave1 > 0.8f)
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

						if (simplex.Noise(curX / 100.0f, curY / 100.0f) > 0.75f && depth > 100)
						{
							chunk.SetTile(x, y, new Lava { TileState = 8 });
							//TileUpdate[x, y] = true;
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
