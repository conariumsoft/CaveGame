using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common.Game.Biomes
{
	public class Desert : Biome
	{

		public Desert(Generator gen) : base(gen) { }

		public override float GetHeight(int x)
		{
			float n = (float)Simplex.Noise(x / 100.420f) * 10 + (float)Octave4.Noise2D(x / 100.0f, (-x * 0.10f) + (x / 20.0f)) * 20;
			return n;
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
				chunk.SetTile(chunkX, chunkY, new Tiles.Air());
			}
			else if (depth <= 1.5f)
			{
				chunk.SetTile(chunkX, chunkY, new Tiles.Sand());
			}
			else if (depth <= 5)
			{
				chunk.SetTile(chunkX, chunkY, new Tiles.Sand());
				chunk.SetWall(chunkX, chunkY, new Walls.Sandstone());
			}
			else
			{
				chunk.SetWall(chunkX, chunkY, new Game.Walls.Sandstone());
				var noise = Simplex.Noise(chunkX / 4.0f, chunkY / 4.0f) * 30;
				if (noise + depth > 30.5)
					chunk.SetTile(chunkX, chunkY, new Tiles.Sandstone());
				else
					chunk.SetTile(chunkX, chunkY, new Tiles.Sand());
			}
		}
	}
}
