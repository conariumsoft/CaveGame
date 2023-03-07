using CaveGame.Common.Noise;
using CaveGame.Common.Game;
using System;
using CaveGame.Common.Extensions;
using CaveGame.Common.WorldGeneration;
using CaveGame.Common.Game.Tiles;
using System.Numerics;
using System.Collections.Generic;
using CaveGame.Common.Game.Biomes;


// Copyright 2020 Conarium Software
// author: joshms

namespace CaveGame.Common
{
	public enum SurfaceBiome : int
	{
		Forest = 0,
		Plains,
		Mountains,
		Desert,
		Badlands,

	}

    public class Generator
	{
		public const float BIOME_SIZE = 10000.5f;
		public const float BIOME_TRANSITION = 0.2f;

		public Dictionary<SurfaceBiome, Biome> Biomes { get; private set; }

		public int Seed { get; set; }

		public OctaveNoise Octave4;
		public OctaveNoise Octave8;
		public OctaveNoise Octave3;
		public OctaveNoise Octave2;
		public SimplexNoise Simplex;
		public Random RNG;



		private SurfaceBiome[] biomemap;
		
		// Generates the biome map.
		private void GenerateBiomeNoiseMap()
		{
			int sampleCounts = 100; 
			float biomeRepeatChance = 0.35f; // Skew biome probability towards repeating.
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

		public SurfaceBiome GetDominantSurfaceBiome(int x)
		{
			float alpha = (Simplex.Noise(x / BIOME_SIZE) + 1) / 2.0f; // transform from [-1, 1] to [0, 1]
			int biome = (int)Math.Floor(alpha * 100.0f);

			return biomemap[biome];
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

			Biomes = new Dictionary<SurfaceBiome, Biome>
			{
				[SurfaceBiome.Plains] = new Plains(this),
				[SurfaceBiome.Badlands] = new Badlands(this),
				[SurfaceBiome.Desert] = new Desert(this),
				[SurfaceBiome.Forest] = new Forest(this),
				[SurfaceBiome.Mountains] = new Mountain(this),
			};


			GenerateBiomeNoiseMap();
		}


		

		public float GetHeightmap(int x, SurfaceBiome biome) => Biomes[biome].GetHeight(x);

		public float GetBiomeSurface(int x)
        {
			SurfaceBiome primaryBiome = GetDominantSurfaceBiome(x);
			SurfaceBiome secondaryBiome = GetSecondarySurfaceBiome(x);

			var primaryScale= GetPrimarySurfaceBiomeScale(x);
			var secondScale = GetSecondarySurfaceBiomeScale(x);


			return (GetHeightmap(x, primaryBiome)*primaryScale) + (GetHeightmap(x, secondaryBiome)*secondScale);
		}


		public const float SURFACE_BASE_HEIGHT = -40;

		public float GetBaseSurface(int x, int y)
        {
			float surfacePass1 = Octave4.Noise2D(x: x, y: y, xScale: 10.0, yScale: 10.0, xOffset: -2405, yOffset: 222);
			float surfacePass2 = Octave4.Noise2D(x: x, y: y, xScale: 325.0, yScale: 299.0, xOffset: 2, yOffset: -9924);
			float surface = (surfacePass1*3) + (surfacePass2*10);
			return ( surface - SURFACE_BASE_HEIGHT);
		}



		// what the fuck?
		public SurfaceBiome GetSecondarySurfaceBiome(int x)
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

		public float GetPrimarySurfaceBiomeScale(int x)
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

		public float GetSecondarySurfaceBiomeScale(int x)
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

		public float GetPrimarySurfaceBiomeStrength(int x)
        {
			float alpha = (Simplex.Noise(x / BIOME_SIZE) + 1) / 2.0f; // transform from [-1, 1] to [0, 1]
			int biome = (int)Math.Floor(alpha * 100.0f);
			float biomedelta = (alpha * 100) - biome;
			return biomedelta;
		}


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

					Biomes[biome].SurfacePass(ref chunk, chunkX, chunkY);

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
			SurfaceBiome biome = GetDominantSurfaceBiome(x);
			Biomes[biome].StructurePass(world, x, y);
		}
	}
}
