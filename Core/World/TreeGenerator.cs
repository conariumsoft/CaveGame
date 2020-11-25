using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.WorldGeneration
{
    public static class TreeGenerator
    {
        public static void GenerateTree(IGameWorld world, Random rng, int x, int y)
        {
			int treeHeight = rng.Next(7, 16);
			int leavesSize = rng.Next(2, 4);

			for (int dx = -leavesSize; dx <= leavesSize; dx++)
			{
				for (int dy = -leavesSize; dy <= leavesSize; dy++)
				{
					//if (world.IsTile<Tiles.Air>(x, y))
						world.SetTile(dx + x, dy + y - treeHeight, new Core.Game.Tiles.OakLeaves());
				}
			}
			for (int ty = 0; ty < treeHeight; ty++)
			{
				world.SetTile(x, y-ty, new Game.Tiles.OakLog());
			}
		}
    }
}
