#if CLIENT
using CaveGame.Client;
#endif
using CaveGame.Core.Game.Entities;
using CaveGame.Core.Inventory;
using CaveGame.Core.Network;
using DataManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CaveGame.Core.Game.Tiles
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

		[Conditional("DEBUG")]
		public static void AssertTileEnumeration() {	}

		// Tile Properties
		public virtual float Friction => 1;
		public virtual byte Opacity => 3;
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


		public virtual void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 color)
		{
			sb.Draw(
				tilesheet,
				new Vector2(x * Globals.TileSize, y * Globals.TileSize),
				Quad, color.MultiplyAgainst(Color), 0,
				Vector2.Zero, 1, SpriteEffects.None, 0
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
#if CLIENT
		public static Texture2D Tilesheet = GameTextures.TileSheet;
#else
		public static Texture2D Tilesheet;
#endif
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
