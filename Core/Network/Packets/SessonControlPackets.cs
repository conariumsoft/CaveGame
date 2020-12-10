using DataManagement;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Network.Packets
{
	/// <summary>
	/// 
	/// </summary>
	public class GetServerInformationPacket : Packet
	{
		public GetServerInformationPacket(int protocol) : base(PacketType.GetServerInfo)
		{
			Payload = new byte[4];
			ClientProtocolVersion = protocol;
		}

		public GetServerInformationPacket(byte[] data) : base(data) { }
		/// <summary> 
		/// index 0, size 4 
		/// </summary>
		public int ClientProtocolVersion
		{
			get => Payload.ReadInt(0);
			set => Payload.WriteInt(0, value);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class ServerInformationReplyPacket : Packet
	{

		public int ServerProtocolVersion { get => Payload.ReadInt(0); set => Payload.WriteInt(0, value); }

		public int MaxPlayers { get => Payload.ReadInt(4); set => Payload.WriteInt(4, value); }


		public int ServerNameStrLength => Payload.ReadInt(8);
		public string ServerName
		{
			get => Payload.ReadString(8, 32, Encoding.ASCII);
			set => Payload.WriteString(8, value, Encoding.ASCII, 32);
		}
		public string ServerMOTD
		{
			get => Payload.ReadString(40, 128, Encoding.ASCII);
			set => Payload.WriteString(40, value, Encoding.ASCII, 128);
		}
		public string[] PlayerList // max 10
		{
			get => Payload.ReadStringArray(168, Encoding.ASCII, 16, 10);
			set => Payload.WriteStringArray(168, Encoding.ASCII, 16, 10, value);
		}
		public ServerInformationReplyPacket(byte[] data) : base(data) { }
		public ServerInformationReplyPacket(int protocol, string name, string motd, int maxplayers, string[] connected) : base(PacketType.ServerInfoReply)
		{
			Payload = new byte[480];
			ServerProtocolVersion = protocol;
			ServerName = name;
			ServerMOTD = motd;
			MaxPlayers = maxplayers;
			PlayerList = connected;
		}
	}


	public class AcceptJoinPacket : Packet
	{
		public int YourUserNetworkID   { get => Payload.ReadInt(0);	 set => Payload.WriteInt(0, value); }
		public int YourPlayerNetworkID { get => Payload.ReadInt(4);	 set => Payload.WriteInt(4, value); }

		public AcceptJoinPacket(int userid, int playerid) : base(PacketType.SAcceptJoin)
		{
			Payload = new byte[12];
			YourPlayerNetworkID = playerid;
			YourUserNetworkID = userid;
		}
		public AcceptJoinPacket(byte[] bytes) : base(bytes) { }
	}
	public enum UserDisconnectReason : byte
    {
		LogOff,
		Crash,
		Timeout,
		Kicked
    }
	public class DisconnectPacket : Packet
	{
		public int LeavingEntityID { get => Payload.ReadInt(0); set => Payload.WriteInt(0, value); }
		public UserDisconnectReason DisconnectReason { get => (UserDisconnectReason)Payload[4]; set => Payload[4] = (byte)value; }

		public DisconnectPacket(int entityID, UserDisconnectReason disconnectReason) : base(PacketType.ClientQuit)
		{
			Payload = new byte[8];
			LeavingEntityID = entityID;
			DisconnectReason = disconnectReason;
		}
		public DisconnectPacket(byte[] data) : base(data) { }
	}
	public class RejectJoinPacket : Packet
	{
		public string RejectReason
		{
			get => Payload.ReadString(0, 128, Encoding.ASCII);
			set => Payload.WriteString(0, value, Encoding.ASCII, 128);
		}
		public RejectJoinPacket(string reason) : base(PacketType.SDenyJoin)
		{
			Payload = new byte[192];
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
			set
			{

				UsernameLength = (byte)value.Length;

				Encoding.UTF8.GetBytes(value.ToCharArray(), 0, UsernameLength).CopyTo(Payload, 12);


			}
		}
		public Color PlayerColor
		{
			get
			{
				byte r = Payload[4];
				byte g = Payload[5];
				byte b = Payload[6];
				return new Color(r, g, b);
			}
			set
			{
				Payload[4] = value.R;
				Payload[5] = value.G;
				Payload[6] = value.B;
			}
		}
		public PlayerJoinedPacket(int id, string user, Color color) : base(PacketType.SPlayerJoined)
		{
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
		public PlayerLeftPacket(int id) : base(PacketType.SPlayerLeft)
		{
			Payload = new byte[4];
			EntityID = id;
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
		public RequestJoinPacket(string username) : base(PacketType.CRequestJoin)
		{
			RequestedName = username;
		}
	}

	public class KickPacket : Packet
	{


		public KickPacket(string reason) : base(PacketType.SKick)
		{
			Payload = new byte[128];
			KickReason = reason;

		}
		public KickPacket(byte[] bytes) : base(bytes) { }
		public byte KickReasonLength
		{
			get { return Payload[0]; }
			set { Payload[0] = value; }
		}
		public string KickReason
		{
			get { return Encoding.UTF8.GetString(Payload, 1, KickReasonLength); }
			set
			{

				KickReasonLength = (byte)value.Length;

				Encoding.UTF8.GetBytes(value.ToCharArray(), 0, KickReasonLength).CopyTo(Payload, 1);
			}
		}
	}
}
