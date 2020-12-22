#if CLIENT
using CaveGame.Client;

#endif

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DataManagement;
using CaveGame.Core.WorldGeneration;

namespace CaveGame.Core.Game.Tiles
{
	public class FurniturePointer : Tile
	{
		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 light) { } // leave empty
	}

	public class Void : Tile
	{
		public override void Drop(IGameServer server, IGameWorld world, Point tilePosition) { }
		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 light) { } // leave empty
	}

	public class Air : Tile, INonSolid, IWaterBreakable
	{
		public override void Drop(IGameServer server, IGameWorld world, Point tilePosition) { }
		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 light) { } // leave empty

	}
	public class Vacuum { }
	public class Fog { }
	public class Miasma { }




	public class Cobweb : Tile {
		public override byte Hardness => 1;
		public override Rectangle Quad => TileMap.Cobweb;
	}

	public class Ladder : Tile {
		public override Rectangle Quad => TileMap.Ladder;
		public override byte Hardness => 4;
		public override Color Color => new Color(0.8f, 0.5f, 0.3f);
	}
	public class Platform : Tile, IPlatformTile {
		public override byte Hardness => 2;
		public override byte Opacity => 2;
		public override Color Color => new Color(0.8f, 0.5f, 0.3f);
		public override Rectangle Quad => TileMap.Platform;
	}
	public class Rope : Tile, ITileUpdate, INonSolid {
		public override byte Hardness => 2;
		public override byte Opacity => 1;
		public override Rectangle Quad => TileMap.Rope;

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (world.IsTile<INonSolid>(x, y - 1) && !world.IsTile<Rope>(x, y - 1))
				world.SetTile(x, y, new Air());
		}
	}

}