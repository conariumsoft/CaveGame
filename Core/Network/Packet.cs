using CaveGame.Core.Game.Entities;
using CaveGame.Core.Furniture;
using CaveGame.Core.Game.Tiles;
using CaveGame.Core.Game.Walls;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CaveGame.Core.Game.Inventory;
using CaveGame.Core.FileUtil;
using CaveGame.Core.Inventory;
using CaveGame.Core.Game.Items;
using DataManagement;
using System.Collections.Generic;

namespace CaveGame.Core.Network
{

	public static class MonoGameByteArrayExtensions // serialize monogame types
    {
		/// <summary>
		/// Data Length = 8 (2*float)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static Vector2 ReadVector2(this byte[] data, int index)=> new Vector2(data.ReadFloat(index), data.ReadFloat(index + 4));
		/// <summary>
		/// Data Length = 8 (2*float)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="index"></param>
		/// <param name="value"></param>
        public static void WriteVector2(this byte[] data, int index, Vector2 value)
        {
			data.WriteFloat(index, value.X);
			data.WriteFloat(index + 4, value.Y);
        }
		public static Color ReadColorRGBA(this byte[] data, int index) => new Color(data[index], data[index + 1], data[index + 2], data[index+3]);
		/// <summary>
		/// Data Length = 4 (4*byte)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="index"></param>
		/// <param name="value"></param>
		public static void WriteColorRGBA(this byte[] data, int index, Color value)
        {
			data[index] = value.R;
			data[index + 1] = value.G;
			data[index + 2] = value.B;
			data[index + 3] = value.A;


			byte[] somedata = new byte[20];
        }
    }

	public enum PacketType : uint
	{
		#region Agnostic-Sender Packets
		nPing = 0,


		#endregion

		#region Client-Sender Packets
		cHandshake,
		cPlaceTile,
		cPlaceWall,
		cDamageTile,
		cDamageWall,
		cRequestLogin,
		cConfirmLogin,
		cLogout,
		cSendChatMessage,
		cChangeTimeOfDay,
		#endregion

		#region Server-Sender Packets
		sHandshakeResponse,
		sDownloadChunk,
		sChatMessage,
		sKick,
		sUpdateTile,
		sUpdateWall,
		sAcceptLogin,
		sRejectLogin,
		sSpawnEntityGeneric,
		sSpawnItemStackEntity,
		sRemoveEntity,
		sEntityPhysicsUpdate,
		sOpenDoor, sCloseDoor,
		sPlaceFurniture, sRemoveFurniture,
		sDamageTile,
		sGivePlayerItem,
		sPlayerState,
		sPlayerPeerJoined,
		sPlayerPeerLeft,
		sExplosion,
		sUpdateTimeOfDay,
		#endregion

		/*cGetServerInfo = 0,
		sCRequestJoin,
		sAcceptJoin,
		SDenyJoin,
		sKick,
		CAcceptJoinAck,
		CKeepAlive,
		SKeepAliveAck,
		CQuit,
		CChatMessage,
		ServerChatMessage,
		SDownloadChunk,
		STilesUpdate,
		PlaceTile, PlaceWall,CActivateTile,CDamageTile,CRequestChunk,
		SPlayerJoined,
		SPlayerLeft,CPlayerInput,	SAddPlayerEntity,
		ServerEntityPhysicsState,
		SDestroyEntity,ClientQuit,
		PlayerState,
		SExplosion,
		PlayerThrowItemAction,
		SpawnBombEntity,
		SpawnWurmholeEntity,
		TriggerWurmholeEntitySpawnItemStackEntity,
		PlaceFurniture, RemoveFurniture,
		OpenDoor, CloseDoor, TimeOfDay,
		TransferContainer,
		UpdateContainer,
		DamageTile,
		GivePlayerItem, // Temporary for testing?
		AdminCommand*/
	}

	public enum NetEntityType : byte
    {
		Bomb,
		Wurmhole,
		Arrow,
		Dynamite,


    }

	public class Packet
	{
		public PacketType Type;
		public long Timestamp;
		public byte[] Payload = new byte[0];
		// Creates a packet with the set type and an empty Payload
		public Packet(PacketType type)
		{
			Type = type;
			Timestamp = DateTime.Now.Ticks;
		}
		public Packet(byte[] bytes)
		{
			// start peeling out the data from the byte array
			int i = 0;
			Type = (PacketType)BitConverter.ToUInt32(bytes, 0);
			i += sizeof(PacketType);

			Timestamp = BitConverter.ToInt64(bytes, i);
			i += sizeof(long);

			Payload = bytes.Skip(i).ToArray();
		}
		public byte[] GetBytes()
		{
			int ptSize = sizeof(PacketType);
			int tsSize = sizeof(long);

			int i = 0;
			byte[] bytes = new byte[ptSize + tsSize + Payload.Length];

			BitConverter.GetBytes((uint)this.Type).CopyTo(bytes, i);
			i += ptSize;

			// Timestamp
			BitConverter.GetBytes(Timestamp).CopyTo(bytes, i);
			i += tsSize;

			// Payload
			Payload.CopyTo(bytes, i);
			i += Payload.Length;

			return bytes;

		}
		public override string ToString()
		{
			return string.Format("[Packet:{0}\n  timestamp={1}\n  payload size={2}]",
				this.Type, new DateTime(Timestamp), Payload.Length);
		}
		// Sends a Packet to a specific receiver 
		public void Send(UdpClient client, IPEndPoint receiver)
		{
			byte[] bytes = GetBytes();
			client.Send(bytes, bytes.Length, receiver);
		}
		// Send a Packet to the default remote receiver (will throw error if not set)
		public void Send(UdpClient client)
		{
			byte[] bytes = GetBytes();
			client.Send(bytes, bytes.Length);
		}
		protected string DumpHex(byte[] data, int index = 0)
		{
			return DumpHex(data, index, data.Length);
		}
		protected string DumpHex(byte[] data, int index, int length)
		{
			StringBuilder bob = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				bob.Append(String.Format("{0:X2} ", data[i + index]));
			}
			return bob.ToString();
		}
	}

	public class TimeOfDayPacket : Packet
	{
		public float Time
		{
			get => Payload.ReadFloat(0);
			set => Payload.WriteFloat(0, value);
		}
		public TimeOfDayPacket(byte[] data) : base(data) { }
		public TimeOfDayPacket(float time) : base(PacketType.sUpdateTimeOfDay)
		{
			Payload = new byte[4];
			Time = time;
		}
	}
	public class ExplosionPacket : Packet
	{
		public Explosion Explosion
        {
			get => new Explosion
			{
				Position = Payload.ReadVector2(0), // 8 bytes (8)
				BlastRadius = Payload.ReadFloat(8), // 4 bytes (12)
				BlastPressure = Payload.ReadFloat(12)// 4 bytes (16)			
			};

			set
            {
				Payload.WriteVector2(0, value.Position);
				Payload.WriteFloat(8, value.BlastRadius);
				Payload.WriteFloat(12, value.BlastPressure);
            }
        }
		public ExplosionPacket(Explosion explosion, bool harmtiles, bool harmentities) : base(PacketType.sExplosion) {
			Payload = new byte[20];
			Explosion = explosion;
			DamageEntities = harmentities;
			DamageTiles = harmtiles;
		}
		public ExplosionPacket(byte[] data) : base(data) { }

		public bool DamageTiles
		{
			get { return Payload[16].Get(0); }
			set { Payload[16].Set(0, value); }
		}
		public bool DamageEntities
		{
			get { return Payload[16].Get(1); }
			set { Payload[16].Set(1, value); }
		}
	}
	public class SpawnEntityGenericPacket : Packet
    {
		public int EntityNetworkID { get => Payload.ReadInt(0); set => Payload.WriteInt(0, value); }

		public NetEntityType EntityType { get => (NetEntityType)Payload[4]; set => Payload[4] = (byte)value; } 

		public SpawnEntityGenericPacket(int id, NetEntityType type) : base(PacketType.sSpawnEntityGeneric)
		{
			Payload = new byte[12];
			EntityNetworkID = id;
			EntityType = type;
		}
		public SpawnEntityGenericPacket(byte[] data) : base(data) { }
	}


	public class SpawnWurmholeEntityPacket : Packet
    {
		public int EntityNetworkID
        {
			get => Payload.ReadInt(0);
			set => Payload.WriteInt(0, value);
        }
		

		public SpawnWurmholeEntityPacket(byte[] data) : base(data) { }
		public SpawnWurmholeEntityPacket(int entityNetworkID) : base(PacketType.SpawnWurmholeEntity) 
        {
			Payload = new byte[8];
			EntityNetworkID = entityNetworkID;
        }
    }

	public class TriggerWurmholeEntityPacket : Packet
    {
		public int EntityNetworkID
		{
			get => Payload.ReadInt(0);
			set => Payload.WriteInt(0, value);
		}
		public TriggerWurmholeEntityPacket(byte[] data) : base(data) { }
		public TriggerWurmholeEntityPacket(int entityNetworkID) : base(PacketType.TriggerWurmholeEntity) // should be PacketType.TriggerWurmholeEntity
		{
			Payload = new byte[8];
			EntityNetworkID = entityNetworkID;
		}
	}

    public class SpawnItemStackPacket : Packet {

		public int EntityNetworkID
		{
			get => Payload.ReadInt(0);
			set => Payload.WriteInt(0, value);
		}

		public Vector2 Position
        {
			get => Payload.ReadVector2(4);
			set => Payload.WriteVector2(4, value);
		}
		public ItemStack ItemStack
        {
			get
            {
				int quantity = Payload.ReadInt(12);
				Payload.ReadStringAuto(16, Encoding.ASCII, out string itemname);
				return new ItemStack { Quantity = quantity, Item = Item.FromName(itemname) };
            }
			set
            {
				Payload = new byte[16+4+Encoding.ASCII.GetByteCount(value.Item.Name)];
				Payload.WriteInt(12, value.Quantity);
				Payload.WriteStringAuto(16, value.Item.Name, Encoding.ASCII);
            }
        }


		public SpawnItemStackPacket(Vector2 position, ItemStack stack, int networkID) : base(PacketType.SpawnItemStackEntity)
        {
			ItemStack = stack; // Reminder: set ItemStack first to initalize payload size
			// this is bad practice, but it works.
			EntityNetworkID = networkID;
			Position = position;
        }
		public SpawnItemStackPacket(byte[] data) : base(data) { }
	}

	public class GivePlayerItemPacket : Packet
    {
		public ItemStack Reward
        {
			get
			{
				int quantity = Payload.ReadInt(0);
				Payload.ReadStringAuto(4, Encoding.ASCII, out string itemname);
				return new ItemStack { Quantity = quantity, Item = Item.FromName(itemname) };
			}
			set
			{
				Payload = new byte[4 + 4 + Encoding.ASCII.GetByteCount(value.Item.Name)];
				Payload.WriteInt(0, value.Quantity);
				Payload.WriteStringAuto(4, value.Item.Name, Encoding.ASCII);
			}
		}




		public GivePlayerItemPacket(byte[] data) : base(data) { }
		public GivePlayerItemPacket(ItemStack reward) : base(PacketType.GivePlayerItem)
        {
			
			Reward = reward;
        }
    }
	public class DamageTilePacket : Packet
    {
		public Point Position
        {
			get
            {
				int x = TypeSerializer.ToInt(Payload, 0);
				int y = TypeSerializer.ToInt(Payload, 4);
				return new Point(x, y);
            }

			set
            {
				TypeSerializer.FromInt(ref Payload, 0, value.X);
				TypeSerializer.FromInt(ref Payload, 4, value.Y);
			}
        }

		public int Damage
        {
			get => TypeSerializer.ToInt(Payload, 8);
			set => TypeSerializer.FromInt(ref Payload, 8, value);
        }

		public DamageTilePacket(byte[] data) : base(data) { }

		public DamageTilePacket(Point worldPosition, int damage) : base(PacketType.DamageTile) {
			Payload = new byte[16];
			Position = worldPosition;
			Damage = damage;
		}
		
    }

	public class DropEntityPacket : Packet
	{
		public int EntityID
		{
			get => TypeSerializer.ToInt(Payload, 0);
			set => TypeSerializer.FromInt(ref Payload, 0, value);

		}
		public DropEntityPacket(int entityid) : base(PacketType.SDestroyEntity)
		{
			Payload = new byte[8];
			EntityID = entityid;
		}
		public DropEntityPacket(byte[] data) : base(data) { }
	}


	public class TransferContainerPacket : Packet
    {
		
		public TransferContainerPacket(Container container) : base(PacketType.TransferContainer)
        {
			Container = container;
        }
		public Container Container
        {
			get => Container.FromMetabinary(Metabinary.Deserialize(Payload));
			set => Payload = value.ToMetabinary().Serialize();
        }
    }

	

	public class EntityPositionPacket : Packet
	{
		public int EntityID
		{
			get => Payload.ReadInt(0);
			set => Payload.WriteInt(0, value);
		}

		public int Health
		{
			get => Payload.ReadInt(4);
			set => Payload.WriteInt(4, value);
		}

		public Vector2 Position
        {
			get => Payload.ReadVector2(8);
			set => Payload.WriteVector2(8, value);
        }

		public Vector2 Velocity
        {
			get => Payload.ReadVector2(16);
			set => Payload.WriteVector2(16, value);
        }

		public Vector2 NextPosition
        {
			get => Payload.ReadVector2(24);
			set => Payload.WriteVector2(24, value);
        }
		

		public EntityPositionPacket(int id, int health, Vector2 position, Vector2 velocity, Vector2 nextPosition) : base(PacketType.ServerEntityPhysicsState) {
			Payload = new byte[32];
			EntityID = id;
			Health = health;
			Position = position;
			Velocity = velocity;
			NextPosition = nextPosition;
		}

		public EntityPositionPacket(IEntity entity) : base(PacketType.ServerEntityPhysicsState)
        {
			Payload = new byte[32];
			EntityID = entity.EntityNetworkID;
			Health = entity.Health;
			Position = entity.Position;
			if (entity is IPhysicsEntity physics)
            {
				Velocity = physics.Velocity;
				NextPosition = physics.NextPosition;
			}
        }

		public EntityPositionPacket(byte[] bytes) : base(bytes) { }
	}

	public class PlaceFurniturePacket : Packet
	{
		public PlaceFurniturePacket(byte furnitureid, int netid, int x, int y) : base(PacketType.PlaceFurniture) {
			Payload = new byte[16];
			FurnitureID = furnitureid;
			NetworkID = netid;
			WorldX = x;
			WorldY = y;
		}
		public PlaceFurniturePacket(byte[] data) : base(data) { }
		public byte FurnitureID
		{
			get { return Payload[12]; }
			set { Payload[12] = value; }
		}
		public int NetworkID
		{
			get { return TypeSerializer.ToInt(Payload, 0); }
			set { TypeSerializer.FromInt(ref Payload, 0, value); }
		}
		public int WorldX
		{
			get { return TypeSerializer.ToInt(Payload, 4); }
			set { TypeSerializer.FromInt(ref Payload, 4, value); }
		}
		public int WorldY
		{
			get { return TypeSerializer.ToInt(Payload, 8); }
			set { TypeSerializer.FromInt(ref Payload, 8, value); }
		}
	}

	public class OpenDoorPacket : Packet
	{
		public int FurnitureNetworkID {
			get { return TypeSerializer.ToInt(Payload, 0); }
			set { TypeSerializer.FromInt(ref Payload, 0, value); }
		}
		public Direction Direction {
			get { return (Direction)Payload[5]; }
			set { Payload[5] = (byte)value;}
		}
		public OpenDoorPacket(int id, Direction dir) : base(PacketType.OpenDoor) {
			Payload = new byte[16];
			FurnitureNetworkID = id;
			Direction = dir;
		}
		public OpenDoorPacket(byte[] data) : base(data) { }
	}

	public class CloseDoorPacket : Packet
	{
		public int FurnitureNetworkID {
			get { return TypeSerializer.ToInt(Payload, 0); }
			set { TypeSerializer.FromInt(ref Payload, 0, value); }
		}
		public CloseDoorPacket(int id) : base(PacketType.CloseDoor)
		{
			Payload = new byte[8];
			FurnitureNetworkID = id;
		}
		public CloseDoorPacket(byte[] data) : base(data) { }
	}

	public class RemoveFurniturePacket : Packet
	{
		public RemoveFurniturePacket(int id) : base(PacketType.RemoveFurniture) {
			Payload = new byte[12];
			FurnitureNetworkID = id;
		}
		public RemoveFurniturePacket(byte[] data) : base(data) { }
		public int FurnitureNetworkID
		{
			get => Payload.ReadInt(0);//{ return TypeSerializer.ToInt(Payload, 0); }
			set => Payload.WriteInt(0, value);//{ TypeSerializer.FromInt(ref Payload, 0, value); }
		}
	}

	public class PlayerStatePacket : Packet
	{
		public int EntityID
		{
			get => Payload.ReadInt(0);
			set => Payload.WriteInt(0, value);
		}
		public Direction Facing
		{
			get { return (Direction)Payload[4]; }
			set { Payload[4] = (byte)value; }
		}

		public bool OnGround {
			get { return Payload[5].Get(0); }
			set { Payload[5].Set(0, value); }
		}

		public bool Walking
		{
			get { return Payload[5].Get(1); }
			set { Payload[5].Set(1, value); }
		}

		public PlayerStatePacket(Direction f, bool grounded, bool walk) : base(PacketType.PlayerState)
		{
			Payload = new byte[16];

			OnGround = grounded;
			Facing = f;
			Walking = walk;
		}

		public PlayerStatePacket(byte[] data) : base(data) { }


	}

	public class PlayerInputPacket : Packet
	{
		public PlayerInputPacket() : base(PacketType.CPlayerInput) { }
	}

	public class ClientChatMessagePacket : Packet
	{
		public string Message
		{
			get { return Encoding.UTF8.GetString(Payload); }
			set { Payload = Encoding.UTF8.GetBytes(value); }
		}

		public ClientChatMessagePacket(string msg) : base(PacketType.CChatMessage) {
			Message = msg;
		}

		public ClientChatMessagePacket(byte[] bytes) : base(bytes) { }
	}


	// TODO: new payload
	public class ServerChatMessagePacket: Packet
	{
		// Payload array offsets

		public byte MessageLength
		{
			get { return Payload[3]; }
			set { Payload[3] = value; }
		}
		public string Message
		{
			get { return Encoding.UTF8.GetString(Payload, 4, MessageLength); }
			set { 
				MessageLength = (byte)value.Length;
				Encoding.UTF8.GetBytes(value).CopyTo(Payload, 4); }
		}

		public Color TextColor
		{
			get {
				byte r = Payload[0];
				byte g = Payload[1];
				byte b = Payload[2];
				return new Color(r, g, b);
			}
			set {
				Payload[0] = value.R;
				Payload[1] = value.G;
				Payload[2] = value.B;
			}
		}

		public ServerChatMessagePacket(byte[] data) : base(data) { }

		public ServerChatMessagePacket(string msg) : base(PacketType.ServerChatMessage) {
			Payload = new byte[140];
			TextColor = Color.White;
			Message = msg;
		}

		public ServerChatMessagePacket(string msg, Color col) : base(PacketType.ServerChatMessage) {
			Payload = new byte[140];
			TextColor = col;
			Message = msg;
		}
	}
 
	public class ChunkDownloadPacket : Packet
	{

		public Chunk StoredChunk
		{
			get {
				
				int chunkCoordsX = Payload.ReadInt(0);
				int chunkCoordsY = Payload.ReadInt(4);

				Chunk chk = new Chunk(chunkCoordsX, chunkCoordsY);
				int index = 8;
				for (int x = 0; x < Globals.ChunkSize; x++)
				{
					for (int y = 0; y < Globals.ChunkSize; y++)
					{
						chk.Tiles[x, y] = Tile.Deserialize(ref Payload, index);
						index+=4;
					}
				}

				for (int x = 0; x < Globals.ChunkSize; x++)
				{
					for (int y = 0; y < Globals.ChunkSize; y++)
					{
						chk.Walls[x, y] = Wall.Deserialize(ref Payload, index);
						index+=4;
					}
				}
				return chk;
			}
			set {
				//Payload = new byte[10000];
				
				Payload.WriteInt(0, value.Coordinates.X);
				Payload.WriteInt(4, value.Coordinates.Y);
				int index = 8;
				for (int x = 0; x < Globals.ChunkSize; x++)
				{
					for (int y = 0; y < Globals.ChunkSize; y++)
					{
						var t = value.Tiles[x, y];
						t.Serialize(ref Payload, index);
						index+=4;
					}
				}
				for (int x = 0; x < Globals.ChunkSize; x++)
				{
					for (int y = 0; y < Globals.ChunkSize; y++)
					{
						var w = value.Walls[x, y];
						w.Serialize(ref Payload, index);
						index+=4;
					}
				}
			}
		}
		public ChunkDownloadPacket(byte[] bytes) : base(bytes) { }
		public ChunkDownloadPacket(Chunk chunk) : base(PacketType.SDownloadChunk)
		{
			Payload = new byte[10000];
			StoredChunk = chunk;
		}
	}

	public enum ThrownItem : byte
	{
		Bomb,
	}

	public class PlayerThrowItemPacket : Packet
	{
		public ThrownItem Item
		{
			get { return (ThrownItem)Payload[0]; }
			set { Payload[0] = (byte)value; }
		}

		public Rotation ThrownDirection
		{
			get { return Rotation.FromRad(TypeSerializer.ToFloat(Payload, 1)); }
			set { TypeSerializer.FromFloat(ref Payload, 1, value.Radians); }
		}

		public PlayerThrowItemPacket(ThrownItem item, Rotation dir) : base(PacketType.PlayerThrowItemAction)
		{
			Payload = new byte[20];
			Item = item;
			ThrownDirection = dir;
		}
		public PlayerThrowItemPacket(byte[] data) : base(data) { }
	}

	public class PlayerActionPacket : Packet
	{
		public PlayerActionPacket(PacketType type) : base(type)
		{
		}

	}

	public class AdminCommandPacket : Packet
    {
		// 0 - PlayerNetworkID int
		// 4 - CommandStringLength int
		//
		//
		//
		//
		//
		//

		public string Command
		{
			get
            {
				Payload.ReadStringAuto(4, Encoding.ASCII, out string ret);
				return ret;
            }
			set => Payload.WriteStringAuto(4, value, Encoding.ASCII);
		}


		public int CommandByteDataLength => Payload.ReadInt(4);

		public int PlayerNetworkID
        {
			get => Payload.ReadInt(0);
			set => Payload.WriteInt(0, value);
        }



		public string[] Arguments
        {
			get
            {
				int index = 4+CommandByteDataLength+4;
				int expectedStrings = Payload.ReadInt(index);
				List<string> argdata = new List<string>();
				index += 4;

				for (int i = 0; i < expectedStrings; i++)
                {
					index += Payload.ReadStringAuto(index, Encoding.ASCII, out string result);
					argdata.Add(result);
				}
				return argdata.ToArray();
			}
			set
            {	
				int index = 4+CommandByteDataLength+4;
				Payload.WriteInt(index, value.Length);
				index += 4;

				foreach (string str in value)
					index += Payload.WriteStringAuto(index, str, Encoding.ASCII);
            }
        }

		public AdminCommandPacket(byte[] data) : base(data) { }
		public AdminCommandPacket(string command, string[] args, int playerNetworkID) : base(PacketType.AdminCommand) {
			// get size?
			int size = 8+ Encoding.ASCII.GetBytes(command).Length + 4;
			foreach (var str in args)
				size += Encoding.ASCII.GetBytes(str).Length + 4;

			Payload = new byte[size];
			Command = command;
			Arguments = args;
			PlayerNetworkID = playerNetworkID;
		}
	}


	public class PlaceWallPacket : Packet
	{

		public short WallID
		{
			get => Payload.ReadShort(0);
			set => Payload.WriteShort(0, value);
		}
		public byte TileState
		{
			get { return Payload[2]; }
			set { Payload[2] = value; }
		}
		public byte Damage
		{
			get { return Payload[3]; }
			set { Payload[3] = value; }
		}
		public int WorldX
		{
			get => Payload.ReadInt(4);
			set => Payload.WriteInt(4, value);
		}
		public int WorldY
		{
			get => Payload.ReadInt(8);
			set => Payload.WriteInt(8, value);
		}
		public PlaceWallPacket(byte[] data) : base(data) { }
		public PlaceWallPacket(short wallID, byte tileState, byte damage, int worldX, int worldY) : base(PacketType.PlaceWall)
		{
			Payload = new byte[16];
			WallID = wallID;
		//	TileState = tileState;
			Damage = damage;
			WorldX = worldX;
			WorldY = worldY;
		}
	}
	public class PlaceTilePacket : Packet 
	{
		public short TileID
		{
			get => Payload.ReadShort(0);
			set => Payload.WriteShort(0, value);
		}
		public byte TileState
		{
			get => Payload[2];
			set => Payload[2] = value;
		}
		public byte Damage
		{
			get { return Payload[3];  }
			set { Payload[3] = value; }
		}
		public int WorldX
		{
			get => Payload.ReadInt(4); //{ return TypeSerializer.ToInt(Payload, 4); }
			set => Payload.WriteInt(4, value); //{ TypeSerializer.FromInt(ref Payload, 4, value); }
		}
		public int WorldY
		{
			get => Payload.ReadInt(8); // { return TypeSerializer.ToInt(Payload, 8); }
			set => Payload.WriteInt(8, value);//{ TypeSerializer.FromInt(ref Payload, 8, value); }
		}
		public PlaceTilePacket(byte[] data) : base(data) { }
		public PlaceTilePacket(short tileID, byte tileState, byte damage, int worldX, int worldY) : base(PacketType.PlaceTile) {
			Payload = new byte[24];
			TileID = tileID;
			TileState = tileState;
			Damage = damage;
			WorldX = worldX;
			WorldY = worldY;
		}
	}
	// temporary?
	public class RequestChunkPacket : Packet
	{
		public int X
		{
			get
			{
				return TypeSerializer.ToInt(Payload, 0);
			}
			set
			{
				TypeSerializer.FromInt(ref Payload, 0, value);
			}
		}
		public int Y
		{
			get
			{
				return TypeSerializer.ToInt(Payload, 4);
			}
			set
			{
				TypeSerializer.FromInt(ref Payload, 4, value);
			}
		}
		public RequestChunkPacket(byte[] bytes) : base(bytes) {
			//Payload = new byte[8];
		}
		public RequestChunkPacket(ChunkCoordinates coords) : base(PacketType.CRequestChunk)
		{
			Payload = new byte[8];
			X = coords.X;
			Y = coords.Y;
		}
	}
}
