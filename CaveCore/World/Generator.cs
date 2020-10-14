using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
	public enum SurfaceBiomes
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

	public enum RegionBiomes
	{

	}

	public enum SubsurfaceBiomes
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

			for (int i = 0; i < 128; i++)
			{
				int previous = 0;
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

		public void HeightPass(ref Chunk chunk)
		{

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
