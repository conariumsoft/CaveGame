using CaveGame.Core.Generic;
using CaveGame.Core.Inventory;
using CaveGame.Core.WorldGeneration;
using DataManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Tiles
{
	#region Logs
	public class Log : Tile, INonSolid
	{

	}
	public class OakLog : Log, ITileUpdate
	{

		public override byte Hardness => 3;
		public override byte Opacity => 1;
		public override Rectangle Quad => TileMap.Log;
		public override Color Color => new Color(0.6f, 0.3f, 0.2f);

		public override void Drop(IGameServer server, IGameWorld world, Point tilePosition)
		{
			ItemStack stack = new ItemStack { Quantity = RNG.Next(1, 4), Item = new TileItem(new OakPlank()) };
			Drop(server, world, tilePosition, stack);
		}


		public void TileUpdate(IGameWorld world, int x, int y)
		{
			var below = world.GetTile(x, y + 1);


			if (!(below is ISoil) && !(below is OakLog))
				world.BreakTile(x, y);
		}
	}
	public class RedwoodLog { }
	public class BirchLog { }
	public class PineLog { }
	public class EbonyLog { }
	#endregion
	#region Planks

	public abstract class Plank : Tile, ILocalTileUpdate
	{
		public override byte Hardness => 3;
		public override Rectangle Quad => TileMap.Plank;


		public override void Draw(GraphicsEngine gfx, int x, int y, Light3 color)
		{


			var Plank = new Rectangle(2 * Globals.TileSize, 0, 4, Globals.TileSize);
			var PlankRight = new Rectangle((2 * Globals.TileSize) + 4, 0 * Globals.TileSize, 4, Globals.TileSize);
			var RPlank = new Rectangle(8 * Globals.TileSize, 0, 4, Globals.TileSize);

			var RPlankRight = new Rectangle((8 * Globals.TileSize + 4), 0, 4, Globals.TileSize);

			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);

			var texture = gfx.TileSheet;
			var outputColor = color.MultiplyAgainst(Color);

			if (TileState.Get(1)) // cornerd
				gfx.Sprite(texture, position, RPlank, outputColor, Rotation.Zero, Vector2.Zero, 1, SpriteEffects.None, 1);
			else
				gfx.Sprite(texture, position, Plank, outputColor, Rotation.Zero, Vector2.Zero, 1, SpriteEffects.None, 1);

			if (TileState.Get(0)) // cornerc
				gfx.Sprite(texture, position + new Vector2(4, 0), RPlankRight, outputColor, Rotation.Zero, Vector2.Zero, 1, SpriteEffects.None, 1);
			else
				gfx.Sprite(texture, position + new Vector2(4, 0), PlankRight, outputColor, Rotation.Zero, Vector2.Zero, 1, SpriteEffects.None, 1);

#if EDITOR
			base.Draw(gfx, x, y, color);
#endif
		}

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			bool planetop = IsEmpty(world, x, y - 1);
			bool planebottom = IsEmpty(world, x, y + 1);
			bool planeleft = IsEmpty(world, x - 1, y);
			bool planeright = IsEmpty(world, x + 1, y);

			byte newNumber = TileState;
			//newNumber.Set(3, planetop);
			//newNumber.Set(2, planeleft);
			newNumber.Set(1, planeleft && planetop && planebottom);
			newNumber.Set(0, planeright && planetop && planebottom);



			if (TileState != newNumber)
			{
				TileState = newNumber;
				world.DoUpdatePropogation(x, y);
			}
		}
	}

	public class OakPlank : Plank
	{
		public override Color Color => new Color(0.8f, 0.55f, 0.25f);
	}
	public class RedwoodPlank : Plank
	{
		public override Color Color => new Color(163, 82, 65);
	}
	public class PinePlank : Plank
	{
		public override Color Color => new Color(148, 109, 92);
	}
	public class EbonyPlank : Plank
	{
		public override Color Color => new Color(74, 74, 74);
	}
	#endregion

	public abstract class Sapling : Tile, INonSolid, IWaterBreakable, ITileUpdate, IRandomTick
	{
		public virtual void TreeGenerate(IGameWorld world, int x, int y) { }
		public virtual byte MatureAge => 10;

		public byte Age => TileState;
		public override byte Hardness => 1;
		public override byte Opacity => 0;
		public override void Drop(IGameServer s, IGameWorld w, Point t) { }// do nothing

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!world.IsTile<ISoil>(x, y + 1))
				world.SetTile(x, y, new Air());
		}

		public void RandomTick(IGameWorld world, int x, int y)
		{
			TileState++;
			if (TileState >= MatureAge)
				TreeGenerate(world, x, y);
		}
	}

	public class OakSapling : Sapling
	{
		public override Rectangle Quad => TileMap.Sapling;
		public override byte MatureAge => 10;
		public override void TreeGenerate(IGameWorld world, int x, int y) => TreeGenerator.GenerateTree(world, RNG, x, y);
	}
	public class OakLeaves : Tile, INonSolid, IRandomTick
	{
        public override Rectangle Quad => TileMap.Leaves;
        public override Color Color => Color.Green;


        public override void Drop(IGameServer server, IGameWorld world, Point tilePosition)
		{
			if (RNG.NextDouble() > 0.75)
			{
				ItemStack stack = new ItemStack { Quantity = RNG.Next(1, 2), Item = new TileItem(new OakSapling()) };
				Drop(server, world, tilePosition, stack);
			}

		}

		public void RandomTick(IGameWorld world, int x, int y)
		{
			for (int tx = -4; tx < 4; tx++)
			{
				for (int ty = -4; ty < 4; ty++)
				{
					if (world.IsTile<OakLog>(x + tx, y + ty))
						TileState = 1;
				}
			}

			if (TileState == 0)
			{
				world.BreakTile(x, y);
			}

		}
	}
	public class PineNeedles { }
}
