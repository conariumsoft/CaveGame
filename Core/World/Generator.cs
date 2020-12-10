using CaveGame.Core.Noise;
using CaveGame.Core.Game;
using System;
using DataManagement;
using CaveGame.Core.WorldGeneration;
using CaveGame.Core.Game.Tiles;
using System.Numerics;

namespace CaveGame.Core
{
	public enum SurfaceBiome : int
	{
		Forest = 0,
		Plains,
		Mountains,
		Desert,
		Badlands,

	}

	public interface IBiomeImplementation
    {

    }

	public abstract class Biome
	{
		public virtual SurfaceBiome BiomeID => SurfaceBiome.Forest;

		public int Seed => Generator.Seed;
		public OctaveNoise Octave4 => Generator.Octave4;
		public OctaveNoise Octave2 => Generator.Octave2;
		public OctaveNoise Octave3 => Generator.Octave3;
		public OctaveNoise Octave8 => Generator.Octave8;
		public SimplexNoise Simplex => Generator.Simplex;


		private Generator Generator;



		public Biome(Generator generator)
		{
			Generator = generator;
		}

		public abstract float GetHeight(int x);
		public abstract void SurfacePass(Chunk chunk, int x, int y);
		public abstract void StructurePass(IGameWorld world, int x, int y);
	}


   /* public class MountainsBiome : Biome
    {
        public override float GetHeight(int x)
        {
            
        }

        public override void SurfacePass(Chunk chunk, int x, int y)
        {
           
        }
    }

	public class ForestBiome : Biome
    {

    }

	public class PlainsBiome : Biome
    {

    }

	public class DesertBiome : Biome
    {

    }*/



    public class Generator
	{
		public const float BIOME_SIZE = 10000.5f;
		public const float BIOME_TRANSITION = 0.2f;


		public int Seed { get; set; }

		public OctaveNoise Octave4;
		public OctaveNoise Octave8;
		public OctaveNoise Octave3;
		public OctaveNoise Octave2;
		public SimplexNoise Simplex;
		public Random RNG;

		private SurfaceBiome[] biomemap;
		
		private void GenerateBiomeNoiseMap()
		{
			int sampleCounts = 100;
			float biomeRepeatChance = 0.35f;
			int biomeCount = 5;

			biomemap = new SurfaceBiome[sampleCounts];
			SurfaceBiome previous = SurfaceBiome.Desert;
			for (int i = 0; i < sampleCounts; i++)
			{
				if (RNG.NextFloat() > biomeRepeatChance)
					biomemap[i] = previous;
				else
					biomemap[i] = (SurfaceBiome)RNG.Next(biomeCount);


				previous = biomemap[i];
			}
		}

		public Generator(int WorldSeed)
		{
			Seed = WorldSeed;
			Octave4 = new OctaveNoise(seed: Seed, octaves: 4);
			Octave8 = new OctaveNoise(seed: Seed, octaves: 8);
			Octave3 = new OctaveNoise(seed: Seed, octaves: 3);
			Octave2 = new OctaveNoise(seed: Seed, octaves: 2);
			Simplex = new SimplexNoise();
			RNG = new Random(Seed: WorldSeed);
			

			GenerateBiomeNoiseMap();

		}
		
		
		private SurfaceBiome GetDominantSurfaceBiome(int x)
		{
			float alpha = (Simplex.Noise(x/BIOME_SIZE)+1)/2.0f; // transform from [-1, 1] to [0, 1]
			int biome = (int)Math.Floor(alpha * 100.0f);

			return biomemap[biome];
		}
		private float GetDesertHeight(int x)
		{
			float n = (float)Simplex.Noise(x / 100.420f) * 10 + (float)Octave4.Noise2D(x / 100.0f, (-x * 0.10f) + (x / 20.0f)) * 20;
			return n;
		}
		private float GetForestHeight(int x)
		{

			float n = (float)Simplex.Noise(x/100.420f)*10 + (float)Octave4.Noise2D(x/60.0f, (-x*0.10f)+(x/20.0f))*30;
			return 20 + n;
		}

		private float GetMountainHeight(int x)
		{
			
			float simplex = (float)Simplex.Noise(x / 50.420f);
			float octave = (float)Octave8.Noise2D(x / 120.0f, (-x * 0.05f) + (x / 40.0f));
			float n =   (simplex * 15) * (octave * 15);
			float a = Simplex.Noise(x / 10.0f) * 2;
			float b = Simplex.Noise(x / 5.0f) * 2;
			return (a+b + 100) + n;
		}
		private float GetPlainsHeight(int x)
        {
			return Simplex.Noise(x / 100.420f)*4;
        }

		private float GetBandlandsHeight(int x)
        {

			float baseline = Simplex.Noise(x / 80f) * 8;
			float raised = (float)(Octave4.Noise2D(x / 6.0f, (-x * 0.03f))+0.5f);

			if (raised > 0.8)
				raised = (raised*5) + 25;
            else if (raised > 0.4)
				raised = (raised*10) + 15;

            return baseline + raised;
        }

		private float GetHeightmap(int x, SurfaceBiome biome)
        {

			if (biome == SurfaceBiome.Desert)
				return GetDesertHeight(x);
			if (biome == SurfaceBiome.Forest)
				return GetForestHeight(x);
			if (biome == SurfaceBiome.Mountains)
				return GetMountainHeight(x);
			if (biome == SurfaceBiome.Plains)
				return GetPlainsHeight(x);
			if (biome == SurfaceBiome.Badlands)
				return GetBandlandsHeight(x);


			return 0;
        }

		private float GetBiomeSurface(int x)
        {

			SurfaceBiome primaryBiome = GetDominantSurfaceBiome(x);
			SurfaceBiome secondaryBiome = GetSecondarySurfaceBiome(x);

			var primaryScale= GetPrimarySurfaceBiomeScale(x);
			var secondScale = GetSecondarySurfaceBiomeScale(x);


			return (GetHeightmap(x, primaryBiome)*primaryScale) + (GetHeightmap(x, secondaryBiome)*secondScale);
		}


		public const float SURFACE_BASE_HEIGHT = -40;

		private float GetBaseSurface(int x, int y)
        {
			float surfacePass1 = Octave4.Noise2D(x: x, y: y, xScale: 10.0, yScale: 10.0, xOffset: -2405, yOffset: 222);
			float surfacePass2 = Octave4.Noise2D(x: x, y: y, xScale: 325.0, yScale: 299.0, xOffset: 2, yOffset: -9924);
			float surface = (surfacePass1*3) + (surfacePass2*10);
			return ( surface - SURFACE_BASE_HEIGHT);
		}



		private SurfaceBiome GetSecondarySurfaceBiome(int x)
        {
			float alpha = (Simplex.Noise(x / BIOME_SIZE) + 1) / 2.0f; // transform from [-1, 1] to [0, 1]
			int biome = (int)Math.Floor(alpha * 100.0f);
			float biomedelta = GetPrimarySurfaceBiomeStrength(x);
			if (biomedelta > (1- BIOME_TRANSITION)) // begin blending with biome on the right side
				return biomemap[(biome + 1).Mod(100)];
			if (biomedelta < BIOME_TRANSITION) // begin blending with biome on the left side
				return biomemap[(biome - 1).Mod(100)];
			return biomemap[biome];

		}

		private float GetPrimarySurfaceBiomeScale(int x)
        {
			float biomedelta = GetPrimarySurfaceBiomeStrength(x);
			if (biomedelta > (1 - BIOME_TRANSITION)) // begin blending with biome on the right side
			{
				float inverse = (1 - BIOME_TRANSITION);
				float lerp = (biomedelta - inverse) / BIOME_TRANSITION;
				return 1-(lerp/2);
			}

			if (biomedelta < BIOME_TRANSITION) // begin blending with biome on the left side
			{
				float lerp = biomedelta  / BIOME_TRANSITION;
				return (lerp/2) + 0.5f;
			}
			return 1;
		}

		private float GetSecondarySurfaceBiomeScale(int x)
        {
			float biomedelta = GetPrimarySurfaceBiomeStrength(x);
			if (biomedelta > (1- BIOME_TRANSITION)) // begin blending with biome on the right side
			{
				float lerp = (biomedelta - ( 1- BIOME_TRANSITION)) / BIOME_TRANSITION;
				return lerp / 2;
			}

			if (biomedelta < BIOME_TRANSITION) // begin blending with biome on the left side
			{
				float lerp = (biomedelta / BIOME_TRANSITION);
				return (1-lerp)/2;
			}
			return 0;
		}

		private float GetPrimarySurfaceBiomeStrength(int x)
        {
			float alpha = (Simplex.Noise(x / BIOME_SIZE) + 1) / 2.0f; // transform from [-1, 1] to [0, 1]
			int biome = (int)Math.Floor(alpha * 100.0f);
			float biomedelta = (alpha * 100) - biome;
			return biomedelta;
		}

		private void MountainsPass(ref Chunk chunk, int chunkX, int chunkY)
        {
			int worldX = ((chunk.Coordinates.X * Globals.ChunkSize) + chunkX);
			int worldY = ((chunk.Coordinates.Y * Globals.ChunkSize) + chunkY);
			float depth = worldY + GetBiomeSurface(worldX) - GetBaseSurface(worldX, worldY);
			if (depth < 0)
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());
			}
			else
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Stone());
			}

			if (depth < 0)
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());
			}
			else if (depth <= 1.5f)
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Grass());
			}
			else if (depth <= 3)
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Dirt());
				chunk.SetWall(chunkX, chunkY, new Game.Walls.Dirt());
			}
			else
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Stone());
				chunk.SetWall(chunkX, chunkY, new Game.Walls.Stone());
			}


			if (depth > 0)
			{
				///
				var noise = Simplex.Noise(worldX / 4.0f, worldY / 4.0f) * 10;
				if (worldY - noise + depth > 10.5)
					chunk.SetTile(chunkX, chunkY, new Game.Tiles.Stone());
			}
		}

		private void ForestPass(ref Chunk chunk, int chunkX, int chunkY)
        {
			int worldX = ((chunk.Coordinates.X * Globals.ChunkSize) + chunkX);
			int worldY = ((chunk.Coordinates.Y * Globals.ChunkSize) + chunkY);
			float depth = worldY + GetBiomeSurface(worldX) - GetBaseSurface(worldX, worldY);
			if (depth < 0)
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());
			}
			else if (depth <= 1.5f)
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Grass());
			}
			else if (depth <= 30)
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Dirt());
				chunk.SetWall(chunkX, chunkY, new Game.Walls.Dirt());
			}
			else
			{
				chunk.SetWall(chunkX, chunkY, new Game.Walls.Stone());
				var noise = Simplex.Noise(worldX / 4.0f, worldY / 4.0f) * 40;
				if (noise + depth > 40.5)
					chunk.SetTile(chunkX, chunkY, new Game.Tiles.Stone());
				else
					chunk.SetTile(chunkX, chunkY, new Game.Tiles.Dirt());
			}
		}

		private void DesertPass(ref Chunk chunk, int chunkX, int chunkY)
        {
			int worldX = ((chunk.Coordinates.X * Globals.ChunkSize) + chunkX);
			int worldY = ((chunk.Coordinates.Y * Globals.ChunkSize) + chunkY);
			float depth = worldY + GetBiomeSurface(worldX) - GetBaseSurface(worldX, worldY);
			if (depth < 0)
			{
				chunk.SetTile(chunkX, chunkY, new Air());
			}
			else if (depth <= 1.5f)
			{
				chunk.SetTile(chunkX, chunkY, new Sand());
			}
			else if (depth <= 5)
			{
				chunk.SetTile(chunkX, chunkY, new Sand());
				chunk.SetWall(chunkX, chunkY, new Game.Walls.Sandstone());
			}
			else
			{
				chunk.SetWall(chunkX, chunkY, new Game.Walls.Sandstone());
				var noise = Simplex.Noise(chunkX / 4.0f, chunkY / 4.0f) * 30;
				if (noise + depth > 30.5)
					chunk.SetTile(chunkX, chunkY, new Sandstone());
				else
					chunk.SetTile(chunkX, chunkY, new Sand());
			}
		}

		private void PlainsPass(ref Chunk chunk, int chunkX, int chunkY)
        {
			int worldX = ((chunk.Coordinates.X * Globals.ChunkSize) + chunkX);
			int worldY = ((chunk.Coordinates.Y * Globals.ChunkSize) + chunkY);
			float depth = worldY + GetBiomeSurface(worldX) - GetBaseSurface(worldX, worldY);
			if (depth < 0)
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());
			}
			else if (depth <= 1.5f)
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Grass());
			}
			else if (depth <= 30)
			{
				chunk.SetTile(chunkX, chunkY, new Game.Tiles.Dirt());
				chunk.SetWall(chunkX, chunkY, new Game.Walls.Dirt());
			}
			else
			{
				chunk.SetWall(chunkX, chunkY, new Game.Walls.Stone());
				var noise = Simplex.Noise(worldX / 4.0f, worldY / 4.0f) * 30;
				if (noise + depth > 30.5)
					chunk.SetTile(chunkX, chunkY, new Game.Tiles.Stone());
				else
					chunk.SetTile(chunkX, chunkY, new Game.Tiles.Dirt());
			}
		}

		private void BadlandsPass(ref Chunk chunk, int chunkX, int chunkY)
        {
			int worldX = ((chunk.Coordinates.X * Globals.ChunkSize) + chunkX);
            int worldY = ((chunk.Coordinates.Y * Globals.ChunkSize) + chunkY);
            float depth = worldY + GetBiomeSurface(worldX) - GetBaseSurface(worldX, worldY);
            if (depth < 0)
            {
                chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());
            }
            else if (depth <= 3)
            {
                chunk.SetTile(chunkX, chunkY, new Game.Tiles.Sandstone());
            }
            else if (depth <= 6)
            {
                chunk.SetTile(chunkX, chunkY, new Game.Tiles.Clay());
                chunk.SetWall(chunkX, chunkY, new Game.Walls.Sandstone());
            }
            else if (depth <= 9)
            {
                chunk.SetTile(chunkX, chunkY, new Game.Tiles.Sandstone());
                chunk.SetWall(chunkX, chunkY, new Game.Walls.Sandstone());
            }
            else if (depth <= 12)
            {
                chunk.SetTile(chunkX, chunkY, new Game.Tiles.Clay());
                chunk.SetWall(chunkX, chunkY, new Game.Walls.Sandstone());
            }
            else if (depth <= 15)
            {
                chunk.SetTile(chunkX, chunkY, new Game.Tiles.Sandstone());
                chunk.SetWall(chunkX, chunkY, new Game.Walls.Sandstone());
            }
            else if (depth <= 18)
            {
                chunk.SetTile(chunkX, chunkY, new Game.Tiles.Clay());
                chunk.SetWall(chunkX, chunkY, new Game.Walls.Sandstone());
            }
			else
            {
                chunk.SetWall(chunkX, chunkY, new Game.Walls.Stone());
                var noise = Simplex.Noise(worldX / 4.0f, worldY / 4.0f) * 30;
                if (noise + depth > 30.5)
                    chunk.SetTile(chunkX, chunkY, new Game.Tiles.Stone());
                else
                    chunk.SetTile(chunkX, chunkY, new Game.Tiles.Dirt());
            }
		}

		//public float Get

		public void HeightPass(ref Chunk chunk)
		{

			for (int chunkX = 0; chunkX < Chunk.ChunkSize; chunkX++)
			{
				for (int chunkY = 0; chunkY < Chunk.ChunkSize; chunkY++)
				{

					int worldX = ((chunk.Coordinates.X * Globals.ChunkSize) + chunkX);
					int worldY = ((chunk.Coordinates.Y * Globals.ChunkSize) + chunkY);

					float depth = worldY + GetBiomeSurface(worldX) - GetBaseSurface(worldX, worldY);
					

					SurfaceBiome biome = GetDominantSurfaceBiome(worldX);

					// Surface Biome Generation
					if (biome == SurfaceBiome.Mountains)
						MountainsPass(ref chunk, chunkX, chunkY);

					if (biome == SurfaceBiome.Forest)
						ForestPass(ref chunk, chunkX, chunkY);

					if (biome == SurfaceBiome.Desert)
						DesertPass(ref chunk, chunkX, chunkY);

					if (biome == SurfaceBiome.Plains)
						PlainsPass(ref chunk, chunkX, chunkY);
					
					if (biome == SurfaceBiome.Badlands)
						BadlandsPass(ref chunk, chunkX, chunkY);

					// Cave and other structures

					// Mushroom Cave
					if (depth > 1000)
					{
						float noise = Simplex.Noise((worldX - 999) / 400.0f, (worldY + 420) / 400.0f);
						float n2 = (float)Octave4.Noise2D((worldX - 999) / 4.0f, (worldY + 420) / 4.0f);
						if (noise > 0.75f)
						{
							if (n2 < 0.10f)
								chunk.SetTile(chunkX, chunkY, new Mycelium());
							else
								chunk.SetTile(chunkX, chunkY, new Air());
						}
					}

					if (depth > 0)
					{
						if (Simplex.Noise(worldX / 50.0f, (worldY / 60.0f) + 4848) > 0.85f)
							chunk.SetTile(chunkX, chunkY, new Clay());

						if (Simplex.Noise((worldX + 444) / 45.0f, (worldY / 22.0f) + 458) > 0.92f)
							chunk.SetTile(chunkX, chunkY, new Granite());
					}

					// caves
					if (depth >= 0)
					{
						// jagged caves

						float cavemap = Simplex.Noise(worldX, worldY, xScale: 200, yScale: 200, xOffset: 60, yOffset: -200);

						//cavemap = FloatRange.I_Unit.NormalizeFrom(FloatRange.I_Negative1_Positive1, cavemap); // normalize from [-1, 1] to [0, 1]

						float caveDetail1 = Octave2.Noise2D(worldX, worldY, xScale: 8, yScale: 8, xOffset: 10, yOffset: 444);
						float caveDetail2 = Simplex.Noise(worldX, worldY, 20, 20, 55, 242);
						//caveDetail1 = FloatRange.I_Unit.NormalizeFrom(FloatRange.I_Negative1_Positive1, caveDetail1); // normalize from [-0.5, 0.5] to [0, 1]


						float caveCarve = (cavemap) - (caveDetail1*0.3f) + (caveDetail2*0.08f);

						float cavewidth = 0.13f;
						if (caveCarve > -cavewidth && caveCarve < cavewidth)
                        {
							if (depth < 50)
							{
								if (caveCarve < (-cavewidth) + 0.05f || caveCarve > (cavewidth - 0.05f))
								{
									if (chunk.GetTile(chunkX, chunkY).GetType() == typeof(Dirt))
                                    {
										chunk.SetTile(chunkX, chunkY, new Grass());
									}
								} else
                                {
									chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());
								}
							} else
                            {
								chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());
							}
							
							
						}

						

						
						/*if (cavetiny > 0.45f)
							chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());
						if (cave2 > -0.1f && cave2 < 0.1f)
							chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());

						if (depth > 5)
                        {
							if (cave1 * cavetiny > 0.5f)
								chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());
							if (cave1 > 0.8f)
								chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());
						}


						if (depth > 500)
                        {
							if (cave1 > 0.75f)
                            {
								chunk.SetTile(chunkX, chunkY, new Game.Tiles.Air());

								// bruh caves
								if (cave1 < 0.15f && cave2 < 0.15)
								{
									chunk.SetTile(chunkX, chunkY, new Game.Tiles.Mud());
								}
							}
							
						}*/

						if (depth > 50 && chunk.GetTile(chunkX, chunkY) is Game.Tiles.Air && Simplex.Noise(worldX / 5.0f, worldY / 8.0f) > 0.98f)
						{
							chunk.SetTile(chunkX, chunkY, new Cobweb());
						}

						

						if (depth > 150)
						{
							if (Simplex.Noise( (worldX + 444) / 100.0f, worldY / 100.0f) > 0.8f)
							{
								chunk.SetTile(chunkX, chunkY, new Lava { TileState = 8 });
							}
							if (Simplex.Noise(worldX / 400.0f, worldY / 400.0f) > 0.9f)
							{
								if (chunk.GetTile(chunkX, chunkY) is Air)
									chunk.SetTile(chunkX, chunkY, new Lava { TileState = 8 });
							}
						}
					}


					float liquidNoise = Simplex.Noise(worldX / 50.0f, worldY / 50.0f);
					if (liquidNoise > 0.8f && depth < 6)
					{
						float carve = (liquidNoise - 0.5f) * 6.0f;
						if (depth > 0 && depth + carve > 0)
						{
							chunk.SetWall(chunkX, chunkY, new Game.Walls.Air());
							chunk.SetTile(chunkX, chunkY, new Water());
						} 
					}
				}
			}
		}
		public void StructurePass(IGameWorld world, int x, int y)
		{
			float depth = y + GetBiomeSurface(x) - GetBaseSurface(x, y);
			if (GetDominantSurfaceBiome(x) == SurfaceBiome.Forest && depth < 3)
            {
				
				if (RNG.Next(7) == 2 && world.GetTile(x, y) is Game.Tiles.Grass && world.GetTile(x, y - 1) is Game.Tiles.Air)
				{
					TreeGenerator.GenerateTree(world, RNG, x, y - 1);
				}
			}
		}
	}
}
