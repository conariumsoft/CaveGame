#define PACKETDEBUG

using CaveGame.Core;
using CaveGame.Core.FileUtil;
using CaveGame.Core.Furniture;
using CaveGame.Core.Game.Entities;
using CaveGame.Core.LuaInterop;
using CaveGame.Core.Network;
using CaveGame.Core.Game.Tiles;
using DataManagement;
using KeraLua;
using Microsoft.Xna.Framework;
using NLua;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CaveGame.Core.Inventory;
using CaveGame.Core.Network.Packets;
using CaveGame.Core.Generic;

namespace CaveGame.Server
{
	public interface IPluginAPIServer : IGameServer
	{
		DateTime Time { get; }
		IMessageOutlet Output { get; }


	}
	public class PlayerChatMessageEventArgs : LuaEventArgs
    {
		public Player Player { get; private set; }
		public string Message { get; private set; }
		public PlayerChatMessageEventArgs(Player player, string message)
        {
			Player = player;
			Message = message;
        }
    }

	public class PlayerEventArgs : LuaEventArgs
	{
		public Player Player {get; private set;}
		public PlayerEventArgs(Player player)
        {
			Player = player;
        }
    }


	public class EntityManager : IEntityManager
    {
		public EntityManager() { }

		static int entityNetworkID;
		public int GetNextEntityNetworkID()
        {
			var current = entityNetworkID;
			entityNetworkID++;
			return current;
        }
    }



	public class GameServer : IGameServer
	{
		public delegate void ThrowDelegate(Rotation rotation, Player player);
		public delegate void NetworkListener(NetworkMessage message, User user);

		public bool Running { get; set; }

		public DateTime Time => DateTime.Now;
		
		public void Chat(string msg) => SendToAll(new ServerChatMessagePacket(msg));
		public void Chat(string msg, Color color) => SendToAll(new ServerChatMessagePacket(msg, color));
		public void Message(User user, string msg) => SendTo(new ServerChatMessagePacket(msg), user);
		public void Message(User user, string msg, Color color) => SendTo(new ServerChatMessagePacket(msg, color), user);


		public Entity GetEntity(int networkID) => (Entity)World.FindEntityOfID(networkID);

		public IMessageOutlet Output
		{
			get { return server.Output; }
			set { server.Output = value; }
		}

		private NetworkServer server;


		public EntityManager EntityManager { get; private set; }
		public int TickRate { get; private set; }
		public List<User> ConnectedUsers { get; private set; }

		public ServerWorld World { get; private set; }
		public string ServerName { get; private set; }
		public string ServerMOTD { get; private set; }
		public int MaxPlayers { get; private set; }
		


		IEntityManager IGameServer.EntityManager => EntityManager;
		IServerWorld IGameServer.World => World;

		public void SpawnEntity(IEntity entity)
		{
			entity.EntityNetworkID = EntityManager.GetNextEntityNetworkID();
			World.SpawnEntity(entity);
		}

		RepeatingIntervalTask[] ServerTasks;
		public GameServer(ServerConfig config, WorldMetadata worldmdt) {
			InitNetworkEvents();

			ServerName = config.ServerName;
			ServerMOTD = config.ServerMOTD;
			MaxPlayers = config.MaxPlayers;

			TickRate = config.TickRate;
			
			server = new NetworkServer(config.Port);
			ConnectedUsers = new List<User>();
			World = new ServerWorld(worldmdt);
			World.Server = this;

			EntityManager = new EntityManager();

			ServerTasks = new RepeatingIntervalTask[]
			{
				new (ReplicateNetworkEntitiesPhysicalStates, 1.0f/20.0f),

			};

		}


		private Dictionary<PacketType, NetworkListener> NetworkEvents;
		private void InitNetworkEvents() => NetworkEvents = new Dictionary<PacketType, NetworkListener>()
		{
			[PacketType.cLogout] = OnClientLogout,
			[PacketType.cAdminCommand] = OnAdminCommandInput,
			[PacketType.cSendChatMessage] = OnClientChat,
			[PacketType.cRequestChunk] = OnClientRequestChunk,
			[PacketType.netPlaceFurniture] = OnClientPlaceFurniture,
			[PacketType.cDamageFurniture] = OnClientDamageFurniture,
			[PacketType.netDamageTile] = OnClientDamageTile,
			[PacketType.netPlaceTile] = OnClientPlaceTile,
			[PacketType.netDamageWall] = OnClientDamageWall,
			[PacketType.netPlaceWall] = OnClientPlaceWall,
			[PacketType.cThrowItem] = OnClientThrowItem,
			[PacketType.netCloseDoor] = OnClientCloseDoor,
			[PacketType.netOpenDoor] = OnClientOpenDoor,
			[PacketType.netEntityPhysicsUpdate] = OnPlayerPosition,
			[PacketType.netPlayerState] = OnPlayerState,
		};


		#region NetworkListenerMethods

		protected virtual void OnPing(NetworkMessage msg, User user)=> SendTo(msg.Packet, user); // response
		private void OnClientDamageWall(NetworkMessage message, User user)
		{
			throw new NotImplementedException();
		}
		private void HandshakeResponse(NetworkMessage msg)
		{

			string[] playerslist = ConnectedUsers.Select(z => z.Username).ToArray();
			server.SendPacket(
				new HandshakeResponsePacket(
					Globals.ProtocolVersion,
					ServerName,
					ServerMOTD,
					MaxPlayers,
					playerslist
				),
			msg.Sender);
		}
		protected virtual void OnPlayerConnects(User newuser, Player plr)
        {
			SendTo(new AcceptJoinPacket(newuser.UserNetworkID, plr.EntityNetworkID), newuser);
			SendToAllExcept(new PlayerJoinedPacket(plr.EntityNetworkID, plr.DisplayName, plr.Color), newuser);
			SendTo(new TimeOfDayPacket(World.TimeOfDay), newuser);
		}
		protected virtual void OnClientLoginSuccess(NetworkMessage msg)
        {

        }
		protected virtual void OnClientRequestLogin(NetworkMessage msg)
		{
			RequestJoinPacket packet = new RequestJoinPacket(msg.Packet.GetBytes());

			foreach (User user in ConnectedUsers)
			{
				// username is taken
				if (user.Username.Equals(packet.RequestedName))
				{
					server.SendPacket(new RejectJoinPacket("Username is taken!"), msg.Sender);
					break;
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

			int thisPeerNetworkID = EntityManager.GetNextEntityNetworkID();

			Player plr = new Player();
			plr.Color = Color.White;
			plr.DisplayName = packet.RequestedName;
			plr.User = newuser;
			plr.EntityNetworkID = thisPeerNetworkID;
			World.SpawnEntity(plr);

			
			//PluginManager.CallOnPlayerJoined(plr);
			
			newuser.PlayerEntity = plr;
			newuser.UserNetworkID = thisPeerNetworkID;
			OnPlayerConnects(newuser, plr);


		}
		protected virtual void OnClientLogout(NetworkMessage msg, User user)
		{
			DisconnectPacket packet = new DisconnectPacket(msg.Packet.GetBytes());

			if (World.FindEntityOfID(packet.LeavingEntityID, out Player player))
				World.Entities.Remove(player);
				

			SendToAll(new PlayerLeftPacket(packet.LeavingEntityID));
			ConnectedUsers.Remove(user);
		}
		private void OnPlayerPosition(NetworkMessage msg, User user) // player tells us their state
		{
			// TODO: make server more authoritative, prevent cheating
			EntityPhysicsStatePacket packet = new EntityPhysicsStatePacket(msg.Packet.GetBytes());

			if (World.FindEntityOfID(packet.EntityID, out Player player))
			{
				player.Position = packet.Position;
				player.NextPosition = packet.NextPosition;
				player.Velocity = packet.Velocity;
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
		protected virtual void OnClientChat(NetworkMessage msg, User user)
		{
			ClientChatMessagePacket chatMessagePacket = new ClientChatMessagePacket(msg.Packet.GetBytes());
			Chat(user.Username + ": " + chatMessagePacket.Message);
		}
		private void OnClientPlaceTile(NetworkMessage msg, User user)
		{
			PlaceTilePacket packet = new PlaceTilePacket(msg.Packet.GetBytes());

			Tile t = Tile.FromID(packet.TileID);

			t.TileState = packet.TileState;
			t.Damage = packet.Damage;
			World.SetTile(packet.WorldX, packet.WorldY, t);
			SendToAll(packet);
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

		private void OnClientPlaceWall(NetworkMessage msg, User user)
		{
			PlaceWallPacket packet = new PlaceWallPacket(msg.Packet.GetBytes());

			Core.Game.Walls.Wall w = Core.Game.Walls.Wall.FromID(packet.WallID);
			w.Damage = packet.Damage;
			World.SetWall(packet.WorldX, packet.WorldY, w);

		}

		float bombVelocity = 3.5f;
		float arrowVelocity = 3.0f;

		private TEntity SpawnThrowable<TEntity>(Rotation dir, Vector2 startpos, float vel, NetEntityType entityType) where TEntity: IEntity, IPhysicsEntity, new()
        {
			TEntity entity = new TEntity
			{ Velocity = dir.ToUnitVector() * vel, Position = startpos, NextPosition = startpos};
			SpawnEntity(entity);
			SendToAll(new SpawnEntityGenericPacket(entity.EntityNetworkID, entityType));
			return entity;
		}

		private void OnThrowBomb(Rotation dir, Vector2 pos, float force) => SpawnThrowable<Bomb>(dir, pos, force, NetEntityType.Bomb);
		private void OnShootArrow(Rotation dir, Vector2 pos, float force) => SpawnThrowable<Arrow>(dir, pos, force, NetEntityType.Arrow);

		private void OnClientThrowItem(NetworkMessage msg, User user)
		{
			PlayerThrowItemPacket packet = new PlayerThrowItemPacket(msg.Packet.GetBytes());
			Player p = user.PlayerEntity;
			if (packet.Item == ThrownItem.Bomb)
				OnThrowBomb(packet.ThrownDirection, p.Position, bombVelocity);
			if (packet.Item == ThrownItem.Arrow)
				OnShootArrow(packet.ThrownDirection, p.Position, arrowVelocity);
		}

		private void OnClientPlaceFurniture(NetworkMessage msg, User user)
		{
			PlaceFurniturePacket packet = new PlaceFurniturePacket(msg.Packet.GetBytes());

			FurnitureTile f = FurnitureTile.FromID(packet.FurnitureID);
			f.FurnitureNetworkID = EntityManager.GetNextEntityNetworkID(); // don't worry about it LOL
			f.Position = new Point(packet.WorldX, packet.WorldY);
			if (World.IsCellOccupied(packet.WorldX, packet.WorldY))
				return;

			World.Furniture.Add(f);

			packet.NetworkID = f.FurnitureNetworkID;
			SendToAll(packet);
		}
		private void OnClientDamageFurniture(NetworkMessage msg, User user)
		{
			RemoveFurniturePacket packet = new RemoveFurniturePacket(msg.Packet.GetBytes());
			var match = World.Furniture.FirstOrDefault(t => t.FurnitureNetworkID == packet.FurnitureNetworkID);
			if (match != null)
			{
				World.Furniture.Remove(match);
				SendToAllExcept(packet, user);
			}
		}
		private void OnClientCloseDoor(NetworkMessage msg, User user) {
			CloseDoorPacket packet = new CloseDoorPacket(msg.Packet.GetBytes());
			var match = World.Furniture.FirstOrDefault(t=>t.FurnitureNetworkID == packet.FurnitureNetworkID);
			if (match!=null && match is WoodenDoor confirmedDoor)
			{
				confirmedDoor.State = DoorState.Closed;
				SendToAllExcept(packet, user);
			}
		}
		private void OnClientOpenDoor(NetworkMessage msg, User user)
		{
			OpenDoorPacket packet = new OpenDoorPacket(msg.Packet.GetBytes());
			var match = World.Furniture.FirstOrDefault(t => t.FurnitureNetworkID == packet.FurnitureNetworkID);
			if (match != null && match is WoodenDoor confirmedDoor)
			{
				if (packet.Direction == Direction.Left)
					confirmedDoor.State = DoorState.OpenLeft;
				if (packet.Direction == Direction.Right)
				{
					confirmedDoor.State = DoorState.OpenRight;
				}
				SendToAllExcept(packet, user);
			}
		}

        private void OnClientDamageTile(NetworkMessage msg, User user)
        {
            DamageTilePacket packet = new DamageTilePacket(msg.Packet.GetBytes());

			var tile = World.GetTile(packet.Position.X, packet.Position.Y);

			tile.Damage += (byte)packet.Damage;
			SendToAll(packet);
			if (tile.Damage >= tile.Hardness)
            {
				tile.Drop(this, World, packet.Position);
				World.SetTile(packet.Position.X, packet.Position.Y, new Air());
				SendToAll(new PlaceTilePacket(0, 0, 0, packet.Position.X, packet.Position.Y));
            }
			
        }

		private void OnAdminCommandInput(NetworkMessage msg, User user)
        {
			AdminCommandPacket packet = new AdminCommandPacket(msg.Packet.GetBytes());

			//Debug.WriteLine("GOT ADMIN COMMAND {0} {1}", packet.Command, packet.PlayerNetworkID);

			var player = (Player)World.FindEntityOfID(packet.PlayerNetworkID);

			if (packet.Command == "sv_summon")
            {
				Vector2 spawnPos = player.NextPosition;
				if (packet.Arguments.Length > 0)
                {
					if (packet.Arguments[0] == "wurmhole")
                    {
						var wurm = new Wurmhole();
						wurm.EntityNetworkID = EntityManager.GetNextEntityNetworkID();
						


						if (packet.Arguments.Length == 3)
                        {
							bool xSuccess = Single.TryParse(packet.Arguments[1], out float x);
							bool ySuccess = Single.TryParse(packet.Arguments[2], out float y);
							if (xSuccess && ySuccess)
								spawnPos = new Vector2(x, y);
                        }
						wurm.NextPosition = spawnPos;
						World.Entities.Add(wurm);
						SendToAll(new SpawnEntityGenericPacket(wurm.EntityNetworkID, NetEntityType.Wurmhole));

						return;
                    }
					if (packet.Arguments[0] == "bomb")
					{
						var bomb = new Core.Game.Entities.Bomb();
						bomb.EntityNetworkID = EntityManager.GetNextEntityNetworkID();



						if (packet.Arguments.Length == 3)
						{
							bool xSuccess = Single.TryParse(packet.Arguments[1], out float x);
							bool ySuccess = Single.TryParse(packet.Arguments[2], out float y);
							if (xSuccess && ySuccess)
								spawnPos = new Vector2(x, y);
						}
						bomb.NextPosition = spawnPos;
						World.Entities.Add(bomb);
						SendToAll(new SpawnEntityGenericPacket(bomb.EntityNetworkID, NetEntityType.Bomb));

						return;
					}
				}
            }

        }

		#endregion

		private void ReadIncomingPackets()
		{
			while (server.HaveIncomingMessage())
			{
				NetworkMessage msg = server.GetLatestMessage();

				if (msg.Packet.Type == PacketType.cHandshake)
					HandshakeResponse(msg);
				if (msg.Packet.Type == PacketType.cRequestLogin)
					OnClientRequestLogin(msg);
				if (msg.Packet.Type == PacketType.cConfirmLogin)
					OnClientLoginSuccess(msg);


				User user = GetConnectedUser(msg.Sender);
				if (user == null)
					continue;

				user.KeepAlive = 0;

				foreach (var ev in NetworkEvents)
					if (ev.Key == msg.Packet.Type)
						ev.Value.Invoke(msg, user);
				#region old shit
				/*if (msg.Packet.Type == PacketType.ClientQuit)
					OnClientLogout(msg, user);
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
				if (msg.Packet.Type == PacketType.DamageTile)
					OnPlayerDamageTile(msg, user);
				if (msg.Packet.Type == PacketType.cAdminCommand)
					OnAdminCommandInput(msg, user);*/
				#endregion
			}
        }

		float tileticker = 0;

		protected virtual void InternalUserKick(User user)
		{
			Output?.Out("[server " + DateTime.Now.ToString("HH:mm:ss.ff") + "] Kicked user: " + user.DisconnectReason);
			SendTo(new KickPacket(user.DisconnectReason), user);
			SendToAll(new PlayerLeftPacket(user.PlayerEntity.EntityNetworkID));
			World.Entities.Remove(user.PlayerEntity);
			ConnectedUsers.Remove(user);
		}


		// generic entity physics replication
		protected virtual void ReplicateNetworkEntitiesPhysicalStates()
		{
			foreach (var entity in World.Entities)
			{
				if (entity.Dead)
					continue;

				SendToAll(new EntityPhysicsStatePacket(entity));


				if (entity is Wurmhole wurmhole && !entity.Dead)
				{
					// TODO: remove dipshit hack;
					if (wurmhole.Provoked && wurmhole.TriggerNetworkHandled == false)
					{
						wurmhole.TriggerNetworkHandled = true;
						SendToAll(new ProvokeEntityGenericPacket(wurmhole.EntityNetworkID));
					}
				}
			}
		}

		public virtual void HandleUserConnectionStates()
        {

        }

		public virtual void Update(GameTime gt)
		{
			ReadIncomingPackets();
			HandleUserConnectionStates();

			ServerTasks.ForEach(t => t.Update(gt));

			ConnectedUsers.ForEach(u => u.SendDispatchMessages(this));
			ConnectedUsers.ForEach(u => u.KeepAlive += gt.GetDelta());
			ConnectedUsers.Where(u => u.KeepAlive > 10.0f).ForEach(u => u.Kick("Network Timeout"));
			ConnectedUsers.ToArray().Where(u => u.Kicked == true).ForEach(InternalUserKick); // InvalidOperationException
			
			World.Update(gt);
			server.Update(gt);
		}

		public void Start()
		{
			server.Start();
			Running = true;

			Task.Run(GameserverThreadLoop);
		}
		public virtual void Shutdown()
		{
			GameConsole.Log("Shutting Down. Not Saving while testing worldgen");
			World.SaveData();
			Thread.Sleep(100);
		}
		public void SendTo(Packet p, User user) => server.SendPacket(p, user.EndPoint);
		public void SendToAll(Packet p)
		{
			foreach (var user in ConnectedUsers)
				server.SendPacket(p, user.EndPoint);
		}
		public void SendToAllExcept(Packet p, User exclusion)
		{
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

		public void GameserverThreadLoop()
		{
			

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