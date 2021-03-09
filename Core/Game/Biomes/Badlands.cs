using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Biomes
{
	public class Badlands : Biome
	{
		public Badlands(Generator g) : base(g) { }


		public override float GetHeight(int x)
		{
			float baseline = Simplex.Noise(x / 80f) * 8;
			float raised = (float)(Octave4.Noise2D(x / 6.0f, (-x * 0.03f)) + 0.5f);

			if (raised > 0.4)
				raised = (raised * 4) + 5;

			return 40+baseline + raised;
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
	}
}
