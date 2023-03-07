using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common.Game.Biomes
{
	public class Plains : Biome
	{
		public Plains(Generator gen) : base(gen) { }

		public override float GetHeight(int x)
		{
			return Simplex.Noise(x / 100.420f) * 4;
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
	}
}
