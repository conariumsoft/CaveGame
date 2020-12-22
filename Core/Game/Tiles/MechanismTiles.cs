using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Tiles
{
	public class TNT : Tile, ITileUpdate
	{
		public override Rectangle Quad => TileMap.TNT;
		public override byte Hardness => 1;
		public void TileUpdate(IGameWorld world, int x, int y) { }
	}
	public class Switch { }
	public class ANDGate { }
	public class ORGate { }
	public class XORGate { }
	public class Diode { }
	public class NORGate { }
	public class XANDGate { }
	public class Pump { }
	public class Pipe { }
	public class Trapdoor { }
}
