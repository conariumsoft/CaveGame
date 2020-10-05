#define PACKETDEBUG

using CaveGame.Core;
using CaveGame.Core.Entities;
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CaveGame.Server
{

	public class GameServer
	{
		public IMessageOutlet Output
		{
			get { return server.Output; }
			set
			{
				server.Output = value;
			}
		}

		private NetworkServer server;

		public bool Running
		{
			get;
			set;
		}

		public int TickRate { get; private set; }

		public List<User> ConnectedUsers { get; private set; }
		public ServerWorld World { get; private set; }
		float ticker;

		public GameServer(ServerConfig config) {
			TickRate = config.TickRate;
			ticker = 0;
			server = new NetworkServer(config.Port);
			ConnectedUsers = new List<User>();
			World = new ServerWorld(new WorldConfiguration { Name = config.World, Seed = 666 });
			World.Server = this;
		}

		

		public void Start()
		{
			server.Start();
			Running = true;
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
			foreach(User usr in ConnectedUsers)
				if (usr.EndPoint.Equals(ep))
					return usr;

			return null;

		}


		Random rng = new Random();
		private void StructureGeneration(IGameWorld world, int x, int y)
		{

			if (rng.Next(64) == 2 && world.GetTile(x, y) is Grass && world.GetTile(x, y - 1) is Air)
			{

				
				for (int dx = -3; dx <= 3; dx++)
				{
					for (int dy = -3; dy <= 3; dy++)
					{
						world.SetTile(dx+x, dy+y - 7, new Leaves());
					}
				}
				world.SetTile(x, y - 1, new Log());
				world.SetTile(x, y - 2, new Log());
				world.SetTile(x, y - 3, new Log());
				world.SetTile(x, y - 4, new Log());
				world.SetTile(x, y - 5, new Log());

				//world.SetTile(x, y - 1, new Stone());
			}
		}


		private void OnPlayerConnects(NetworkMessage msg)
		{
			Output?.Out("[Server] req");
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

			User newuser = new User
			{
				EndPoint = msg.Sender,
				Username = packet.RequestedName
			};
			ConnectedUsers.Add(newuser);

			// tell them about currently existing players
			foreach (var entity in World.Entities)
			{
				if (entity is Player player)
				{
					SendTo(new PlayerJoinedPacket(player.EntityNetworkID, player.DisplayName, player.Color), newuser);
				}
			}


			Player plr = new Player();
			plr.Color = Color.White;
			plr.DisplayName = packet.RequestedName;
			plr.EntityNetworkID = plr.GetHashCode();
			World.Entities.Add(plr);

			server.SendPacket(new AcceptJoinPacket(newuser.UserNetworkID, plr.EntityNetworkID), msg.Sender);
			SendToAllExcept(new PlayerJoinedPacket(plr.EntityNetworkID, plr.DisplayName, plr.Color), newuser);
		}

		

		private void ReadIncomingPackets()
		{
			// TODO: Experiment with direct dequeue
			while (server.HaveIncomingMessage())
			{
				NetworkMessage msg = server.GetLatestMessage();

				if (msg.Packet.Type == PacketType.CRequestJoin)
					OnPlayerConnects(msg);


				//
				var user = GetConnectedUser(msg.Sender);

				if (user == null)
				{
					continue;
				} else
				{
					user.KeepAlive = 0;
					
				}
				if (msg.Packet.Type == PacketType.ClientQuit)
				{
					QuitPacket packet = new QuitPacket(msg.Packet.GetBytes());


					var plr = FindEntityOfID(packet.EntityID);

					if (plr != null)
					{
						World.Entities.Remove(plr);
						SendToAll(new PlayerLeftPacket(packet.EntityID));
					}

					ConnectedUsers.Remove(user);
					
					Output?.Out("Disconnected " + user.Username);
				}

				if (msg.Packet.Type == PacketType.SPlayerPosition)
				{
					EntityPositionPacket packet = new EntityPositionPacket(msg.Packet.GetBytes());
					//Output?.Out("DT_IN: " + packet.ToString());
					var entity = FindEntityOfID(packet.EntityID);

					if (entity != null && entity is Player player)
					{
						player.Position = new Vector2(packet.PosX, packet.PosY);
						player.NextPosition = new Vector2(packet.NextX, packet.NextY);
						player.Velocity = new Vector2(packet.VelX, packet.VelY);
					}
				}

				if (msg.Packet.Type == PacketType.CChatMessage)
				{
					ClientChatMessagePacket chatMessagePacket = new ClientChatMessagePacket(msg.Packet.GetBytes());
					//server.Output?.Out("GOT CHAT MESSAGE!" + chatMessagePacket.Message);
					return;
				}

				if (msg.Packet.Type == PacketType.PlaceTile)
				{
					PlaceTilePacket packet = new PlaceTilePacket(msg.Packet.GetBytes());

					Tile t = Tile.FromID(packet.TileID);

					
					World.SetTile(packet.WorldX, packet.WorldY, t);
					//SendToAll(packet);
				}

				if (msg.Packet.Type == PacketType.CRequestChunk)
				{
					RequestChunkPacket packet = new RequestChunkPacket(msg.Packet.GetBytes());

					Chunk chunk;
					ChunkCoordinates coords = new ChunkCoordinates(packet.X, packet.Y);
					if (World.Chunks.ContainsKey(coords)) {
						chunk = World.Chunks.GetValueOrDefault(coords);
					} else
					{
						chunk = new Chunk(packet.X, packet.Y);
						//World.Chunks.Add(coords, chunk);
						World.Chunks.TryAdd(coords, chunk);
					}

					if (!chunk.DungeonPassCompleted)
					{
						chunk.DungeonPassCompleted = true;
						// make sure all adjacent chunks are loaded
						for (int x = -1; x <= 1; x++)
						{
							for (int y = -1; y <= 1; y++)
							{
								var newCoords = new ChunkCoordinates(packet.X + x, packet.Y + y);
								if (!World.Chunks.ContainsKey(newCoords))
								{
									var thischunk = new Chunk(packet.X+x, packet.Y+y);
									//World.Chunks.Add(newCoords, thischunk);
									World.Chunks.TryAdd(newCoords, thischunk);
								}
							}
						}

						for (int x = 0; x < Chunk.ChunkSize; x++)
						{
							for (int y = 0; y < Chunk.ChunkSize; y++)
							{
								StructureGeneration(World, coords.X * Chunk.ChunkSize + x, coords.Y * Chunk.ChunkSize + y);
							}
						}
					}

					Debug.WriteLine(packet.X + ", " + packet.Y);
					ChunkDownloadPacket chunkpacket = new ChunkDownloadPacket(chunk);
					server.SendPacket(chunkpacket, msg.Sender);
					return;
				}

				

			}
		}

		private IEntity FindEntityOfID(int id)
		{
			foreach (var entity in World.Entities)
				if (entity.EntityNetworkID == id)
					return entity;
			return null;
		}

		float tileticker = 0;


		public void Update(GameTime gt)
		{
			float delta = (float)gt.ElapsedGameTime.TotalSeconds;


			foreach(var user in ConnectedUsers.ToArray())
			{
				user.KeepAlive += delta;

				if (user.KeepAlive > 10.0f)
				{
					Output?.Out("Kicking " + user.Username + " for network timeout");
					ConnectedUsers.Remove(user);
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
								SendToAll(new PlaceTilePacket(t.ID, t.TileState, t.Damage, (c.Coordinates.X * Chunk.ChunkSize) + x, (c.Coordinates.Y * Chunk.ChunkSize) + y));
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
