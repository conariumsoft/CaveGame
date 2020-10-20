#define PACKETDEBUG

using CaveGame.Core;
using CaveGame.Core.Entities;
using CaveGame.Core.FileUtil;
using CaveGame.Core.Furniture;
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CaveGame.Server
{
	public interface IPluginAPIServer
	{
		IMessageOutlet Output { get; }

		DateTime Time { get; }


		void Chat(string text);
		void Chat(string text, Color color);
		ServerWorld World { get; }

	}

	public interface IGameServer
	{

	}

	public class GameServer : IPluginAPIServer, IGameServer
	{

		#region Lua API Members
		public DateTime Time => DateTime.Now;

		public void Chat(string msg)
		{
			SendToAll(new ServerChatMessagePacket(msg));
		}
		public void Chat(string msg, Color color)
		{
			SendToAll(new ServerChatMessagePacket(msg, color));
		}

		public IMessageOutlet Output
		{
			get { return server.Output; }
			set { server.Output = value; }
		}


		#endregion

		const int serverProtocolVersion = 1;

		private NetworkServer server;

		public PluginManager PluginManager;

		public bool Running { get; set; }


		

		public int TickRate { get; private set; }

		public List<User> ConnectedUsers { get; private set; }
		public ServerWorld World { get; private set; }
		float ticker;


		public string ServerName { get; private set; }
		public string ServerMOTD { get; private set; }
		public int MaxPlayers { get; private set; }

		public GameServer(ServerConfig config) {

			ServerName = config.ServerName;
			ServerMOTD = config.ServerMOTD;
			MaxPlayers = config.MaxPlayers;

			TickRate = config.TickRate;
			ticker = 0;
			server = new NetworkServer(config.Port);
			ConnectedUsers = new List<User>();
			World = new ServerWorld(new WorldConfiguration { Name = config.World, Seed = 420 });
			World.Server = this;

			PluginManager = new PluginManager();
		}

		public void LoadPlugins()
		{


			if (!System.IO.Directory.Exists("Plugins"))
				System.IO.Directory.CreateDirectory("Plugins");


			foreach (string folder in System.IO.Directory.EnumerateDirectories("Plugins"))
			{
				var pluginDefinition = Configuration.Load<PluginDefinition>(folder + "/plugin.xml");

				var plug = new Plugin(pluginDefinition, this, System.IO.File.ReadAllText(folder + "/main.lua"));
				PluginManager.Plugins.Add(plug);
			}

			PluginManager.CallOnPluginLoaded();
		}

		
		public void Start()
		{
			server.Start();
			Running = true;
		}

		public void Shutdown()
		{
			PluginManager.CallOnServerShutdown();
			Console.WriteLine("Shutting Down. Not Saving while testing worldgen");
			World.SaveData();
			Thread.Sleep(100);
		}

		public void SendTo(Packet p, User user) {
			server.SendPacket(p, user.EndPoint);
		}
		public void SendToAll(Packet p) {
			foreach (var user in ConnectedUsers)
				server.SendPacket(p, user.EndPoint);
		}
		public void SendToAllExcept(Packet p, User exclusion) {
			foreach (var user in ConnectedUsers)
			{
				if (!user.Equals(exclusion))
					server.SendPacket(p, user.EndPoint);
			}
		}
		public User GetConnectedUser(IPEndPoint ep)
		{
			foreach (User usr in ConnectedUsers)
				if (usr.EndPoint.Equals(ep))
					return usr;

			return null;

		}
		public void OutputAndChat(string text)
		{
			Chat(text);
			Output?.Out(String.Format("[{0} {1}] {2}", "server", DateTime.Now.ToString("HH:mm:ss.ff"), text));
		}

		#region NetworkListenerMethods


		private void OnServerInfoRequested(NetworkMessage msg)
		{

			string[] playerslist = ConnectedUsers.Select(z => z.Username).ToArray();
			server.SendPacket(
				new ServerInformationReplyPacket(
					serverProtocolVersion,
					ServerName,
					ServerMOTD,
					MaxPlayers,
					playerslist
				),
			msg.Sender);
		}

		

		private void OnPlayerConnects(NetworkMessage msg)
		{
			RequestJoinPacket packet = new RequestJoinPacket(msg.Packet.GetBytes());

			foreach (User user in ConnectedUsers)
			{
				// username is taken
				if (user.Username.Equals(packet.RequestedName))
				{
					//	server.SendPacket(new RejectJoinPacket("Username is taken!"), msg.Sender);
					//	break;
				}
			}

			OutputAndChat(String.Format("{0} has joined the server.", packet.RequestedName));

			User newuser = new User
			{
				EndPoint = msg.Sender,
				Username = packet.RequestedName
			};
			ConnectedUsers.Add(newuser);

			// tell them about currently existing players
			foreach (Player player in World.Entities.Where(t=>t is Player))
				SendTo(new PlayerJoinedPacket(player.EntityNetworkID, player.DisplayName, player.Color), newuser);

			// inform player about furniture in the world
			// TODO: create PlaceFurniturePacket(furniture) for conciseness;
			foreach (var furniture in World.Furniture)
				SendTo(new PlaceFurniturePacket((byte)furniture.ID, furniture.FurnitureNetworkID, furniture.Position.X, furniture.Position.Y), newuser);


			Player plr = new Player();
			plr.Color = Color.White;
			plr.DisplayName = packet.RequestedName;
			plr.EntityNetworkID = 666-plr.GetHashCode();
			plr.User = newuser;
			World.Entities.Add(plr);

			PluginManager.CallOnPlayerJoined(plr);

			newuser.PlayerEntity = plr;

			SendTo(new AcceptJoinPacket(newuser.UserNetworkID, plr.EntityNetworkID), newuser);
			SendToAllExcept(new PlayerJoinedPacket(plr.EntityNetworkID, plr.DisplayName, plr.Color), newuser);
			SendTo(new TimeOfDayPacket(World.TimeOfDay), newuser);
		}
		private void OnClientQuit(NetworkMessage msg, User user)
		{
			QuitPacket packet = new QuitPacket(msg.Packet.GetBytes());

			if (FindEntityOfID(packet.EntityID, out Player player))
				World.Entities.Remove(player);

			SendToAll(new PlayerLeftPacket(packet.EntityID));
			ConnectedUsers.Remove(user);
			OutputAndChat(String.Format("{0} has left the server.", user.Username));
		}
		private void OnPlayerPosition(NetworkMessage msg, User user)
		{
			EntityPositionPacket packet = new EntityPositionPacket(msg.Packet.GetBytes());

			if (FindEntityOfID(packet.EntityID, out Player player))
			{
				player.Position = new Vector2(packet.PosX, packet.PosY);
				player.NextPosition = new Vector2(packet.NextX, packet.NextY);
				player.Velocity = new Vector2(packet.VelX, packet.VelY);
			}
		}
		private void OnPlayerState(NetworkMessage msg, User user)
		{
			PlayerStatePacket packet = new PlayerStatePacket(msg.Packet.GetBytes());

			Player plr = user.PlayerEntity;

			plr.Walking = packet.Walking;
			plr.OnGround = packet.OnGround;
			plr.Facing = packet.Facing;

			packet.EntityID = plr.EntityNetworkID;

			SendToAllExcept(packet, user);
		}
		private void OnClientChat(NetworkMessage msg, User user)
		{
			ClientChatMessagePacket chatMessagePacket = new ClientChatMessagePacket(msg.Packet.GetBytes());
			if (PluginManager.CallOnChatMessage(user.PlayerEntity, chatMessagePacket.Message))
			{
				Chat(user.Username + ": " + chatMessagePacket.Message);
			}
		}
		private void OnPlayerPlaceTile(NetworkMessage msg, User user)
		{
			PlaceTilePacket packet = new PlaceTilePacket(msg.Packet.GetBytes());

			Tile t = Tile.FromID(packet.TileID);

			t.TileState = packet.TileState;
			t.Damage = packet.Damage;
			World.SetTile(packet.WorldX, packet.WorldY, t);
			//SendToAll(packet);
		}
		private void OnClientRequestChunk(NetworkMessage msg, User user)
		{
			RequestChunkPacket packet = new RequestChunkPacket(msg.Packet.GetBytes());

			Chunk chunk;
			ChunkCoordinates coords = new ChunkCoordinates(packet.X, packet.Y);

			chunk = World.RetrieveChunk(coords);

			ChunkDownloadPacket chunkpacket = new ChunkDownloadPacket(chunk);
			server.SendPacket(chunkpacket, msg.Sender);
		}

		private void OnPlayerPlaceWall(NetworkMessage msg, User user)
		{
			PlaceWallPacket packet = new PlaceWallPacket(msg.Packet.GetBytes());

			Core.Walls.Wall w = Core.Walls.Wall.FromID(packet.WallID);
			w.Damage = packet.Damage;
			World.SetWall(packet.WorldX, packet.WorldY, w);

		}

		int bombcount = 0;
		private void OnPlayerThrowItemAction(NetworkMessage msg, User user)
		{
			PlayerThrowItemPacket packet = new PlayerThrowItemPacket(msg.Packet.GetBytes());

			Player p = user.PlayerEntity;

			if (packet.Item == ThrownItem.Bomb)
			{
				Bomb bomb = new Bomb();
				bomb.Velocity = packet.ThrownDirection.ToUnitVector()*5;
				bomb.Position = p.Position;
				bomb.NextPosition = p.NextPosition;
				bomb.EntityNetworkID = 1000000 + bombcount;
				World.Entities.Add(bomb);
				SendToAll(new SpawnBombEntityPacket(bomb.EntityNetworkID));
				bombcount++;
			}
		}

		int furniturecount = 0;
		private void OnPlaceFurniture(NetworkMessage msg, User user)
		{
			PlaceFurniturePacket packet = new PlaceFurniturePacket(msg.Packet.GetBytes());

			FurnitureTile f = FurnitureTile.FromID(packet.FurnitureID);
			f.FurnitureNetworkID = 111111+furniturecount;
			f.Position = new Point(packet.WorldX, packet.WorldY);
			if (World.IsCellOccupied(packet.WorldX, packet.WorldY))
				return;

			World.Furniture.Add(f);

			packet.NetworkID = f.FurnitureNetworkID;
			SendToAll(packet);
			furniturecount++;
		}
		private void OnRemoveFurniture(NetworkMessage msg, User user)
		{
			RemoveFurniturePacket packet = new RemoveFurniturePacket(msg.Packet.GetBytes());

		}
		private void OnPlayerClosesDoor(NetworkMessage msg, User user) {
			CloseDoorPacket packet = new CloseDoorPacket(msg.Packet.GetBytes());
			var match = World.Furniture.FirstOrDefault(t=>t.FurnitureNetworkID == packet.FurnitureNetworkID);
			if (match!=null && match is WoodenDoor confirmedDoor)
			{
				confirmedDoor.State = DoorState.Closed;
				SendToAllExcept(packet, user);
			}
		}
		private void OnPlayerOpensDoor(NetworkMessage msg, User user)
		{
			OpenDoorPacket packet = new OpenDoorPacket(msg.Packet.GetBytes());
			var match = World.Furniture.FirstOrDefault(t => t.FurnitureNetworkID == packet.FurnitureNetworkID);
			if (match != null && match is WoodenDoor confirmedDoor)
			{
				if (packet.Direction == HorizontalDirection.Left)
					confirmedDoor.State = DoorState.OpenLeft;
				if (packet.Direction == HorizontalDirection.Right)
				{
					confirmedDoor.State = DoorState.OpenRight;
				}
				SendToAllExcept(packet, user);
			}
		}

		#endregion

		private void ReadIncomingPackets()
		{
			while (server.HaveIncomingMessage())
			{
				NetworkMessage msg = server.GetLatestMessage();

				if (msg.Packet.Type == PacketType.GetServerInfo)
					OnServerInfoRequested(msg);

				if (msg.Packet.Type == PacketType.CRequestJoin)
					OnPlayerConnects(msg);


				User user = GetConnectedUser(msg.Sender);
				if (user == null)
					continue;

				user.KeepAlive = 0;
				
				if (msg.Packet.Type == PacketType.ClientQuit)
					OnClientQuit(msg, user);
				if (msg.Packet.Type == PacketType.ServerEntityPhysicsState)
					OnPlayerPosition(msg, user);
				if (msg.Packet.Type == PacketType.PlayerState)
					OnPlayerState(msg, user);
				if (msg.Packet.Type == PacketType.CChatMessage)
					OnClientChat(msg, user);
				if (msg.Packet.Type == PacketType.PlaceTile)
					OnPlayerPlaceTile(msg, user);
				if (msg.Packet.Type == PacketType.PlaceWall)
					OnPlayerPlaceWall(msg, user);
				if (msg.Packet.Type == PacketType.CRequestChunk)
					OnClientRequestChunk(msg, user);
				if (msg.Packet.Type == PacketType.PlayerThrowItemAction)
					OnPlayerThrowItemAction(msg, user);
				if (msg.Packet.Type == PacketType.PlaceFurniture)
					OnPlaceFurniture(msg, user);
				if (msg.Packet.Type == PacketType.RemoveFurniture)
					OnRemoveFurniture(msg, user);
				if (msg.Packet.Type == PacketType.OpenDoor)
					OnPlayerOpensDoor(msg, user);
				if (msg.Packet.Type == PacketType.CloseDoor)
					OnPlayerClosesDoor(msg, user);
			}
		}

		private IEntity FindEntityOfID(int id)
		{
			foreach (var entity in World.Entities)
				if (entity.EntityNetworkID == id)
					return entity;
			return null;
		}

		private bool FindEntityOfID(int id, out IEntity e)
		{
			foreach (var entity in World.Entities) {
				if (entity.EntityNetworkID == id)
				{
					e = entity;
					return true;
				}
			}
			e = null;
			return false;
		}

		private bool FindEntityOfID<T>(int id, out T e) where T : Entity, IEntity
		{
			foreach (var entity in World.Entities)
			{
				if (entity.EntityNetworkID == id)
				{
					e = (T)entity;
					return true;
				}
			}
			e = null;
			return false;
		}

		float tileticker = 0;


		public void Update(GameTime gt)
		{
			float delta = (float)gt.ElapsedGameTime.TotalSeconds;


			foreach(var user in ConnectedUsers.ToArray())
			{
				if (user.Kicked)
				{
					Output?.Out("[server " + DateTime.Now.ToString("HH:mm:ss.ff") + "] Kicked user: "+user.DisconnectReason);
					SendTo(new KickPacket(user.DisconnectReason), user);
					SendToAll(new PlayerLeftPacket(user.PlayerEntity.EntityNetworkID));
					World.Entities.Remove(user.PlayerEntity);
					ConnectedUsers.Remove(user);
				}


				user.KeepAlive += delta;

				if (user.KeepAlive > 10.0f)
				{
					user.Kick("Network Timeout");
					
				}
			}

			ticker += delta;

			if (ticker > 1/ 20.0f)
			{
				ticker = 0;
				foreach(var entity in World.Entities)
				{


					if (entity is Player player)
					{
						var pos = new EntityPositionPacket(player.EntityNetworkID, 
							player.Position.X, player.Position.Y,
							player.Velocity.X, player.Velocity.Y,
							player.NextPosition.X, player.NextPosition.Y);
						//Output?.Out("DT_OUT: " + pos.ToString());
						SendToAll(pos);
					}
					if (entity is Bomb bomb && !entity.Dead)
					{
						var pos = new EntityPositionPacket(bomb.EntityNetworkID,
							bomb.Position.X, bomb.Position.Y,
							bomb.Velocity.X, bomb.Velocity.Y,
							bomb.NextPosition.X, bomb.NextPosition.Y);
						//Output?.Out("DT_OUT: " + pos.ToString());
						SendToAll(pos);
					}
				}
			}

			ReadIncomingPackets();
			World.Update(gt);
			tileticker += delta;

			if (tileticker > (1/5.0f))
			{
				tileticker = 0;
				foreach (var kvp in World.Chunks)
				{
					Chunk c = kvp.Value;

					for (int x = 0; x < Chunk.ChunkSize; x++)
					{
						for (int y = 0; y < Chunk.ChunkSize; y++)
						{
							if (c.NetworkUpdated[x, y] == true)
							{
								c.NetworkUpdated[x, y] = false;
								var t = c.Tiles[x, y];
								var w = c.Walls[x, y];
								SendToAll(new PlaceTilePacket(t.ID, t.TileState, t.Damage, (c.Coordinates.X * Chunk.ChunkSize) + x, (c.Coordinates.Y * Chunk.ChunkSize) + y));
								SendToAll(new PlaceWallPacket(w.ID, 0, w.Damage, (c.Coordinates.X * Chunk.ChunkSize) + x, (c.Coordinates.Y * Chunk.ChunkSize) + y));
							}
						}
					}
				}
			}
		}

		public void ClientRun()
		{
			server.Run();
		}

		public void Run()
		{
			server.Run();

			Stopwatch timing = new Stopwatch();

			TimeSpan runningTotal = new TimeSpan();
			while (Running)
			{
				timing.Stop();
				
				runningTotal += timing.Elapsed;
				GameTime gt = new GameTime(runningTotal, timing.Elapsed);
				timing.Reset();
				timing.Start();
				Update(gt);

				Thread.Sleep(TickRate);
			}
		}
	}
}
