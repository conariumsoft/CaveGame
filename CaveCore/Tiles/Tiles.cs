#if CLIENT
using CaveGame.Client;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
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
		OakLog,
		Leaves,
		Water,
		StoneBrick,
		ClayBrick,
		Torch,
		Clay,
		OakPlank,
		Granite,
		Sandstone,
		Mud,
		Sand,
		RedTorch,
		BlueTorch,
		GreenTorch,
		YellowTorch,
		WhiteTorch,
		CopperOre,
		IronOre,
		GoldOre,
		UraniumOre,
		CobaltOre,
		LeadOre,
	}


	public class TDef: ILightEmitter // TileData
	{
		public byte ID { get; set; }
		public Color Color { get; set; }
		public Rectangle Quad { get; set; }
		public byte Opacity { get; set; }
		public byte Hardness { get; set; }
		public Light3 Light { get; set; }
		public float Friction { get; set; }

		public TDef()
		{
			Color = Color.White;
			Hardness = 4;
			Opacity = 4;
			Quad = TileMap.Brick;
			Friction = 1;
		}


	}

	public static class TileDefinitions
	{
public static TDef Void        = new TDef {};
public static TDef Air		   = new TDef { Hardness = 0,  Opacity = 1, Quad = TileMap.Default };
public static TDef Dirt		   = new TDef { Hardness = 2,  Opacity = 3, Quad = TileMap.Soil,	 Color = Color.SaddleBrown };
public static TDef Water       = new TDef { Hardness = 0,  Opacity = 2, Quad = TileMap.Default,	 Color = Color.Blue };
public static TDef Grass       = new TDef { Hardness = 2,  Opacity = 3, Quad = TileMap.Soil,	 Color = Color.Green };
public static TDef Stone       = new TDef { Hardness = 10, Opacity = 3, Quad = TileMap.Stone,	 Color = new Color(0.7f, 0.7f, 0.7f) };
public static TDef StoneBrick  = new TDef { Hardness = 10, Opacity = 3, Quad = TileMap.Brick,	 Color = new Color(0.7f, 0.7f, 0.7f) };
public static TDef ClayBrick   = new TDef { Hardness = 10, Opacity = 3, Quad = TileMap.Brick,	 Color = new Color(0.85f, 0.4f, 0.4f) };
public static TDef Log         = new TDef { Hardness = 5, Opacity = 1, Quad = TileMap.Log, Color = new Color(0.6f, 0.3f, 0.2f) };
public static TDef Leaves      = new TDef { Hardness = 5, Opacity = 0, Quad = TileMap.Leaves, Color = new Color(0.1f, 0.9f, 0.1f) };
public static TDef Torch	   = new TDef { Hardness = 2, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.8f, 0.8f, 0.4f) };
public static TDef WhiteTorch  = new TDef { Hardness = 2, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.8f, 0.8f, 0.8f) };
public static TDef YellowTorch = new TDef { Hardness = 2, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.9f, 0.9f, 0.2f) };
public static TDef RedTorch    = new TDef { Hardness = 2, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.9f, 0.2f, 0.2f) };
public static TDef BlueTorch   = new TDef { Hardness = 2, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.2f, 0.2f, 0.9f) };
public static TDef GreenTorch  = new TDef { Hardness = 2, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.2f, 0.9f, 0.2f) };

public static TDef Mud		 = new TDef { Hardness = 2, Opacity = 3, Quad = TileMap.Soil, Color = new Color(0.3f, 0.1f, 0.1f) };
public static TDef Sand		 = new TDef { Hardness = 2, Opacity = 3, Quad = TileMap.Soil, Color = new Color(0.8f, 0.6f, 0.2f) };
public static TDef Granite	 = new TDef { Hardness = 10, Opacity = 3, Quad = TileMap.Soil, Color = new Color(0.8f, 0.7f, 0.7f) };
public static TDef Sandstone = new TDef { Hardness = 10, Opacity = 3, Quad = TileMap.Stone, Color = new Color(0.7f, 0.6f, 0.4f) };
public static TDef Clay      = new TDef { Hardness = 3, Opacity = 3, Quad = TileMap.Soil, Color = new Color(0.8f, 0.5f, 0.2f )};
public static TDef OakPlank  = new TDef { Hardness = 6, Opacity = 3, Quad = TileMap.Plank, Color = new Color(0.8f, 0.5f, 0.3f) };
public static TDef CopperOre = new TDef { Hardness = 12, Opacity = 3, Quad = TileMap.Ore, Color = new Color(1.0f, 0.45f, 0f) };
public static TDef LeadOre   = new TDef { Hardness = 12, Opacity = 3, Quad = TileMap.Ore, Color = new Color(0.35f, 0.35f, 0.4f) };
public static TDef TinOre    = new TDef { Hardness = 12, Opacity = 3, Quad = TileMap.Ore, Color = new Color(0.65f, 0.4f, 0.4f) };
public static TDef IronOre   = new TDef { Hardness = 12, Opacity = 3, Quad = TileMap.Ore, Color = new Color(1.0f, 0.75f, 0.75f) };
public static TDef CobaltOre = new TDef { Hardness = 12, Opacity = 3, Quad = TileMap.Ore, Color = new Color(0.3f, 0.3f, 1f) };
public static TDef GoldOre   = new TDef { Hardness = 12, Opacity = 3, Quad = TileMap.Ore, Color = new Color(1f, 1f, 0.5f) };
public static TDef UraniumOre= new TDef { Hardness = 12, Opacity = 3, Quad = TileMap.Ore, Color = new Color(0.35f, 0.8f, 0.35f) };
	}

	public interface IWaterBreakable { }
	public interface INonSolid { }
	// Property interfaces
	public interface ILightEmitter
	{
		public Light3 Light { get; }
	}
	// Method interfaces

	public interface ILocalTileUpdate
	{
		public void LocalTileUpdate(IGameWorld world, int x, int y);
	}
	public interface IRandomTick
	{
		public void RandomTick(IGameWorld world, int x, int y);
	}
	public interface ITileUpdate
	{
		public void TileUpdate(IGameWorld world, int x, int y);
	}
	public interface ISoil { }
	public interface IGas { }
	public interface ILiquid { }
	public interface IVegetation { }
	public interface IOre { }
	public interface IMineral { }

	public class Tile : IEquatable<Tile>
	{
#if CLIENT
		public static Texture2D Tilesheet = GameTextures.TileSheet;
#else
		public static Texture2D Tilesheet;
#endif
		public static Random RNG = new Random();

		public static byte IDOf<T>()
		{
			var type = typeof(T);

			byte id = (byte)Enum.Parse(typeof(TileID), type.Name);

			return id;
		}

		public static Tile FromID(byte t)
		{
			var basetype = typeof(Tile);
			var types = basetype.Assembly.GetTypes().Where(type => type.IsSubclassOf(basetype));


			foreach (var type in types)
			{

				bool exists = Enum.TryParse(typeof(TileID), type.Name, out object id);
				if (exists && (TileID)id == (TileID)t)
					return (Tile)type.GetConstructor(Type.EmptyTypes).Invoke(null);
			}
			throw new Exception("ID not valid!");
		}

		public byte Opacity => data.Opacity;
		public Color Color => data.Color;
		public Rectangle Quad => data.Quad;
		public byte Hardness => data.Hardness;

		private TDef data;

		public Tile(TDef tdata)
		{
			data = tdata;
		}

		public byte ID
		{
			get {
				var name = this.GetType().Name;
				return (byte)Enum.Parse(typeof(TileID), name);
			}
		}
		public string Namespace => "CaveGame";
		public string TileName => this.GetType().Name;
		
		public byte Damage { get; set; }
		public byte TileState { get; set; }


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
			sb.Draw(
				Tilesheet, 
				new Vector2(x * Globals.TileSize, y * Globals.TileSize), 
				Quad, color.MultiplyAgainst(Color), 0,
				Vector2.Zero, 1, SpriteEffects.None, 0.5f
			);
		}

		public bool Equals([AllowNull] Tile other)
		{
			return (other.ID == ID && other.TileState == TileState && other.Damage == Damage);
		}

		protected bool IsEmpty(IGameWorld w, int x, int y)
		{
			if (w.GetTile(x, y) is INonSolid)
			{
				return true;
			}
			return false;
		}

		protected bool IsThis(IGameWorld w, int x, int y)
		{
			return (w.GetTile(x, y).ID == ID);
		}

	}
	public class Void : Tile
	{
		public Void() : base(TileDefinitions.Void) { }
		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 light) { } // leave empty
	}

#region Gases
	public class Air : Tile, INonSolid, IWaterBreakable
	{
		public Air() : base(TileDefinitions.Air) { }
		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 light) { } // leave empty
	}
	public class Vacuum { }
	public class Fog { }
	public class Miasma { }
#endregion
#region Soils
	public class Grass : Tile, IRandomTick, ITileUpdate, ILocalTileUpdate, ISoil
	{
		public Grass() : base(TileDefinitions.Grass) { }

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

		public static Rectangle Patch = new Rectangle(8 * Globals.TileSize, 6 * Globals.TileSize, Globals.TileSize, Globals.TileSize);

		private bool IsGrass(IGameWorld w, int x, int y)
		{
			if (w.GetTile(x, y) is Grass)
			{
				return true;
			}
			return false;
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


			if (below is Dirt && CanBreathe(world, x, y + 1))
			{
				world.SetTile(x, y + 1, new Grass());
				//return;
			}

			if (left is Dirt && CanBreathe(world, x - 1, y))
			{
				world.SetTile(x - 1, y, new Grass());
				//return;
			}


			if (right is Dirt && CanBreathe(world, x + 1, y))
			{
				world.SetTile(x + 1, y, new Grass());
				//return;
			}


			if (above is Dirt && CanBreathe(world, x, y - 1))
			{
				world.SetTile(x, y - 1, new Grass());
				//return;
			}
		}


		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, y * Globals.TileSize), TileMap.Soil, color.MultiplyAgainst(Color.SaddleBrown));
			var corner = new Rectangle(9 * Globals.TileSize, 6 * Globals.TileSize, Globals.TileSize, Globals.TileSize);
			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize) + new Vector2(4, 4);

			if (TileState.Get(0)) // cornerd
				sb.Draw(tilesheet, position, corner, color.MultiplyAgainst(Color.Green), MathHelper.ToRadians(270), new Vector2(4, 4), 1, SpriteEffects.None, 1);
			if (TileState.Get(1)) // cornerc
				sb.Draw(tilesheet, position, corner, color.MultiplyAgainst(Color.Green), MathHelper.ToRadians(180), new Vector2(4, 4), 1, SpriteEffects.None, 1);
			if (TileState.Get(2)) // cornerb
				sb.Draw(tilesheet, position, corner, color.MultiplyAgainst(Color.Green), MathHelper.ToRadians(90), new Vector2(4, 4), 1, SpriteEffects.None, 1);

			if (TileState.Get(3)) // cornera
				sb.Draw(tilesheet, position, corner, color.MultiplyAgainst(Color.Green), 0, new Vector2(4, 4), 1, SpriteEffects.None, 1);

			if (TileState.Get(4)) // planeright
				sb.Draw(tilesheet, position, Patch, color.MultiplyAgainst(Color.Green), MathHelper.ToRadians(90), new Vector2(4, 4), 1, SpriteEffects.None, 1);

			if (TileState.Get(5)) // planebottom
				sb.Draw(tilesheet, position, Patch, color.MultiplyAgainst(Color.Green), MathHelper.ToRadians(0), new Vector2(4, 4), 1, SpriteEffects.FlipVertically, 1);

			if (TileState.Get(6)) // planeleft
				sb.Draw(tilesheet, position, Patch, color.MultiplyAgainst(Color.Green), MathHelper.ToRadians(270), new Vector2(4, 4), 1, SpriteEffects.None, 1);

			if (TileState.Get(7)) // planetop
				sb.Draw(tilesheet, position, Patch, color.MultiplyAgainst(Color.Green), MathHelper.ToRadians(0), new Vector2(4, 4), 1, SpriteEffects.None, 1);

		}

		public void TileUpdate(IGameWorld w, int x, int y) { }

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			bool planetop = IsEmpty(world, x, y - 1);
			bool planebottom = IsEmpty(world, x, y + 1);
			bool planeleft = IsEmpty(world, x - 1, y);
			bool planeright = IsEmpty(world, x + 1, y);
			bool air_tl = IsEmpty(world, x - 1, y - 1);
			bool air_bl = IsEmpty(world, x - 1, y + 1);
			bool air_tr = IsEmpty(world, x + 1, y - 1);
			bool air_br = IsEmpty(world, x + 1, y + 1);
			bool gabove = IsGrass(world, x, y - 1);
			bool gbelow = IsGrass(world, x, y + 1);
			bool gleft = IsGrass(world, x - 1, y);
			bool gright = IsGrass(world, x + 1, y);
			bool cornera = (gleft && gabove && air_tl);
			bool cornerb = (gright && gabove && air_tr);
			bool cornerc = (gright && gbelow && air_br);
			bool cornerd = (gleft && gbelow && air_bl);
			byte newNumber = TileState;
			newNumber.Set(7, planetop);
			newNumber.Set(6, planeleft);
			newNumber.Set(5, planebottom);
			newNumber.Set(4, planeright);
			newNumber.Set(3, cornera);
			newNumber.Set(2, cornerb);
			newNumber.Set(1, cornerc);
			newNumber.Set(0, cornerd);


			if (TileState != newNumber)
			{
				TileState = newNumber;
				world.DoUpdatePropogation(x, y);
			}
		}
	}
	public class Dirt : Tile, ISoil
	{
		public Dirt() : base(TileDefinitions.Dirt) {}
	}
	public class Mud : Tile, ISoil
	{
		public Mud() : base(TileDefinitions.Mud) { }
	}
	public class Sand : Tile
	{
		public Sand() : base(TileDefinitions.Sand) { }
	}

#endregion
#region Liquids

	public class Water : Tile, ILiquid, INonSolid, ITileUpdate, IRandomTick, ILocalTileUpdate
	{
		public Water() : base(TileDefinitions.Water) { TileState = 8; }

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

		public void TileUpdate(IGameWorld world, int x, int y) { }

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			EvaporationCheck(world, x, y);

			var below = world.GetTile(x, y + 1);

			if (below is IWaterBreakable s)
			{
				world.SetTile(x, y + 1, new Water { TileState = this.TileState });
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
				world.DoUpdatePropogation(x, y + 1);
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
				if (TileState > wleft.TileState + 1)
				{
					TileState--;
					wleft.TileState++;
				} else if (TileState - wleft.TileState == 1)
				{
					if (RNG.NextDouble() > 0.5)

						wleft.TileState = TileState;
					else
						TileState = wleft.TileState;
				}
				world.DoUpdatePropogation(x, y);
				world.DoUpdatePropogation(x - 1, y);

				world.SetTileNetworkUpdated(x, y);
				world.SetTileNetworkUpdated(x - 1, y);
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

			var rect = new Rectangle(0, 15*Globals.TileSize, Globals.TileSize, TileState);
			sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, (y * Globals.TileSize) + (8 - TileState)), rect, light.MultiplyAgainst(Color.Blue));
		}
	}
	public class Magma { }
	public class Sludge { }
	public class Ectoplasm { }
	public class Oil { }
	public class LiquidOxygen { }
	public class LiquidNitrogen { }
#endregion
#region Misc
	public class Cobweb { }

#endregion
#region Plastics
	public abstract class Plastic { }
	public class RedPlastic : Plastic { }
	public class BluePlastic : Plastic { }
	public class YellowPlastic : Plastic { }
	public class GreenPlastic : Plastic { }
#endregion
#region Stone&Bricks
	public class Stone : Tile
	{
		public Stone() : base(TileDefinitions.Stone) { }
	}
	public class Sandstone : Tile
	{
		public Sandstone() : base(TileDefinitions.Sandstone) { }
	}
	public class Glass { }
	public class Limestone { }
	public class Obsidian { }
	public class Brimstone { }
	public class Snow { }
	public class Ice { }
	public class IceBrick { }
#endregion
#region Minerals
	public class Granite : Tile
	{
		public Granite() : base(TileDefinitions.Granite) { }
	}
	public class Sovite
	{
	
	}
	public class Pyrite { }
	public class Cinnabar { }
	public class Magnetite { }
#endregion

#region Ores
	public class Ore : Tile, ILocalTileUpdate
	{
		public Ore(TDef data) : base(data) { }
#if CLIENT

		private void DrawMask(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color, int rotation, Rectangle quad, Color tilecolor)
		{
			if (GameGlobals.GraphicsDevice != null)
			{
				var position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);
				var pixels4 = new Vector2(4, 4);

				sb.Draw(tilesheet, position + pixels4, quad, color.MultiplyAgainst(tilecolor), MathHelper.ToRadians(rotation), new Vector2(4, 4), 1, SpriteEffects.None, 1);
			}
		}

		private void DrawDirtMask(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color, int rotation)
		{
			DrawMask(tilesheet, sb, x, y, color, rotation, TileMap.DirtFading, TileDefinitions.Dirt.Color);
		}

		private void DrawStoneMask(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color, int rotation)
		{
			DrawMask(tilesheet, sb, x, y, color, rotation, TileMap.StoneFading, TileDefinitions.Stone.Color);
		}

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			var position = new Vector2(x*Globals.TileSize, y * Globals.TileSize);
			
			sb.Draw(tilesheet, position, Quad, color.MultiplyAgainst(Color));

			//sb.End();

			if (TileState.Get(0)) // Top Dirt
				DrawDirtMask(tilesheet, sb, x, y, color, 0);
			if (TileState.Get(1)) // Top Stone
				DrawStoneMask(tilesheet, sb, x, y, color, 0);
			if (TileState.Get(2)) // Bottom Dirt
				DrawDirtMask(tilesheet, sb, x, y, color, 180);
			if (TileState.Get(3)) // Bottom Stone
				DrawStoneMask(tilesheet, sb, x, y, color, 180);

			if (TileState.Get(4)) // Left Dirt
				DrawDirtMask(tilesheet, sb, x, y, color, 270);

			if (TileState.Get(5)) // Left Stone
				DrawStoneMask(tilesheet, sb, x, y, color, 270);

			if (TileState.Get(6)) // Right Dirt
				DrawDirtMask(tilesheet, sb, x, y, color, 90);

			if (TileState.Get(7)) // Right Stone
				DrawStoneMask(tilesheet, sb, x, y, color, 90);

			//sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		}
#endif
		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			var top = world.GetTile(x, y - 1);
			var bottom = world.GetTile(x, y + 1);
			var left = world.GetTile(x - 1, y);
			var right = world.GetTile(x + 1, y);

			byte val = 0;

			if (top is Dirt)
				val.Set(0, true);
			if (top is Stone)
				val.Set(1, true);
			if (bottom is Dirt)
				val.Set(2, true);
			if (bottom is Stone)
				val.Set(3, true);
			if (left is Dirt)
				val.Set(4, true);
			if (left is Stone)
				val.Set(5, true);
			if (right is Dirt)
				val.Set(6, true);
			if (right is Stone)
				val.Set(7, true);

			TileState = val;
		}
	}

	public class CopperOre : Ore {
		public CopperOre() : base(TileDefinitions.CopperOre) { }
	}
	public class LeadOre : Ore {
		public LeadOre() : base(TileDefinitions.LeadOre) { }
	}
	public class TinOre: Ore {
		public TinOre() : base(TileDefinitions.TinOre) { }
	}
	public class ChromiumOre { }
	public class AluminiumOre { }
	public class IronOre : Ore {
		public IronOre() : base(TileDefinitions.IronOre) { }
	}
	public class NickelOre { }
	public class CobaltOre : Ore {
		public CobaltOre() : base(TileDefinitions.CobaltOre) { } 
	}
	public class GoldOre : Ore {
		public GoldOre() : base(TileDefinitions.GoldOre) { }
	}
	public class TitaniumOre { }
	public class UraniumOre : Ore, ILightEmitter {
		public Light3 Light => new Light3(4, 8, 4);
		public UraniumOre() : base(TileDefinitions.UraniumOre) { }
	}

#endregion

	public class Clay : Tile 
	{
		public Clay() : base(TileDefinitions.Clay) { }
	}
	public class StoneBrick : Tile, ILocalTileUpdate
	{
		public StoneBrick() : base(TileDefinitions.StoneBrick) {}

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			//sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, y * Globals.TileSize), TileMap.Soil, color.MultiplyAgainst(Color.SaddleBrown));
			var TL = new Rectangle(0, Globals.TileSize, 4, 4);
			var TR = new Rectangle(4, Globals.TileSize, 4, 4);
			var BL = new Rectangle(0, Globals.TileSize+4, 4, 4);
			var BR = new Rectangle(4, Globals.TileSize+4, 4, 4);

			var RTL = new Rectangle(9 * Globals.TileSize, Globals.TileSize, 4, 4);
			var RTR = new Rectangle(9 * Globals.TileSize +4, Globals.TileSize, 4, 4);
			var RBL = new Rectangle(9 * Globals.TileSize, Globals.TileSize + 4, 4, 4);
			var RBR = new Rectangle(9 * Globals.TileSize + 4, Globals.TileSize + 4, 4, 4);

			var BrickRight = new Rectangle(0 + 4, Globals.TileSize, 4, Globals.TileSize);
			var RBrick = new Rectangle(9 * Globals.TileSize, Globals.TileSize, 4, Globals.TileSize);

			var RBrickRight = new Rectangle((9 * Globals.TileSize) + 4, Globals.TileSize, 4, Globals.TileSize);

			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);


			if (TileState.Get(3)) // BottomRight
				sb.Draw(tilesheet, position + new Vector2(4, 4), RBR, color.MultiplyAgainst(Color));
			else
				sb.Draw(tilesheet, position + new Vector2(4, 4), BR, color.MultiplyAgainst(Color));

			if (TileState.Get(2)) // BottomLeft
				sb.Draw(tilesheet, position + new Vector2(0, 4), RBL, color.MultiplyAgainst(Color));
			else
				sb.Draw(tilesheet, position + new Vector2(0, 4), BL, color.MultiplyAgainst(Color));

			if (TileState.Get(1)) // TopLeft
				sb.Draw(tilesheet, position, RTL, color.MultiplyAgainst(Color));
			else
				sb.Draw(tilesheet, position, TL, color.MultiplyAgainst(Color));

			if (TileState.Get(0)) // TopRight
				sb.Draw(tilesheet, position + new Vector2(4, 0), RTR, color.MultiplyAgainst(Color));
			else
				sb.Draw(tilesheet, position + new Vector2(4, 0), TR, color.MultiplyAgainst(Color));


		}

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			bool planetop = IsEmpty(world, x, y - 1);
			bool planebottom = IsEmpty(world, x, y + 1);
			bool planeleft = IsEmpty(world, x - 1, y);
			bool planeright = IsEmpty(world, x + 1, y);

			byte newNumber = TileState;
			newNumber.Set(3, planebottom && planeright);
			newNumber.Set(2, planeleft && planebottom);
			newNumber.Set(1, planeleft && planetop);
			newNumber.Set(0, planeright && planetop);

			if (TileState != newNumber)
			{
				TileState = newNumber;
				world.DoUpdatePropogation(x, y);
			}
		}

	}
	public class ClayBrick: Tile
	{
		public ClayBrick() : base(TileDefinitions.ClayBrick) {}
	}
	public class MudBrick { }
	public class SandBrick { }
	public class Cobblestone { }

#region Logs
	public class OakLog : Tile, INonSolid, ITileUpdate
	{
		
		public OakLog() : base(TileDefinitions.Log) {}

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			var below = world.GetTile(x, y + 1);


			if (!(below is ISoil) && !(below is OakLog))
			{
				world.SetTile(x, y, new Air()); // TODO: Drop Log;
			}
		}
	}
	public class RedwoodLog { }
	public class BirchLog { }
	public class PineLog { }
	public class EbonyLog { }
#endregion
#region Planks
	public class OakPlank : Tile, ILocalTileUpdate {

		public OakPlank() : base(TileDefinitions.OakPlank) { }

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			//sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, y * Globals.TileSize), TileMap.Soil, color.MultiplyAgainst(Color.SaddleBrown));
			var Plank = new Rectangle(2 * Globals.TileSize, 0, 4, Globals.TileSize);
			var PlankRight = new Rectangle((2 * Globals.TileSize) + 4, 0 * Globals.TileSize, 4, Globals.TileSize);
			var RPlank = new Rectangle(8 * Globals.TileSize, 0, 4, Globals.TileSize);

			var RPlankRight = new Rectangle((8 * Globals.TileSize + 4), 0, 4, Globals.TileSize);

			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);

			if (TileState.Get(1)) // cornerd
				sb.Draw(tilesheet, position, RPlank, color.MultiplyAgainst(Color), MathHelper.ToRadians(0), Vector2.Zero, 1, SpriteEffects.None, 1);
			else
				sb.Draw(tilesheet, position, Plank, color.MultiplyAgainst(Color), MathHelper.ToRadians(0), Vector2.Zero, 1, SpriteEffects.None, 1);

			if (TileState.Get(0)) // cornerc
				sb.Draw(tilesheet, position+new Vector2(4,0), RPlankRight, color.MultiplyAgainst(Color), MathHelper.ToRadians(0), Vector2.Zero, 1, SpriteEffects.None, 1);
			else
				sb.Draw(tilesheet, position + new Vector2(4, 0), PlankRight, color.MultiplyAgainst(Color), MathHelper.ToRadians(0), Vector2.Zero, 1, SpriteEffects.None, 1);


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
	public class RedwoodPlank { }
	public class PinePlank { }
	public class EbonyPlank { }
#endregion
	public class Leaves : Tile, INonSolid, IRandomTick
	{
		
		
		public Leaves() : base(TileDefinitions.Leaves)
		{

		}

		public void RandomTick(IGameWorld world, int x, int y)
		{
			
		}

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{

			base.Draw(tilesheet, sb, x, y, color);
		}
	}
	public class PineNeedles { }
	public abstract class BaseTorch : Tile , INonSolid, ITileUpdate, IWaterBreakable
	{
		public static Rectangle[] AnimStates =
		{
			new Rectangle(Globals.TileSize*11, Globals.TileSize*5, Globals.TileSize, Globals.TileSize),
			new Rectangle(Globals.TileSize*12, Globals.TileSize*5, Globals.TileSize, Globals.TileSize),
			new Rectangle(Globals.TileSize*13, Globals.TileSize*5, Globals.TileSize, Globals.TileSize),
			new Rectangle(Globals.TileSize*14, Globals.TileSize*5, Globals.TileSize, Globals.TileSize),
		};

		public BaseTorch(TDef def) : base(def) { }

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			Vector2 position = new Vector2(Globals.TileSize * x, Globals.TileSize * y);
			sb.Draw(tilesheet, position, TileMap.Torch, color.MultiplyAgainst(Color));
		}


		public void TileUpdate(IGameWorld world, int x, int y)
		{


			//if (TileState == 0)
			//{
			if ((world.GetTile(x, y + 1) is INonSolid))
			{
				world.SetTile(x, y, new Air()); // TODO: Drop Tile;
			}
			//}
		}


	}
	public class Torch: BaseTorch, ILightEmitter
	{

		public Light3 Light => new Light3(24, 24, 16);
		public Torch() : base(TileDefinitions.Torch) {}
	}
	public class YellowTorch: BaseTorch, ILightEmitter {
		public Light3 Light => new Light3(24, 24, 0);
		public YellowTorch() : base(TileDefinitions.YellowTorch) { }
	}
	public class WhiteTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(24, 24, 24);
		public WhiteTorch() : base(TileDefinitions.WhiteTorch) { }
	}
	public class GreenTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(0, 32, 0);
		public GreenTorch() : base(TileDefinitions.GreenTorch) { }
	}
	public class RedTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(32, 0, 0);
		public RedTorch() : base(TileDefinitions.RedTorch) { }
	}
	public class BlueTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(0, 0, 32);
		public BlueTorch() : base(TileDefinitions.BlueTorch) { }
	}
	public class Cactus { }
	public class CactusFlower { }
	public class Vine { }
	public class MossyStone { }
	public class MossyBrick { }
	public class Tallgrass { }
	public class CryingLily { }
	public class HexenRose { }
	public class Bamboo { }
	public class Sugarcane { }
	public class WoodMushroom { }
	public class PinkOysterMushroom { }
	public class CubensisMushroom { }
	public class ChestnutMushroom { }
	public class Tulip { }
	public class Cornflower { }
	public class Begonia { }
	public class BleedingHeart { }
	public class EnglishBluebell { }
	public class Poppy { }
	public class TNT { }
	public class Switch { }
	public class ANDGate { }
	public class ORGate { }
	public class XORGate { }
	public class Diode { }
	public class NORGate { }
	public class XANDGate { }
	public class Delay { }
	public class Pump { }
	public class Pipe { }
	public class Trapdoor { }
}
