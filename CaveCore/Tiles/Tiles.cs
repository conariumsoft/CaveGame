using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CaveGame.Core.Tiles
{
	public enum TileID : byte
	{
		Void = 255,
		Air = 0,
		Stone,
		Dirt,
		Grass,
		Log,
		Leaves,
		Water
	}
	public class Tile
	{
		public static Random RNG = new Random();
		public static byte IDOf<T>()
		{
			var type = typeof(T);

			byte id = (byte)Enum.Parse(typeof(TileID), type.Name);

			return id;
		}

		public static Tile FromID(byte t)
		{
			TileID tileid = (TileID)t;
			if (tileid == TileID.Air)
				return new Air();
			if (tileid == TileID.Stone)
				return new Stone();
			if (tileid == TileID.Dirt)
				return new Dirt();
			if (tileid == TileID.Grass)
				return new Grass();
			if (tileid == TileID.Log)
				return new Log();
			if (tileid == TileID.Leaves)
				return new Leaves();
			if (tileid == TileID.Water)
				return new Water();
			return new Air();
		}

		public byte ID
		{
			get {
				var name = this.GetType().Name;
				return (byte)Enum.Parse(typeof(TileID), name);
			}
		}
		public string Namespace => "CaveGame";
		public string TileName => this.Namespace + ":" + this.GetType().Name;
		public byte Hardness { get; protected set; }
		public Rectangle TileQuad { get; protected set; }
		public Color TileColor { get; protected set; }
		public byte Opacity { get; protected set; }
		

		public byte Damage { get; set; }
		public byte TileState { get; set; }


		public Tile()
		{

			TileColor = Color.White;
			Opacity = 4;
		}

		public Tile(byte id = 0, byte tileState = 0, byte damage = 0)
		{
			TileState = tileState;
			Damage = damage;
		}



		public virtual byte[] Serialize() {
			byte[] serializedTile = new byte[4];

			serializedTile[0] = ID;
			serializedTile[1] = Damage;
			serializedTile[2] = TileState;
			serializedTile[3] = 0; // reserved for future uses

			return serializedTile;
		}

		public virtual void Serialize(ref byte[] datagram, int pushIndex)
		{
			datagram[0 + pushIndex] = ID;
			datagram[1 + pushIndex] = Damage;
			datagram[2 + pushIndex] = TileState;
			datagram[3 + pushIndex] = 0; // reserved for future uses
		}


		public virtual void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{


			sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, y * Globals.TileSize), TileQuad, color.MultiplyAgainst(TileColor));
		}
	}

	public class Void : Air
	{

	}



	public class Air : Tile, INonSolid, IWaterBreakable//, ILightEmitter
	{


		public Air() : base()
		{
			//ID = (byte)TileIDs.Air;
			Hardness = 0;
			Opacity = 1;
			TileQuad = TileMap.Default;
		}

		public Light3 Light => new Light3(16, 16, 16);

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 light) { } // leave empty

	}

	public interface ILightEmitter
	{
		public Light3 Light { get; }
	}

	public interface IRandomTick
	{
		public void RandomTick(IGameWorld world, int x, int y);
	}

	public interface ITileUpdate {
		public void TileUpdate(IGameWorld world, int x, int y);
	}

	public interface IWaterBreakable { }
	public interface IBastard
	{
		int Test { get; set; }
	}

	public interface INonSolid { }

	public class Grass : Tile, IRandomTick
	{
		byte growth = 0;


		public Grass() : base()
		{
			//ID = (byte)TileIDs.Grass;
			Hardness = 2;
			TileQuad = TileMap.Soil;
			TileColor = Color.Green;
			Opacity = 3;
			
		}

		private bool CanBreathe(IGameWorld world, int x, int y)
		{
			var above = world.GetTile(x, y - 1);
			var below = world.GetTile(x, y + 1);
			var left = world.GetTile(x - 1, y);
			var right = world.GetTile(x + 1, y);
			var tleft = world.GetTile(x - 1, y - 1);
			var tright = world.GetTile(x + 1, y - 1);
			var bleft = world.GetTile(x - 1, y + 1);
			var bright = world.GetTile(x + 1, y + 1);

			return (above is INonSolid || below is INonSolid || left is INonSolid || right is INonSolid || tleft is INonSolid || tright is INonSolid || bleft is INonSolid || bright is INonSolid);
		}

		public void RandomTick(IGameWorld world, int x, int y)
		{
			var above = world.GetTile(x, y - 1);
			var below = world.GetTile(x, y + 1);
			var left = world.GetTile(x - 1, y);
			var right = world.GetTile(x + 1, y);


			if (!CanBreathe(world, x, y))
			{
				// suffocate

				world.SetTile(x, y, new Dirt());
				return;
			}

			growth++;

			if (growth > 4)
			{
				growth = 0;
				if (below.ID == IDOf<Dirt>() && CanBreathe(world, x, y + 1))
				{
					world.SetTile(x, y + 1, new Grass());
					return;
				}

				if (left.ID == IDOf<Dirt>() && CanBreathe(world, x - 1, y))
				{
					world.SetTile(x - 1, y, new Grass());
					return;
				}


				if (right.ID == IDOf<Dirt>() && CanBreathe(world, x + 1, y))
				{
					world.SetTile(x + 1, y, new Grass());
					return;
				}


				if (above.ID == IDOf<Dirt>() && CanBreathe(world, x, y - 1))
				{
					world.SetTile(x, y - 1, new Grass());
					return;
				}
			}
		}
	}

	public class Dirt : Tile
	{

		public Dirt() : base() {
			//ID = (byte)TileIDs.Dirt;
			Hardness = 2;
			Opacity = 3;
			TileQuad = TileMap.Soil;
			TileColor = Color.SaddleBrown;
		}
	}

	public interface ILiquid { }

	public class Water : Tile, ILiquid, INonSolid, ITileUpdate, IRandomTick, ILightEmitter
	{

		public Light3 Light => new Light3(16, 0, 0);

		public Water() { }

		private bool EvaporationCheck(IGameWorld world, int x, int y)
		{
			if (TileState < 1)
			{
				world.SetTile(x, y, new Air());
				return true;
			}
			return false;
		}

		public void RandomTick(IGameWorld world, int x, int y)
		{
			//world.SetTileUpdated(x, y);
		}

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			EvaporationCheck(world, x, y);
				
			var below = world.GetTile(x, y + 1);

			if (below is IWaterBreakable s)
			{
				world.SetTile(x, y+1, new Water { TileState = this.TileState });
				world.SetTile(x, y, new Air());
				world.DoUpdatePropogation(x, y);
				world.SetTileNetworkUpdated(x, y);
				world.SetTileNetworkUpdated(x, y + 1);
				return;
			} 

			if (below is Water wbelow)
			{
				var most = 8 - below.TileState;

				byte taken = (byte)Math.Min(most, TileState);
				TileState -= taken;
				below.TileState += taken;
				world.DoUpdatePropogation(x, y);
				world.DoUpdatePropogation(x, y+1);
				world.SetTileNetworkUpdated(x, y);
				world.SetTileNetworkUpdated(x, y + 1);
				//return;
			}

			var left = world.GetTile(x - 1, y);

			if (left is IWaterBreakable && TileState > 0)
			{

				world.SetTile(x - 1, y, new Water { TileState = 1 });
				this.TileState--;
				world.DoUpdatePropogation(x, y);
				world.DoUpdatePropogation(x - 1, y);

				world.SetTileNetworkUpdated(x, y);
				world.SetTileNetworkUpdated(x - 1, y);
			}

			var right = world.GetTile(x + 1, y);

			if (right is IWaterBreakable && TileState > 0)
			{
				world.SetTile(x + 1, y, new Water { TileState = 1 });
				this.TileState--;
				world.DoUpdatePropogation(x, y);
				world.DoUpdatePropogation(x + 1, y);
				world.SetTileNetworkUpdated(x, y);
				world.SetTileNetworkUpdated(x + 1, y);
			}

			if (left is Water wleft && TileState > wleft.TileState)
			{
				if (TileState > wleft.TileState+1)
				{
					TileState--;
					wleft.TileState++;
				} else if (TileState-wleft.TileState == 1)
				{
					if (RNG.NextDouble() > 0.5)

						wleft.TileState = TileState;
					else
						TileState = wleft.TileState;
				}
				world.DoUpdatePropogation(x, y);
				world.DoUpdatePropogation(x - 1, y);

				world.SetTileNetworkUpdated(x, y);
				world.SetTileNetworkUpdated(x-1, y);
			}

			if (right is Water wright && TileState > wright.TileState)
			{
				if (TileState > wright.TileState + 1)
				{
					TileState--;
					wright.TileState++;
				}
				else if (TileState - wright.TileState == 1)
				{
					if (RNG.NextDouble() > 0.5)

						wright.TileState = TileState;
					else
						TileState = wright.TileState;
				}
				world.DoUpdatePropogation(x, y);
				world.DoUpdatePropogation(x + 1, y);
				world.SetTileNetworkUpdated(x, y);
				world.SetTileNetworkUpdated(x + 1, y);
			}
			EvaporationCheck(world, x, y);
		}


		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 light)
		{

			var rect = new Rectangle(15 * Globals.TileSize, (7 * Globals.TileSize), Globals.TileSize, TileState);
			sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, (y * Globals.TileSize)+(8-TileState)), rect, light.MultiplyAgainst(Color.Blue));
		}

	}


	public class Stone : Tile
	{

		public Stone() : base()
		{
		//	ID = (byte)TileIDs.Stone;
			Hardness = 10;
			Opacity = 3;
			TileQuad = TileMap.Stone;
			TileColor = new Color(0.7f, 0.7f, 0.7f);
		}

	}

	public class Log : Tile
	{
		public Log() : base()
		{
			TileQuad = TileMap.Log;
			TileColor = new Color(0.6f, 0.5f, 0.2f);
			Hardness = 5;
			Opacity = 1;
		}
	}

	public class Leaves : Tile
	{
		public Leaves() : base()
		{
			TileQuad = TileMap.Leaves;
			TileColor = new Color(0.1f, 0.9f, 0.1f);
			Hardness = 5;
			Opacity = 0;
		}
	}

}
