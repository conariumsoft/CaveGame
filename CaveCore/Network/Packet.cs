using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CaveGame.Core.Network
{

	public enum PacketType : uint
	{
		CRequestJoin = 1,
		SAcceptJoin,
		SDenyJoin,
		SKick,
		CAcceptJoinAck,
		CKeepAlive,
		SKeepAliveAck,
		CQuit,
		CChatMessage,
		SChatMessage,
		SDownloadChunk,
		STilesUpdate,
		PlaceTile,
		CActivateTile,
		CDamageTile,
		CRequestChunk,
		SPlayerJoined,
		SPlayerLeft,
		CPlayerInput,
		SAddPlayerEntity,
		SPlayerPosition,
		SDestroyEntity,
		ClientQuit
	}


	public class Packet
	{
		public PacketType Type;
		public long Timestamp;
		public byte[] Payload = new byte[0];

		#region Constructors
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

		#endregion

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
			// TODO maybe be async instead?
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

	// Client requests connection
	public class RequestJoinPacket : Packet
	{
		public string RequestedName
		{
			get { return Encoding.UTF8.GetString(Payload); }
			set { Payload = Encoding.UTF8.GetBytes(value); }
		}
		public RequestJoinPacket(byte[] bytes) : base(bytes) { }
		public RequestJoinPacket(string username) : base(PacketType.CRequestJoin) {
			RequestedName = username;
		}
	}

	public class DropEntityPacket : Packet
	{
		public int EntityID
		{
			get { return TypeSerializer.ToInt(Payload, 0); }
			set { TypeSerializer.FromInt(ref Payload, 0, value); }

		}
		public DropEntityPacket(int entityid) : base(PacketType.SDestroyEntity)
		{

		}
	}


	public class AcceptJoinPacket : Packet
	{
		public int YourUserNetworkID { 
			get { return TypeSerializer.ToInt(Payload, 0); }
			set { TypeSerializer.FromInt(ref Payload, 0, value); }
		}
		public int YourPlayerNetworkID {
			get { return TypeSerializer.ToInt(Payload, 4); }
			set { TypeSerializer.FromInt(ref Payload, 4, value); }
		}
		
		public AcceptJoinPacket(int userid, int playerid) : base(PacketType.SAcceptJoin) {
			Payload = new byte[12];
			YourPlayerNetworkID = playerid;
			YourUserNetworkID = userid;
		}

		public AcceptJoinPacket(byte[] bytes) : base(bytes) { }
	}

	public class QuitPacket : Packet
	{
		public int EntityID
		{
			get { return TypeSerializer.ToInt(Payload, 0); }
			set { TypeSerializer.FromInt(ref Payload, 0, value); }
		}
		public QuitPacket(int id) : base(PacketType.ClientQuit) {
			Payload = new byte[8];
			EntityID = id;
		}
		public QuitPacket(byte[] data) : base(data) { }
	}


	public class RejectJoinPacket : Packet
	{

		public string RejectReason
		{
			get { return Encoding.UTF8.GetString(Payload); }
			set { Payload = Encoding.UTF8.GetBytes(value); }
		}

		public RejectJoinPacket(string reason) : base(PacketType.SDenyJoin) {
			RejectReason = reason;
		}
		public RejectJoinPacket(byte[] bytes) : base(bytes) { }
	}

	public class PlayerJoinedPacket : Packet
	{
		// encode stuff about the playe
		public int EntityID
		{
			get { return TypeSerializer.ToInt(Payload, 0); }
			set { TypeSerializer.FromInt(ref Payload, 0, value); }
		}
		public byte UsernameLength
		{
			get { return Payload[10]; }
			set { Payload[10] = value; }
		}
		public string Username
		{
			get { return Encoding.UTF8.GetString(Payload, 12, UsernameLength); }
			set {

				UsernameLength = (byte)value.Length;

				Encoding.UTF8.GetBytes(value, 0, UsernameLength).CopyTo(Payload, 12);
				
			
			}
		}
		public Color PlayerColor
		{
			get {
				byte r = Payload[4];
				byte g = Payload[5];
				byte b = Payload[6];
				return new Color(r, g, b);
			}
			set {
				Payload[4] = value.R;
				Payload[5] = value.G;
				Payload[6] = value.B;
			}
		}

		public PlayerJoinedPacket(int id, string user, Color color) : base(PacketType.SPlayerJoined) {
			Payload = new byte[128];
			EntityID = id;
			Username = user;
			PlayerColor = color;
		}
		public PlayerJoinedPacket(byte[] bytes) : base(bytes) { }
	}

	public class PlayerLeftPacket : Packet
	{
		public int EntityID
		{
			get { return TypeSerializer.ToInt(Payload, 0); }
			set { TypeSerializer.FromInt(ref Payload, 0, value); }
		}

		public PlayerLeftPacket(byte[] bytes) : base(bytes) { }

		public PlayerLeftPacket(int id) : base(PacketType.SPlayerLeft) {
			Payload = new byte[4];
			EntityID = id;
		}
	}
	public interface IPrintablePacket { }

	public class EntityPositionPacket : Packet, IPrintablePacket
	{
		

		public override string ToString()
		{
			return String.Format("netid {0}[{1}] x {2}[{3}] y {4}[{5}]",
				EntityID, DumpHex(Payload, 0, 4),
				PosX, DumpHex(Payload, 4, 4),
				PosY, DumpHex(Payload, 8, 4)

			) ;
		}

		public int EntityID
		{
			get { return TypeSerializer.ToInt(Payload, 0); }
			set { TypeSerializer.FromInt(ref Payload, 0, value); }
		}

		public float PosX
		{
			get { return TypeSerializer.ToFloat(Payload, 4); }
			set { TypeSerializer.FromFloat(ref Payload, 4, value); }
		}

		public float PosY
		{
			get { return TypeSerializer.ToFloat(Payload, 8); }
			set { TypeSerializer.FromFloat(ref Payload, 8, value); }
		}

		public float VelX
		{
			get { return TypeSerializer.ToFloat(Payload, 12); }
			set { TypeSerializer.FromFloat(ref Payload, 12, value); }
		}

		public float VelY
		{
			get { return TypeSerializer.ToFloat(Payload, 16); }
			set { TypeSerializer.FromFloat(ref Payload, 16, value); }
		}

		public float NextX
		{
			get { return TypeSerializer.ToFloat(Payload, 20); }
			set { TypeSerializer.FromFloat(ref Payload, 20, value); }
		}

		public float NextY
		{
			get { return TypeSerializer.ToFloat(Payload, 24); }
			set { TypeSerializer.FromFloat(ref Payload, 24, value); }
		}



		public EntityPositionPacket(int entityid, float x, float y, 
			float vx=0, float vy=0, float nx = 0, float ny = 0) : base(PacketType.SPlayerPosition) {
			Payload = new byte[32];
			EntityID = entityid;
			PosX = x;
			PosY = y;
			VelX = vx;
			VelY = vy;
			NextX = nx;
			NextY = ny;
		}

		public EntityPositionPacket(byte[] bytes) : base(bytes) { }

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

	public class ServerChatMessagePacket: Packet
	{
		// Payload array offsets
		

		public string Message
		{
			get { return Encoding.UTF8.GetString(Payload, 3, 128); }
			set { Payload = Encoding.UTF8.GetBytes(value, 3, 128); }
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

		public ServerChatMessagePacket(string msg) : base(PacketType.SChatMessage) {
			TextColor = Color.White;
			Message = msg;
		}

		public ServerChatMessagePacket(string msg, Color col) : base(PacketType.SChatMessage) {
			TextColor = col;
			Message = msg;
		}
	}
 
	public class ChunkDownloadPacket : Packet
	{

		public Chunk StoredChunk
		{
			get {
				int chunkCoordsX = TypeSerializer.ToInt(Payload, 0);
				int chunkCoordsY = TypeSerializer.ToInt(Payload, 4);

				Chunk chk = new Chunk(chunkCoordsX, chunkCoordsY);
				int index = 2;
				for (int x = 0; x < Globals.ChunkSize; x++)
				{
					for (int y = 0; y < Globals.ChunkSize; y++)
					{
						

						var t = Tile.FromID(Payload[index*4]);
						t.Damage = Payload[1 + (index * 4)];
						t.TileState = Payload[2 + (index * 4)];


						chk.Tiles[x, y] = t;
						index++;
					}
				}
				return chk;
			}
			set {
				Payload = new byte[4200];
				int index = 0;
				TypeSerializer.FromInt(ref Payload, index * 4, value.Coordinates.X);
				index++;
				TypeSerializer.FromInt(ref Payload, index * 4, value.Coordinates.Y);
				index++;
				for (int x = 0; x < Globals.ChunkSize; x++)
				{
					for (int y = 0; y < Globals.ChunkSize; y++)
					{
						var t = value.Tiles[x, y];
						t.Serialize(ref Payload, index * 4);
						index++;
					}
				}
			}
		}
		public ChunkDownloadPacket(byte[] bytes) : base(bytes) { }
		public ChunkDownloadPacket(Chunk chunk) : base(PacketType.SDownloadChunk)
		{
			Payload = new byte[4200];
			StoredChunk = chunk;
		}
	}


	public class PlaceTilePacket : Packet 
	{

		public byte TileID
		{
			get { return Payload[0];  }
			set { Payload[0] = value; }
		}
		public byte TileState
		{
			get { return Payload[1];  }
			set { Payload[1] = value; }
		}

		public byte Damage
		{
			get { return Payload[2];  }
			set { Payload[2] = value; }
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

		public PlaceTilePacket(byte[] data) : base(data) { }

		public PlaceTilePacket(byte tileID, byte tileState, byte damage, int worldX, int worldY) : base(PacketType.PlaceTile) {
			Payload = new byte[16];
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
