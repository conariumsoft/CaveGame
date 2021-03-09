using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Biomes
{
	public class Mountain : Biome
	{

		public Mountain(Generator g) : base(g) { }

		public override float GetHeight(int x)
		{
			float simplex = (float)Simplex.Noise(x / 50.420f);
			float octave = (float)Octave8.Noise2D(x / 120.0f, (-x * 0.05f) + (x / 40.0f));
			float n = (simplex * 15) * (octave * 15);
			float a = Simplex.Noise(x / 10.0f) * 2;
			float b = Simplex.Noise(x / 5.0f) * 2;
			return (a + b + 100) + n;
		}

		public override void StructurePass(IGameWorld world, int x, int y)
		{

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
	}
}
