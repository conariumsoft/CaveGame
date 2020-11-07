#if CLIENT
using CaveGame.Client;
using CaveGame.Core.Game.Entities;
#endif
using CaveGame.Core.Walls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CaveGame.Core.Tiles
{
	// TODO: Fix blue oyster mushrooms

	public enum TileID : byte
	{
		Void = 255,
		Air = 0,
		Stone,Dirt,Grass,OakLog,Leaves,Water,StoneBrick,ClayBrick,Torch,Clay,
		OakPlank,EbonyPlank,PinePlank,RedwoodPlank,
		Granite,Sandstone,Mud,Sand,
		RedTorch,BlueTorch,GreenTorch,YellowTorch,WhiteTorch,

		CopperOre,IronOre,GoldOre,UraniumOre,CobaltOre,LeadOre, 

		MudBrick, SandBrick, IceBrick, CarvedStoneBrick,
		CarvedSandBrick, MossyStoneBrick, MossyStone,
		CubedStone, CubedSandstone,
		Cobweb, Tallgrass, Rope, Vine, Ladder, Platform, TNT, Lava, Sludge, Obsidian,
		Snow, Ice, Mycelium, CryingLily, BlueMushroom, Poppy, BlueTallgrass, BlueVine,
		ArsenicOre, GalliumOre, 
		BlackEyedSusan, Chrysanthemum, ForgetMeNot, Honeysuckle, Hydrangea, Lily,
		Magnolia,Orchid,Plumeria,Tulip, HexenRoses
		//GermanOccupiedTerritory,
		//FurniturePointer = 254
	}
	public class TDef : ILightEmitter // TileData
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
		public static TDef Void = new TDef { };
		public static TDef Air = new TDef { Hardness = 0, Opacity = 1, Quad = TileMap.Default };
		public static TDef Dirt = new TDef { Hardness = 2, Quad = TileMap.Soil, Color = Color.SaddleBrown };
		public static TDef Water = new TDef { Hardness = 0, Opacity = 2,Quad = TileMap.Default, Color = Color.Blue };
		public static TDef Lava = new TDef
		{
			Hardness = 0,
			Opacity = 3,
			Quad = TileMap.Default,
			Color = Color.Red
		};
		public static TDef Sludge = new TDef
		{
			Hardness = 0,
			Opacity = 2,
			Quad = TileMap.Default,
			Color = Color.Green
		};
		public static TDef Grass = new TDef { Hardness = 2, Quad = TileMap.Soil, Color = Color.Green };
		public static TDef Stone = new TDef { Hardness = 4,  Quad = TileMap.Stone, Color = new Color(0.7f, 0.7f, 0.7f) };
		public static TDef StoneBrick = new TDef { Hardness = 6,  Quad = TileMap.Brick, Color = new Color(0.7f, 0.7f, 0.7f) };
		public static TDef MudBrick = new TDef {
			Hardness = 12,
			Quad = TileMap.Brick,
			Color = new Color(0.3f, 0.1f, 0.1f)
		};
		public static TDef SandBrick = new TDef {
			Hardness = 12,
			Quad = TileMap.Brick,
			Color = new Color(0.8f, 0.8f, 0.3f)
		};

		public static TDef ClayBrick = new TDef { Hardness = 6, Quad = TileMap.RedBrick, Color = Color.White };
		public static TDef OakLog = new TDef { Hardness = 3, Opacity = 1, Quad = TileMap.Log, Color = new Color(0.6f, 0.3f, 0.2f) };
		public static TDef Leaves = new TDef { Hardness = 1, Opacity = 0, Quad = TileMap.Leaves, Color = new Color(0.1f, 0.9f, 0.1f) };
		public static TDef Torch = new TDef { Hardness = 1, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.8f, 0.8f, 0.4f) };
		public static TDef WhiteTorch = new TDef { Hardness = 1, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.8f, 0.8f, 0.8f) };
		public static TDef YellowTorch = new TDef { Hardness = 1, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.9f, 0.9f, 0.2f) };
		public static TDef RedTorch = new TDef { Hardness = 1, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.9f, 0.2f, 0.2f) };
		public static TDef BlueTorch = new TDef { Hardness = 1, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.2f, 0.2f, 0.9f) };
		public static TDef GreenTorch = new TDef { Hardness = 1, Opacity = 0, Quad = TileMap.Torch, Color = new Color(0.2f, 0.9f, 0.2f) };

		public static TDef Mud = new TDef { Hardness = 2,  Quad = TileMap.Soil, Color = new Color(0.3f, 0.1f, 0.1f) };
		public static TDef Sand = new TDef { Hardness = 2,  Quad = TileMap.Soil, Color = new Color(245, 211, 125) };
		public static TDef Granite = new TDef { Hardness = 5,  Quad = TileMap.Stone, Color = new Color(1.5f, 1.5f, 1.5f) };
		public static TDef Sandstone = new TDef { Hardness = 5,  Quad = TileMap.Stone, Color = new Color(1.0f, 1.0f, 0.3f) };
		public static TDef Clay = new TDef { Hardness = 3,  Quad = TileMap.Soil, Color = new Color(0.6f, 0.2f, 0.2f) };


		public static TDef OakPlank = new TDef { 
			Hardness = 3,  
			Quad = TileMap.Plank, 
			Color = new Color(0.8f, 0.55f, 0.25f)
		};
		public static TDef RedwoodPlank = new TDef
		{
			Hardness = 3,
			Quad = TileMap.Plank,
			Color = new Color(163, 82, 65)
		};
		public static TDef EbonyPlank = new TDef
		{
			Hardness = 3,
			Quad = TileMap.Plank,
			Color = new Color(74, 74, 74)
		};
		public static TDef PinePlank = new TDef
		{
			Hardness = 3,
			Quad = TileMap.Plank,
			Color = new Color(148, 109, 92)
		};


		public static TDef CopperOre = new TDef { Hardness = 12, Quad = TileMap.Ore, Color = new Color(1.0f, 0.45f, 0f) };
		public static TDef LeadOre = new TDef { Hardness = 12,  Quad = TileMap.Ore, Color = new Color(0.35f, 0.35f, 0.4f) };
		public static TDef TinOre = new TDef { Hardness = 12,  Quad = TileMap.Ore, Color = new Color(0.65f, 0.4f, 0.4f) };
		public static TDef IronOre = new TDef { Hardness = 12,  Quad = TileMap.Ore, Color = new Color(1.0f, 0.75f, 0.75f) };
		public static TDef CobaltOre = new TDef { Hardness = 12, Quad = TileMap.Ore, Color = new Color(0.3f, 0.3f, 1f) };
		public static TDef GoldOre = new TDef { Hardness = 12, Quad = TileMap.Ore, Color = new Color(1f, 1f, 0.5f) };
		public static TDef UraniumOre = new TDef { Hardness = 12, Quad = TileMap.Ore, Color = new Color(0.35f, 0.8f, 0.35f) };
		public static TDef IceBrick = new TDef
		{
			Hardness = 12,
			Quad = TileMap.Brick,
			Color = new Color(0.8f, 0.8f, 1.0f),
		};
		public static TDef CarvedStoneBrick = new TDef
		{
			Hardness = 12,
			Quad = TileMap.Carved,
			Color = new Color(0.7f, 0.7f, 0.7f)
		};
		public static TDef CarvedSandBrick = new TDef
		{
			Hardness = 12,
			Quad = TileMap.Carved,
			Color = new Color(0.8f, 0.8f, 0.0f)
		};
		public static TDef MossyStoneBrick = new TDef
		{
			Hardness = 8,
			Quad = TileMap.MossyBrick,
			Color = new Color(0.7f, 0.7f, 0.7f)
		};
		public static TDef MossyStone = new TDef
		{
			Hardness = 6,
			Quad = TileMap.StoneMossy,
			Color = new Color(0.7f, 0.7f, 0.7f)
		};
		public static TDef CubedStone = new TDef
		{
			Color = new Color(0.7f, 0.7f, 0.7f),
			Quad = TileMap.StoneCubes,
			Hardness = 16,
		};
		public static TDef CubedSandstone = new TDef
		{
			Color = new Color(0.7f, 0.7f, 0.0f),
			Quad = TileMap.StoneCubes,
			Hardness = 16,
		};
		public static TDef Cobweb = new TDef
		{
			Hardness = 1,
			Opacity = 3,
			Quad = TileMap.Cobweb
		};
		public static TDef Tallgrass = new TDef
		{
			Hardness = 1,
			Opacity = 1,
			Quad = TileMap.TallGrass,
			Color = Color.Green,
		};
		public static TDef BlueTallgrass = new TDef
		{
			Hardness = 1,
			Opacity = 1,
			Quad = TileMap.TallGrass,
			Color = Color.Blue,
		};
		public static TDef Rope = new TDef
		{
			Hardness = 2,
			Opacity = 1,
			Quad = TileMap.Rope
		};
		public static TDef Ladder = new TDef
		{
			Quad = TileMap.Ladder,
			Hardness = 4,
			Opacity = 2,
			Color = new Color(0.8f, 0.5f, 0.3f)
		};
		public static TDef Vine = new TDef
		{
			Quad = TileMap.Vine,
			Hardness = 1,
			Opacity = 2,
			Color = Color.Green,
		};
		public static TDef BlueVine = new TDef
		{
			Quad = TileMap.Vine,
			Hardness = 1,
			Opacity = 2,
			Color =  new Color(0, 0, 2.5f),
		};
		public static TDef Platform = new TDef
		{
			Quad = TileMap.Platform,
			Hardness = 2,
			Opacity = 1,
			Color = new Color(0.8f, 0.5f, 0.3f)
		};
		public static TDef TNT = new TDef
		{
			Quad = TileMap.TNT,
			Color = Color.White,
			Hardness = 1
		};
		public static TDef Obsidian = new TDef
		{
			Color = new Color(0.5f, 0.1f, 0.5f),
			Quad = TileMap.StoneLight,
			Hardness = 20,
		};

		public static TDef Ice = new TDef
		{
			Quad = TileMap.Soil,
			Color = new Color(0.9f, 0.9f, 0.9f),
			Hardness = 6,
		};
		public static TDef Snow = new TDef
		{
			Quad = TileMap.Soil,
			Color = new Color(0.9f, 0.9f, 0.9f),
			Hardness = 3,

		};

		public static TDef SovietOccupiedTerritory = new TDef
		{

		};
		public static TDef Mycelium = new TDef
		{
			Opacity = 2,
			Hardness = 2,
			Quad = TileMap.Soil,
			Color = new Color(0.1f, 0.1f, 2.5f)
		};

		public static TDef BlueMushroom = new TDef
		{
			Opacity = 0,
			Hardness = 1,
			Quad = TileMap.BlueMushroom,
			Color = Color.White
		};
		public static TDef CryingLily = new TDef
		{
			Opacity = 0,
			Hardness = 1,
			Quad = TileMap.CryingLilyTop,
			Color = Color.White
		};
		public static TDef Poppy = new TDef
		{
			Opacity = 0,
			Hardness = 1,
			Quad = TileMap.Poppy,
			Color = Color.White
		};

		public static TDef FurniturePointer = new TDef
		{
			Opacity = 0,
			Hardness = 0,
			Quad = TileMap.Default,
			Color = Color.White,
		};

		public static TDef Tulip = new TDef{Opacity = 1,Hardness = 1,Quad = TileMap.Tulip,Color = Color.White};
		public static TDef Plumeria = new TDef { Opacity = 1, Hardness = 1, Quad = TileMap.Plumeria, Color = Color.White };
		public static TDef Orchid = new TDef { Opacity = 1, Hardness = 1, Quad = TileMap.Orchid, Color = Color.White };
		public static TDef Magnolia = new TDef { Opacity = 1, Hardness = 1, Quad = TileMap.Magnolia, Color = Color.White };

		public static TDef Lily = new TDef { Opacity = 1, Hardness = 1, Quad = TileMap.Lily, Color = Color.White };
		public static TDef Hydrangea = new TDef { Opacity = 1, Hardness = 1, Quad = TileMap.Hydrangea, Color = Color.White };
		public static TDef Honeysuckle = new TDef { Opacity = 1, Hardness = 1, Quad = TileMap.Honeysuckle, Color = Color.White };
		public static TDef ForgetMeNot = new TDef { Opacity = 1, Hardness = 1, Quad = TileMap.ForgetMeNot, Color = Color.White };
		public static TDef Chrysanthemum = new TDef { Opacity = 1, Hardness = 1, Quad = TileMap.Chrysanthemum, Color = Color.White };
		public static TDef BlackEyedSusan = new TDef { Opacity = 1, Hardness = 1, Quad = TileMap.BlackEyedSusan, Color = Color.White };
		public static TDef HexenRoses = new TDef { Opacity = 1, Hardness = 1, Quad = TileMap.Flowers, Color = Color.White };
	}
	#region stuff
	public interface IWaterBreakable { }
	public interface INonSolid { }

	// Property interfaces
	public interface INonStandardCollision
	{
		bool CollisionCheck(IGameWorld world);
		void OnCollide();
	}
	
	public interface ILightEmitter
	{
		Light3 Light { get; }
	}
	// Method interfaces

	public interface ILocalTileUpdate
	{
		void LocalTileUpdate(IGameWorld world, int x, int y);
	}
	public interface IRandomTick
	{
		void RandomTick(IGameWorld world, int x, int y);
	}
	public interface ITileUpdate
	{
		void TileUpdate(IGameWorld world, int x, int y);
	}
	public interface ISoil { }
	public interface IGas { }
	public interface ILiquid {
		float Viscosity { get; }
	}
	public interface IVegetation { }
	public interface IOre { }
	public interface IMineral { }
	public interface IPlatformTile { }
	#endregion
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
				bool exists = Enum.TryParse(type.Name, out TileID id);
				if (exists && id == (TileID)t)
					return (Tile)type.GetConstructor(Type.EmptyTypes).Invoke(null);
			}
			throw new Exception("ID not valid!");
		}

		public static Tile FromName(string name)
		{
			var basetype = typeof(Tile);
			var types = basetype.Assembly.GetTypes().Where(type => type.IsSubclassOf(basetype));


			foreach (var type in types)
			{
				if (name == type.Name)
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

		public static Tile Deserialize(ref byte[] datagram, int pullIndex)
		{
			Tile t = FromID(datagram[pullIndex]);
			t.Damage = datagram[pullIndex + 1];
			t.TileState = datagram[pullIndex + 2];

			return t;
		}


		public virtual void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			sb.Draw(
				tilesheet,
				new Vector2(x * Globals.TileSize, y * Globals.TileSize),
				Quad, color.MultiplyAgainst(Color), 0,
				Vector2.Zero, 1, SpriteEffects.None, 0
			);
		}

		public bool Equals(Tile other)
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

	public class FurniturePointer : Tile
	{
		public FurniturePointer() : base(TileDefinitions.FurniturePointer) { }
		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 light) {
			
		} // leave empty
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
	

	
	public class Dirt : Tile, ISoil
	{
		public Dirt() : base(TileDefinitions.Dirt) { }
	}
	public class Mud : Tile, ISoil
	{
		public Mud() : base(TileDefinitions.Mud) { }
	}
	public class Sand : Tile
	{
		public Sand() : base(TileDefinitions.Sand) { }
	}
	public class Snow : Tile
	{
		public Snow() : base(TileDefinitions.Snow) { }
	}
	public class Clay : Tile
	{
		public Clay() : base(TileDefinitions.Clay) { }
	}
#endregion
#region Liquids

	public abstract class Liquid : Tile, ILiquid, INonSolid
	{
		public Liquid(TDef def) : base(def) { }

		public abstract float Viscosity { get;}

		private bool EvaporationCheck(IGameWorld world, int x, int y)
		{
			if (TileState < 1 && RNG.NextDouble() > 0.1)
			{
				world.SetTileNoLight(x, y, new Air());
				return true;
			}
			return false;
		}

		protected virtual void FlowBelow<TLiquid>(IGameWorld world, int x, int y) where TLiquid : Liquid, new()
		{
			var below = world.GetTile(x, y + 1);

			if (below is IWaterBreakable s)
			{

				world.SetTileNoLight(x, y + 1, new TLiquid { TileState = this.TileState });
				world.SetTileNoLight(x, y, new Air());
				TileState = 0;
			}

			if (below is TLiquid wbelow)
			{
				var most = 8 - below.TileState;

				byte taken = (byte)Math.Min(most, TileState);
				TileState -= taken;
				below.TileState += taken;
				world.DoUpdatePropogation(x, y);
				world.DoUpdatePropogation(x, y + 1);
			}
		}

		public void LiquidUpdate<TLiquid>(IGameWorld world, int x, int y) where TLiquid : Liquid, new()
		{
			EvaporationCheck(world, x, y);

			FlowBelow<TLiquid>(world, x, y);


			var left = world.GetTile(x - 1, y);
			var right = world.GetTile(x + 1, y);

			if (left is IWaterBreakable && right is IWaterBreakable && TileState >= 2)
			{
				if (TileState % 2 == 0)
				{
					world.SetTileNoLight(x - 1, y, new TLiquid { TileState = 1 });
					world.SetTileNoLight(x + 1, y, new TLiquid { TileState = 1 });
					TileState -= 2;
				}
			} else if (left is IWaterBreakable && TileState > 0)
			{

				
				world.SetTileNoLight(x - 1, y, new TLiquid { TileState = 1 });
				world.DoUpdatePropogation(x, y);
				TileState--;
			}else if (right is IWaterBreakable && TileState > 0)
			{
				
				world.SetTileNoLight(x + 1, y, new TLiquid { TileState = 1 });
				world.DoUpdatePropogation(x, y);
				TileState--;

			}

			if (left is TLiquid wleft && TileState > wleft.TileState)
			{
				if (TileState > wleft.TileState + 1)
				{
					TileState--;
					wleft.TileState++;
				}
				else if (TileState - wleft.TileState == 1)
				{
					if (RNG.NextDouble() > 0.5)

						wleft.TileState = TileState;
					else
						TileState = wleft.TileState;
				}
				if (TileState > wleft.TileState + 1)
				{
					TileState--;
					wleft.TileState++;
				}
				else if (TileState - wleft.TileState == 1)
				{
					if (RNG.NextDouble() > 0.5)

						wleft.TileState = TileState;
					else
						TileState = wleft.TileState;
				}
				world.DoUpdatePropogation(x, y);
				world.DoUpdatePropogation(x - 1, y);

				//world.SetTileNetworkUpdated(x, y);
				//world.SetTileNetworkUpdated(x - 1, y);
			}

			if (right is TLiquid wright && TileState > wright.TileState)
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
				//world.SetTileNetworkUpdated(x, y);
				//world.SetTileNetworkUpdated(x + 1, y);
			}
			EvaporationCheck(world, x, y);
		}
	}

	public class Lava: Liquid, ITileUpdate, ILocalTileUpdate, ILightEmitter
	{
		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 light)
		{
			var rect = new Rectangle(0, 15 * Globals.TileSize, Globals.TileSize, TileState);
			sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, (y * Globals.TileSize) + (8 - TileState)), rect, light.MultiplyAgainst(Color.Red));
		}

		public override float Viscosity => 1.05f;
		public Lava() : base(TileDefinitions.Lava) { TileState = 8; }

		public Light3 Light => new Light3(32, 4, 4);

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			//LiquidUpdate<Lava>(world, x, y);
		}

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			var below = world.GetTile(x, y+1);

			if (below is Water _)
			{
				world.SetTile(x, y+1, new Obsidian());
				TileState--;
			}
			var left = world.GetTile(x-1, y);

			if (left is Water _)
			{
				world.SetTile(x-1, y, new Obsidian());
				TileState--;
			}
			var right = world.GetTile(x+1, y);

			if (right is Water _)
			{
				world.SetTile(x+1, y, new Obsidian());
				TileState--;
			}
			var top = world.GetTile(x, y-1);

			if (top is Water _)
			{
				world.SetTile(x, y-1, new Obsidian());
				TileState--;
			}

			LiquidUpdate<Lava>(world, x, y);

			

		}
	}

	public class Water : Liquid, ILiquid, ITileUpdate, ILocalTileUpdate
	{
		public override float Viscosity => 1.02f;
		public Water() : base(TileDefinitions.Water) { TileState = 8; }

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 light)
		{

			float brug = Math.Max(0.5f, (1 - (light.Blue/16.0f))*2.0f);

			var rect = new Rectangle(0, 15 * Globals.TileSize, Globals.TileSize, TileState);
			sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, (y * Globals.TileSize) + (8 - TileState)), rect, light.MultiplyAgainst(Color.Blue) * brug);



		}

		public void TileUpdate(IGameWorld world, int x, int y) {
			//LiquidUpdate<Water>(world, x, y);
		}

		public void LocalTileUpdate(IGameWorld world, int x, int y) {
			LiquidUpdate<Water>(world, x, y);
		}
	}
	public class Sludge : Liquid, ILightEmitter, ILocalTileUpdate
	{
		public override float Viscosity => 2.0f;
		public Sludge() : base(TileDefinitions.Sludge) { TileState = 8; }
		public Light3 Light => new Light3(14, 32, 8);

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 light)
		{

			var rect = new Rectangle(0, 15 * Globals.TileSize, Globals.TileSize, TileState);
			sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, (y * Globals.TileSize) + (8 - TileState)), rect, light.MultiplyAgainst(new Color(0.6f, 0.9f, 0.3f)));
		}

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			LiquidUpdate<Sludge>(world, x, y);
		}
	}
	public class Ectoplasm { }
	public class Oil { }
	public class LiquidOxygen { }
	public class LiquidNitrogen { }
#endregion
#region Misc
	public class Cobweb : Tile {
		public Cobweb() : base(TileDefinitions.Cobweb) { }
	}
	public class Ladder : Tile {
		public Ladder() : base(TileDefinitions.Ladder) { }
	}
	public class Platform : Tile, IPlatformTile {
		public Platform() : base(TileDefinitions.Platform) { }
	}
	public class Rope : Tile, ITileUpdate, INonSolid {
		public Rope() : base(TileDefinitions.Rope) {
		
		}


		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (world.IsTile<INonSolid>(x, y - 1) && !world.IsTile<Rope>(x, y - 1))
				world.SetTile(x, y, new Air());
		}
	}
	public class Vine : Tile, IRandomTick, INonSolid, ITileUpdate, IWaterBreakable
	{ 
		public Vine() : base(TileDefinitions.Vine) { }

		public void RandomTick(IGameWorld world, int x, int y)
		{
			var below = world.GetTile(x, y + 1);
			if (below is Air)
			{
				world.SetTile(x, y + 1, new Vine());
			}
		}

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			Vector2 position = new Vector2((x * Globals.TileSize)+(x.Mod(4)), (y * Globals.TileSize));

			sb.Draw(tilesheet, position, Quad, color.MultiplyAgainst(Color));
			//base.Draw(tilesheet, sb, x, y, color);
		}

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			var above = world.GetTile(x, y - 1);
			if (!(above is Vine || above is Grass || above is Mycelium))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class Tallgrass : Tile, INonSolid, ITileUpdate, IWaterBreakable, IRandomTick
	{
		public Tallgrass() : base(TileDefinitions.Tallgrass)
		{
			
		}

		public void RandomTick(IGameWorld world, int x, int y)
		{
			if (RNG.NextDouble()>0.95f)
			{
				world.SetTile(x, y, new Air()); // grass sometimes randomly dies
			}
		}

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);
			Rectangle quad = TileMap.TallGrass;

			int state = ((int)x).Mod(3);

			if (state == 2)
				quad = TileMap.TallGrass2;
			if (state == 1)
				quad = TileMap.TallGrass3;

			sb.Draw(tilesheet, position, quad, color.MultiplyAgainst(Color));
			//base.Draw(tilesheet, sb, x, y, color);
		}

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			TileState++;
			var below = world.GetTile(x, y + 1);
			if (!(below is Grass))
			{
				world.SetTile(x, y, new Air());
			}
		}


	}
	public class BlueTallgrass : Tile, INonSolid, ITileUpdate, IWaterBreakable, IRandomTick
	{
		public BlueTallgrass() : base(TileDefinitions.BlueTallgrass)
		{

		}

		public void RandomTick(IGameWorld world, int x, int y)
		{
			if (RNG.NextDouble() > 0.95f)
			{
				world.SetTile(x, y, new Air()); // grass sometimes randomly dies
			}
		}

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			Vector2 position = new Vector2(x * Globals.TileSize, y * Globals.TileSize);
			Rectangle quad = TileMap.TallGrass;

			int state = ((int)x).Mod(3);

			if (state == 2)
				quad = TileMap.TallGrass2;
			if (state == 1)
				quad = TileMap.TallGrass3;

			sb.Draw(tilesheet, position, quad, color.MultiplyAgainst(Color));
			//base.Draw(tilesheet, sb, x, y, color);
		}

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			TileState++;
			var below = world.GetTile(x, y + 1);
			if (!(below is Mycelium))
			{
				world.SetTile(x, y, new Air());
			}
		}


	}

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
	public class Glass : Tile, ILocalTileUpdate {
		public Glass() : base(new TDef { Opacity = 0, Hardness = 3, Color = Color.White, Quad = TileMap.Glass}) { }

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			throw new NotImplementedException();
		}
	}
	public class Limestone { }
	public class Obsidian : Tile {
		public Obsidian() : base(TileDefinitions.Obsidian)
		{

		}
	}
	public class Brimstone { }
	public class Ice : Tile {
		public Ice() : base(TileDefinitions.Ice) { }
	}
	public class IceBrick : Brick {
		public IceBrick() : base(TileDefinitions.IceBrick) { }
	}
	public class MossyStone : Tile {
		public MossyStone() : base(TileDefinitions.MossyStone) {}
	}
	public class MossyStoneBrick : Tile {
		public MossyStoneBrick() : base(TileDefinitions.MossyStoneBrick) { }
	}
	public class CarvedStoneBrick : Tile {
		public CarvedStoneBrick() : base(TileDefinitions.CarvedStoneBrick) { }
	}
	public class CarvedSandBrick : Tile {
		public CarvedSandBrick() : base(TileDefinitions.CarvedSandBrick) { }
	}
	public class CubedStone : Tile {
		public CubedStone() : base(TileDefinitions.CubedStone) { }
	}
	public class CubedSandstone : Tile { 
		public CubedSandstone() : base(TileDefinitions.CubedSandstone) { }
	}
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
	public class Arsenic { }
	public class Gallium { }
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

	
	public class Brick : Tile, ILocalTileUpdate
	{
		public Brick(TDef def) :  base(def) { }
		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			//sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, y * Globals.TileSize), TileMap.Soil, color.MultiplyAgainst(Color.SaddleBrown));
			var TL = new Rectangle(0, Globals.TileSize, 4, 4);
			var TR = new Rectangle(4, Globals.TileSize, 4, 4);
			var BL = new Rectangle(0, Globals.TileSize + 4, 4, 4);
			var BR = new Rectangle(4, Globals.TileSize + 4, 4, 4);

			var RTL = new Rectangle(9 * Globals.TileSize, Globals.TileSize, 4, 4);
			var RTR = new Rectangle(9 * Globals.TileSize + 4, Globals.TileSize, 4, 4);
			var RBL = new Rectangle(9 * Globals.TileSize, Globals.TileSize + 4, 4, 4);
			var RBR = new Rectangle(9 * Globals.TileSize + 4, Globals.TileSize + 4, 4, 4);

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
	public class StoneBrick : Brick
	{
		public StoneBrick() : base(TileDefinitions.StoneBrick) {}
	}
	public class ClayBrick: Brick
	{
		public ClayBrick() : base(TileDefinitions.ClayBrick) {}

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			//sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, y * Globals.TileSize), TileMap.Soil, color.MultiplyAgainst(Color.SaddleBrown));
			var TL = new Rectangle(0, (Globals.TileSize*3), 4, 4);
			var TR = new Rectangle(4, (Globals.TileSize*3), 4, 4);
			var BL = new Rectangle(0, (Globals.TileSize*3) + 4, 4, 4);
			var BR = new Rectangle(4, (Globals.TileSize*3) + 4, 4, 4);

			var RTL = new Rectangle(10 * Globals.TileSize, Globals.TileSize, 4, 4);
			var RTR = new Rectangle(10 * Globals.TileSize + 4, Globals.TileSize, 4, 4);
			var RBL = new Rectangle(10 * Globals.TileSize, Globals.TileSize + 4, 4, 4);
			var RBR = new Rectangle(10 * Globals.TileSize + 4, Globals.TileSize + 4, 4, 4);

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
	}
	public class MudBrick : Brick {

		public MudBrick() : base(TileDefinitions.MudBrick) { }
	}
	public class SandBrick : Brick {
		public SandBrick() : base(TileDefinitions.SandBrick) { }
	}
	public class Cobblestone { }

#region Logs
	public class OakLog : Tile, INonSolid, ITileUpdate
	{
		
		public OakLog() : base(TileDefinitions.OakLog) {}

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

	public abstract class Plank : Tile, ILocalTileUpdate
	{
		public Plank(TDef def) : base(def) { }

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
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
				sb.Draw(tilesheet, position + new Vector2(4, 0), RPlankRight, color.MultiplyAgainst(Color), MathHelper.ToRadians(0), Vector2.Zero, 1, SpriteEffects.None, 1);
			else
				sb.Draw(tilesheet, position + new Vector2(4, 0), PlankRight, color.MultiplyAgainst(Color), MathHelper.ToRadians(0), Vector2.Zero, 1, SpriteEffects.None, 1);

#if EDITOR
			base.Draw(tilesheet, sb, x, y, color);
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

	public class OakPlank : Plank {
		public OakPlank() : base(TileDefinitions.OakPlank) {}
	}
	public class RedwoodPlank: Plank { 
		public RedwoodPlank() : base(TileDefinitions.RedwoodPlank) { }
	}
	public class PinePlank: Plank { 
		public PinePlank() : base(TileDefinitions.PinePlank) { }
	}
	public class EbonyPlank: Plank {
		public EbonyPlank() : base(TileDefinitions.EbonyPlank) { }
	}
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
			// States:
			// 1 - SupportedBelow
			// 2 - SupportedLeft
			// 3 - SupportedRight
			// 4 - SupportedBehind


			bool supportedBelow = (world.GetTile(x, y + 1) is INonSolid);

			bool supportedLeft = (world.GetTile(x - 1, y) is INonSolid);

			bool supportedRight = (world.GetTile(x + 1, y) is INonSolid);
			bool supportedWall = !(world.GetWall(x, y) is INonSolid);

			if (TileState == 0)
			{
				if (supportedBelow)
					TileState = 1;
				if (supportedLeft)
					TileState = 2;
				if (supportedRight)
					TileState = 3;
				if (supportedWall)
					TileState = 4;
			}

			if (TileState == 0)
			{
				world.SetTile(x, y, new Air()); // TODO: Prevent Placement
			}

			if (TileState == 1 && !supportedBelow)
				world.SetTile(x, y, new Air()); // TODO: Drop Tile;

			if (TileState == 2 && !supportedLeft)
				world.SetTile(x, y, new Air()); // TODO: Drop Tile;

			if (TileState == 3 && !supportedRight)
				world.SetTile(x, y, new Air()); // TODO: Drop Tile;

			if (TileState == 4 && !supportedWall)
				world.SetTile(x, y, new Air()); // TODO: Drop Tile;
			
		}


	}
	public class Torch: BaseTorch, ILightEmitter
	{

		public Light3 Light => new Light3(20, 20, 12);
		public Torch() : base(TileDefinitions.Torch) {}
	}
	public class YellowTorch: BaseTorch, ILightEmitter {
		public Light3 Light => new Light3(20, 20, 0);
		public YellowTorch() : base(TileDefinitions.YellowTorch) { }
	}
	public class WhiteTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(20, 20, 20);
		public WhiteTorch() : base(TileDefinitions.WhiteTorch) { }
	}
	public class GreenTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(0, 20, 0);
		public GreenTorch() : base(TileDefinitions.GreenTorch) { }
	}
	public class RedTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(20, 0, 0);
		public RedTorch() : base(TileDefinitions.RedTorch) { }
	}
	public class BlueTorch : BaseTorch, ILightEmitter
	{
		public Light3 Light => new Light3(0, 0, 20);
		public BlueTorch() : base(TileDefinitions.BlueTorch) { }
	}
	public class BlueMushroom : Tile, ILightEmitter, INonSolid, ITileUpdate, IWaterBreakable
	{
		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is Mycelium))
				world.SetTile(x, y, new Air());
		}

		public Light3 Light => new Light3(2, 3, 3);
		public BlueMushroom() : base(TileDefinitions.BlueMushroom) { }

	}
	public class Cactus { }
	public class CactusFlower { }
	public class CryingLily : Tile, ILightEmitter, INonSolid, ITileUpdate, IWaterBreakable {
		public Light3 Light => new Light3(2, 4, 6);

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y - 1) is Mycelium))
				world.SetTile(x, y, new Air());

			
		}

		public CryingLily() : base(TileDefinitions.CryingLily) { }
		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			SpriteEffects effects = SpriteEffects.None;
			Vector2 position = new Vector2((Globals.TileSize * x) + (x%3)-(y%6), (Globals.TileSize * y));

			if ((x+y).Mod(2)==0)
			{
				effects = SpriteEffects.FlipHorizontally;
			}
			sb.Draw(tilesheet, position, TileMap.CryingLily, color.MultiplyAgainst(Color), 0, Vector2.Zero, Vector2.One, effects, 0);
			//base.Draw(tilesheet, sb, x, y, color);
		}
	}
	public class HexenRoses : Tile, INonSolid, ITileUpdate, IWaterBreakable {

		public HexenRoses() : base(TileDefinitions.HexenRoses) { }

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class Bamboo { }
	public class Sugarcane { }
	public class WoodMushroom { }
	public class PinkOysterMushroom { }
	public class CubensisMushroom { }
	public class ChestnutMushroom { }



	public class BlackEyedSusan : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public BlackEyedSusan() : base(TileDefinitions.BlackEyedSusan) { }

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}

	public class Chrysanthemum : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public Chrysanthemum() : base(TileDefinitions.Chrysanthemum) { }

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class ForgetMeNot : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public ForgetMeNot() : base(TileDefinitions.ForgetMeNot) { }

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class Honeysuckle : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public Honeysuckle() : base(TileDefinitions.Honeysuckle) { }

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class Hydrangea : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public Hydrangea() : base(TileDefinitions.Hydrangea) { }

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class Lily : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public Lily() : base(TileDefinitions.Lily) { }

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class Magnolia : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public Magnolia() : base(TileDefinitions.Magnolia) { }

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class Orchid : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public Orchid() : base(TileDefinitions.Orchid) { }

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class Plumeria : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public Plumeria() : base(TileDefinitions.Plumeria) { }

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class Tulip : Tile, INonSolid, ITileUpdate, IWaterBreakable 
	{
		public Tulip() : base(TileDefinitions.Tulip) { }

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (!(world.GetTile(x, y + 1) is ISoil))
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class Cornflower { }
	public class Begonia { }
	public class BleedingHeart { }
	public class EnglishBluebell { }
	public class Poppy : Tile, INonSolid, ITileUpdate, IWaterBreakable
	{
		public Poppy() : base(TileDefinitions.Poppy) { }
		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (! (world.GetTile(x, y+1) is ISoil) )
			{
				world.SetTile(x, y, new Air());
			}
		}
	}
	public class TNT : Tile, ITileUpdate
	{
		public TNT() : base(TileDefinitions.TNT)
		{

		}

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			//world.SetTile(x, y, new Air());
			//world.Explosion(
			//	new Vector2(x, y) * 8,
			//	10.0f, 4.0f, true, true
			//);
		}
	}
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
