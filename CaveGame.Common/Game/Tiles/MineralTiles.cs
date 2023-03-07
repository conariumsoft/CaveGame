using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common.Game.Tiles
{
	#region Minerals
	public class Granite : Tile
	{
		public override byte Hardness => 5;
		public override Rectangle Quad => TileMap.Stone;
		public override Color Color => new Color(1, 0.85f, 0.85f);
	}
	public class Sovite { }
	public class Pyrite { }
	public class Cinnabar { }
	public class Magnetite { }
	public class Arsenic { }
	public class Gallium { }
	#endregion
}
