using CaveGame.Common.Game.Entities;
using CaveGame.Common.Game.Items;
using CaveGame.Common.Generic;
using CaveGame.Common.Game.Inventory;
using CaveGame.Common.Network;
using CaveGame.Common.Noise;
using CaveGame.Common.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CaveGame.Common.Game.Tiles
{

	public class InvalidTileIDException : Exception
    {
		public InvalidTileIDException(string message) : base(message) { }
    }
	public class TileDataValidationException : Exception
    {
		public TileDataValidationException(List<string> problems) : base(String.Format("Tile Validation failed: {0} problem(s) detected.", problems.Count)) {
			Data.Add("Problems", problems);
		}
    }
	public abstract class Tile : IEquatable<Tile>
	{

		const int grid_width = 64;
		const int grid_height = 64;
		const float n_scale = 0.5f;
		const float n_mult = 40;
		private static float[,] PrecomputeFloatMap()
		{
			var map = new float[grid_width, grid_height];
			SimplexNoise noise = new SimplexNoise();
			for (int x = 0; x < grid_width; x++)
				for (int y = 0; y < grid_height; y++)
					map[x, y] = noise.Noise(x/n_scale, y/n_scale)* n_mult;

			return map;
		}
		private static int[,] PrecomputeIntMap()
        {
			var map = new int[grid_width, grid_height];

			SimplexNoise noise = new SimplexNoise();
			for (int x = 0; x < grid_width; x++)
				for (int y = 0; y < grid_height; y++)
					map[x, y] = (int) ( noise.Noise(x / n_scale, y / n_scale)* n_mult);

			return map;
		}


		public static float[,] RNGFloatMap = PrecomputeFloatMap();
		public static int[,] RNGIntMap = PrecomputeIntMap();


		public static void InitializeManager(int rngseed)
        {

        }

		public static void AssertTileEnumeration() {	}

		// Tile Properties
		public virtual float Friction => 1;
		public virtual byte Opacity => 24;
		public virtual Color Color => Color.White;
		public virtual Rectangle Quad => TileMap.Default;
		public virtual byte Hardness => 2;
		public virtual string Namespace => "CaveGame";
		public virtual string TileName => this.GetType().Name;
		public virtual short ID => GetID();


		private short GetID()
        {
			bool success = Enum.TryParse<TileID>(GetType().Name, out TileID val);

			if (success)
            {
				return (short)val;
            }
			throw new InvalidTileIDException(String.Format("Tile {0} does not have TileID Enumeration of matching name. Please add to TileID.cs.", GetType().Name));
		}
		// Default Methods


		public virtual void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			GFX.Sprite(
				texture: GFX.TileSheet,
				position: new Vector2(x * Globals.TileSize, y * Globals.TileSize),
				quad: Quad, color.MultiplyAgainst(Color), 
				rotation: Rotation.Zero,
				origin: Vector2.Zero, 
				scale: 1, 
				efx: SpriteEffects.None, 
				layer: 0
			);
		}


		public byte Damage { get; set; }
		public byte TileState { get; set; }

		public byte[] Serialize()
        {
			byte[] serializedTile = new byte[4];
			Encode(ref serializedTile, 0);
			return serializedTile;
		}

		public void Serialize(ref byte[] datagram, int index)
		{
			Encode(ref datagram, index);
		}

		public virtual void Encode(ref byte[] datagram, int pushIndex)
		{
			datagram.WriteShort(pushIndex, ID);
			datagram[pushIndex+2] = Damage;
			datagram[pushIndex+3] = TileState;
		}

		public virtual void Decode(ref byte[] datagram, int pullIndex)
		{
			Damage = datagram[pullIndex + 2];
			TileState = datagram[pullIndex + 3];
		}

		public static Tile Deserialize(ref byte[] datagram, int pullIndex)
		{
			Tile t = FromID(datagram.ReadShort(pullIndex));
			t.Decode(ref datagram, pullIndex);
			return t;
		}


		

		protected void Drop(IGameServer server, IGameWorld world, Point tilePosition, ItemStack stack)
		{
			var entity = new ItemstackEntity { ItemStack = stack };
			entity.NextPosition = tilePosition.ToVector2() * 8;
			entity.Position = tilePosition.ToVector2() * 8;
			server.SpawnEntity(entity);
			server.SendToAll(new SpawnItemStackPacket(tilePosition.ToVector2() * 8, entity.ItemStack, entity.EntityNetworkID));
		}

		public virtual void Drop(IGameServer server, IGameWorld world, Point tilePosition)
		{
			ItemStack stack = new ItemStack { Quantity = 1, Item = new TileItem(this) };
			Drop(server, world, tilePosition, stack);
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


		// static shit

		public static Random RNG = new Random();

		public static short IDOf<T>()
		{
			var type = typeof(T);

			byte id = (byte)Enum.Parse(typeof(TileID), type.Name);

			return id;
		}
		public static Tile FromID(short t)
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
	}
}
