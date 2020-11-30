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

namespace CaveGame.Server
{
	public interface IPluginAPIServer: IGameServer
	{
		DateTime Time { get; }
		IMessageOutlet Output { get; }

	}

	public class ServerCommand
	{
		public delegate void SCommandHandler(ServerCommand command, params string[] args);
		public string Keyword;
		public string Description;
		public List<string> Args;

		public ServerCommand(string cmd, string desc, List<string> args)
		{
			Keyword = cmd;
			Description = desc;
			Args = args;
		}

	}

	public class CommandEventArgs : LuaEventArgs
    {

		public Player Sender { get; set; }
		public string Command { get; private set; }
		public List<string> Arguments { get; private set; }

		public CommandEventArgs(string cmd, List<string> args)
        {
			Command = cmd;
			Arguments = args;
        }
		public CommandEventArgs(string cmd, List<string> args, Player player)
		{
			Command = cmd;
			Arguments = args;
			Sender = player;
		}
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

	public class GameServer : IPluginAPIServer, IGameServer
	{

		

		public LuaEvent<PlayerEventArgs> OnPlayerJoinedServer = new LuaEvent<PlayerEventArgs>();
		public LuaEvent<PlayerEventArgs> OnPlayerLeftServer = new LuaEvent<PlayerEventArgs>();
		public LuaEvent<PlayerChatMessageEventArgs> OnChatMessageFromClient = new LuaEvent<PlayerChatMessageEventArgs>();
		public LuaEvent<CommandEventArgs> OnServerCommand = new LuaEvent<CommandEventArgs>();

		public List<ServerCommand> Commands { get; set; }
		public void BindCommand(ServerCommand command)
		{
			Commands.Add(command);
		}
		public void BindCommand(string cmd, string descr, LuaTable args)
		{
			var argsList = new List<string>();

			foreach (var arg in args.Values)
			{
				argsList.Add((string)arg);
			}

			ServerCommand command = new ServerCommand(cmd, descr, argsList);
			Commands.Add(command);
		}

		public PluginManager PluginManager;
		public string Information = "";
		public bool Running { get; set; }

		public DateTime Time => DateTime.Now;
		
		public void Chat(string msg) => SendToAll(new ServerChatMessagePacket(msg));
		public void Chat(string msg, Color color) => SendToAll(new ServerChatMessagePacket(msg, color));
		public void Message(User user, string msg) => SendTo(new ServerChatMessagePacket(msg), user);
		public void Message(User user, string msg, Color color) => SendTo(new ServerChatMessagePacket(msg, color), user);


		public IMessageOutlet Output
		{
			get { return server.Output; }
			set { server.Output = value; }
		}

		private NetworkServer server;

		public void OnCommand(string msg)
		{
			Output.Out("> "+msg);

			string cleaned = msg.Trim();

			string[] keywords = cleaned.Split(' ');

			foreach (ServerCommand cmdDef in Commands)
			{
				if (keywords[0] == cmdDef.Keyword)
				{
					//cmdDef.InvokeCommand(keywords.Skip(1).ToArray());
					OnServerCommand.Invoke(new CommandEventArgs(cmdDef.Keyword, keywords.Skip(1).ToList()));
					return;
				}
			}
			Output.Out("No command " + keywords[0] + " found!", new Color(1.0f, 0, 0));
		}
		

		public EntityManager EntityManager { get; private set; }
		public int TickRate { get; private set; }
		public List<User> ConnectedUsers { get; private set; }

		

		public ServerWorld World { get; private set; }
		public string ServerName { get; private set; }
		public string ServerMOTD { get; private set; }
		public int MaxPlayers { get; private set; }
		

        float ticker;

		IEntityManager IGameServer.EntityManager => EntityManager;
		IServerWorld IGameServer.World => World;

		public void SpawnEntity(IEntity entity)
		{
			entity.EntityNetworkID = EntityManager.GetNextEntityNetworkID();
			World.SpawnEntity(entity);
		}
		public void LoadPlugins() => PluginManager.LoadPlugins(this);

		public GameServer(ServerConfig config) {

			Commands = new List<ServerCommand>();
			ServerName = config.ServerName;
			ServerMOTD = config.ServerMOTD;
			MaxPlayers = config.MaxPlayers;

			TickRate = config.TickRate;
			ticker = 0;
			server = new NetworkServer(config.Port);
			ConnectedUsers = new List<User>();
			World = new ServerWorld(new WorldConfiguration { Name = config.World, Seed = 420 });
			World.Server = this;

			EntityManager = new EntityManager();
			PluginManager = new PluginManager();
			
		}

		

		public void Start()
		{
			server.Start();
			Running = true;
		}

		public void Shutdown()
		{
			PluginManager.UnloadPlugins();
			Console.WriteLine("Shutting Down. Not Saving while testing worldgen");
			//World.SaveData();
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
					Globals.ProtocolVersion,
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


			Player plr = new Player();
			plr.Color = Color.White;
			plr.DisplayName = packet.RequestedName;
			plr.User = newuser;
			SpawnEntity(plr);

			//PluginManager.CallOnPlayerJoined(plr);
			OnPlayerJoinedServer.Invoke(new PlayerEventArgs(plr));
			newuser.PlayerEntity = plr;

			SendTo(new AcceptJoinPacket(newuser.UserNetworkID, plr.EntityNetworkID), newuser);
			SendToAllExcept(new PlayerJoinedPacket(plr.EntityNetworkID, plr.DisplayName, plr.Color), newuser);
			SendTo(new TimeOfDayPacket(World.TimeOfDay), newuser);
		}
		private void OnClientQuit(NetworkMessage msg, User user)
		{
			QuitPacket packet = new QuitPacket(msg.Packet.GetBytes());

			

			if (World.FindEntityOfID(packet.EntityID, out Player player))
            {
				OnPlayerLeftServer.Invoke(new PlayerEventArgs(player));
				World.Entities.Remove(player);
			}
				

			SendToAll(new PlayerLeftPacket(packet.EntityID));
			ConnectedUsers.Remove(user);
			OutputAndChat(String.Format("{0} has left the server.", user.Username));
		}
		private void OnPlayerPosition(NetworkMessage msg, User user)
		{
			EntityPositionPacket packet = new EntityPositionPacket(msg.Packet.GetBytes());

			if (World.FindEntityOfID(packet.EntityID, out Player player))
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
			if (OnChatMessageFromClient.Invoke(new PlayerChatMessageEventArgs(user.PlayerEntity, chatMessagePacket.Message)))
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

			Core.Game.Walls.Wall w = Core.Game.Walls.Wall.FromID(packet.WallID);
			w.Damage = packet.Damage;
			World.SetWall(packet.WorldX, packet.WorldY, w);

		}

		int bombcount = 0;
		float bombVelocity = 3.5f;
		private void OnPlayerThrowItemAction(NetworkMessage msg, User user)
		{
			PlayerThrowItemPacket packet = new PlayerThrowItemPacket(msg.Packet.GetBytes());

			Player p = user.PlayerEntity;

			if (packet.Item == ThrownItem.Bomb)
			{
                Core.Game.Entities.Bomb bomb = new Core.Game.Entities.Bomb
                {
					Velocity = packet.ThrownDirection.ToUnitVector() * bombVelocity,
					Position = p.Position,
					NextPosition = p.NextPosition,
				};
				
				SpawnEntity(bomb);
				SendToAll(new SpawnBombEntityPacket(bomb.EntityNetworkID));
				bombcount++;
			}
		}

		int furniturecount = 0;
		private void OnPlaceFurniture(NetworkMessage msg, User user)
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
			furniturecount++;
		}
		private void OnRemoveFurniture(NetworkMessage msg, User user)
		{
			RemoveFurniturePacket packet = new RemoveFurniturePacket(msg.Packet.GetBytes());
			var match = World.Furniture.FirstOrDefault(t => t.FurnitureNetworkID == packet.FurnitureNetworkID);
			if (match != null)
			{
				World.Furniture.Remove(match);
				SendToAllExcept(packet, user);
			}
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

        private void OnPlayerDamageTile(NetworkMessage msg, User user)
        {
            DamageTilePacket packet = new DamageTilePacket(msg.Packet.GetBytes());

			var tile = World.GetTile(packet.Position.X, packet.Position.Y);

			tile.Damage += (byte)packet.Damage;
			Debug.WriteLine("tile damage:" + tile.Damage);
			SendToAllExcept(packet, user);
			if (tile.Damage >= tile.Hardness)
            {
				Debug.WriteLine("Tile Break");
				tile.Drop(this, World, packet.Position);
				World.SetTile(packet.Position.X, packet.Position.Y, new Air());
				//SendToAll(new PlaceTilePacket(0, 0, 0, packet.Position.X, packet.Position.Y));
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
						SendToAll(new SpawnWurmholeEntityPacket(wurm.EntityNetworkID));

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
						SendToAll(new SpawnBombEntityPacket(bomb.EntityNetworkID));

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
				if (msg.Packet.Type == PacketType.DamageTile)
					OnPlayerDamageTile(msg, user);
				if (msg.Packet.Type == PacketType.AdminCommand)
					OnAdminCommandInput(msg, user);
			}
		}



		float tileticker = 0;

		protected void InternalUserKick(User user)
		{
			Output?.Out("[server " + DateTime.Now.ToString("HH:mm:ss.ff") + "] Kicked user: " + user.DisconnectReason);
			SendTo(new KickPacket(user.DisconnectReason), user);
			SendToAll(new PlayerLeftPacket(user.PlayerEntity.EntityNetworkID));
			World.Entities.Remove(user.PlayerEntity);
			ConnectedUsers.Remove(user);
		}

		protected void DispatchUserMessages(User user)
        {
			if (user.DispatcherHasMessage())
            {
				Packet p = user.PopDispatcherQueue();
				SendTo(p, user);
            }
        }


		public void Update(GameTime gt)
		{
			Information = String.Format("{0} Players", ConnectedUsers.Count);
			float delta = (float)gt.ElapsedGameTime.TotalSeconds;


			ConnectedUsers.Where(u => u.Kicked == true).ForEach(InternalUserKick);
			ConnectedUsers.ForEach(u => DispatchUserMessages(u));

			foreach(var user in ConnectedUsers.ToArray())
			{

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
					if (entity is Wurmhole wurmhole && !entity.Dead)
                    {
						var pos = new EntityPositionPacket(wurmhole.EntityNetworkID,
							wurmhole.Position.X, wurmhole.Position.Y,
							wurmhole.Velocity.X, wurmhole.Velocity.Y,
							wurmhole.NextPosition.X, wurmhole.NextPosition.Y);
						SendToAll(pos);


						// TODO: remove dipshit hack;
						if (wurmhole.Triggered)
                        {
							if (wurmhole.TriggerNetworkHandled == false)
                            {
								wurmhole.TriggerNetworkHandled = true;
								SendToAll(new TriggerWurmholeEntityPacket(wurmhole.EntityNetworkID));
                            }
                        }
					}

					if (entity is Player player)
					{
						var pos = new EntityPositionPacket(player.EntityNetworkID, 
							player.Position.X, player.Position.Y,
							player.Velocity.X, player.Velocity.Y,
							player.NextPosition.X, player.NextPosition.Y);
						//Output?.Out("DT_OUT: " + pos.ToString());
						SendToAll(pos);
					}
					if (entity is Core.Game.Entities.Bomb bomb && !entity.Dead)
					{
						var pos = new EntityPositionPacket(bomb.EntityNetworkID,
							bomb.Position.X, bomb.Position.Y,
							bomb.Velocity.X, bomb.Velocity.Y,
							bomb.NextPosition.X, bomb.NextPosition.Y);
						//Output?.Out("DT_OUT: " + pos.ToString());
						SendToAll(pos);
					}
					if (entity is ItemstackEntity itementity && !entity.Dead)
					{
						var pos = new EntityPositionPacket(itementity.EntityNetworkID,
							itementity.Position.X, itementity.Position.Y,
							itementity.Velocity.X, itementity.Velocity.Y,
							itementity.NextPosition.X, itementity.NextPosition.Y);
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
