using CaveGame.Common.WorldGeneration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common.Game.Biomes
{
	public class Forest : Biome
	{

		public Forest(Generator g) : base(g) { }

		public override float GetHeight(int x)
		{
			float n = (float)Simplex.Noise(x / 100.420f) * 10 + (float)Octave4.Noise2D(x / 60.0f, (-x * 0.10f) + (x / 20.0f)) * 30;
			return 20 + n;
		}

		public override void StructurePass(IGameWorld world, int x, int y)
		{
			float depth = y + Generator.GetBiomeSurface(x) - Generator.GetBaseSurface(x, y);
			if (depth < 3)
			{

				if (Generator.RNG.Next(7) == 2 && world.GetTile(x, y) is Game.Tiles.Grass && world.GetTile(x, y - 1) is Game.Tiles.Air)
				{
					TreeGenerator.GenerateTree(world, Generator.RNG, x, y - 1);
				}
			}
		}

		public override void SurfacePass(ref Chunk chunk, int chunkX, int chunkY)
		{
			int worldX = ((chunk.Coordinates.X * Globals.ChunkSize) + chunkX);
			int worldY = ((chunk.Coordinates.Y * Globals.ChunkSize) + chunkY);
			float depth = worldY + Generator.GetBiomeSurface(worldX) - Generator.GetBaseSurface(worldX, worldY);
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
	}
}
