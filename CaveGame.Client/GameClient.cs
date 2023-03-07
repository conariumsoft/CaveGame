//#define SERVER

using CaveGame.Client.Game.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CaveGame.Client.DesktopGL;
using CaveGame.Common;
using CaveGame.Common.Extensions;
using CaveGame.Common.Game.Entities;
using CaveGame.Common.Game.Furniture;
using CaveGame.Common.Game.Inventory;
using CaveGame.Common.Game.Items;
using CaveGame.Common.Game.Tiles;
using CaveGame.Common.Game.Walls;
using CaveGame.Common.Generic;
using CaveGame.Common.Network;
using CaveGame.Common.Network.Packets;

namespace CaveGame.Client
{
    using ArrowEntity = Common.Game.Entities.Arrow;
    using BombEntity = Common.Game.Entities.Bomb;
    using DynamiteEntity = Common.Game.Entities.Dynamite;
    using WurmholeEntity = Common.Game.Entities.Wurmhole;


	public struct AuthDetails
	{
		public string UserName { get; set; }
		public string ServerAddress { get; set; }


		public string IPAddress => "";
		public string Port => "";

	}

	public delegate void ClientShutdown();

	public class GameClient : IGameContext, IGameClient
	{
		public event ClientShutdown OnShutdown;


		// should this be in gamesettings?
		public int ChunkingRadius { get; set; }
		public bool ShowChunkBoundaries { get; set; }
		public static float CameraZoom = 2.0f;

		// not sure why this is stored here.
		public string NetworkUsername { get; private set; }
		public string ConnectAddress { get; private set; }
		public bool Active { get; set; }

		
		public CaveGameDesktopClient Game { get; private set; }
		// gameclient subsystems

		public GameChat Chat { get; private set; }
		public LocalWorld World { get; private set; }
		public PlayerContainerFrontend Inventory { get; set; }
		public Camera2D Camera { get; }
		public StatusEffectDisplay StatusDisplay { get; private set; }
		protected NetworkClient NetClient { get; set; }


		Microsoft.Xna.Framework.Game IGameContext.Game => Game;
		IClientWorld IGameClient.World => World;
		//public Hotbar Hotbar { get; set; }
		
		
		// ?
		int MyUserID { get; set; }
		int MyPlayerID { get; set; }



		public Game.Entities.LocalPlayer MyPlayer { get; private set; }


		PauseMenu PauseMenu { get; set; }


		protected List<RepeatingIntervalTask> ClientTasks { get; set; }


		
		// Initialize events for networkmanager to listen for
		// each event is bound to a PacketType ID
		private Dictionary<PacketType, NetworkListener> NetworkEvents;
		private void InitNetworkEvents() => NetworkEvents = new Dictionary<PacketType, NetworkListener>()
		{
			[PacketType.sHandshakeResponse]   = OnServerHandshakeReply,
			[PacketType.sChatMessage] = OnServerSendChatMessage,
			[PacketType.sPlayerState] = OnPlayerAnimationStateUpdate,
			[PacketType.sDownloadChunk] = DownloadChunk,
			[PacketType.sUpdateTile] = UpdateTile,
			[PacketType.sUpdateWall] = UpdateWall,

			// TODO: eventually stop using multi-way packets to reduce weirdness
			[PacketType.netPing] = OnPing,
			[PacketType.netPlaceTile] = UpdateTile,
			[PacketType.netPlaceWall] = UpdateWall,
			[PacketType.netOpenDoor] = OnPlayerOpensDoor,
			[PacketType.netCloseDoor] = OnPlayerClosesDoor,
			[PacketType.netEntityPhysicsUpdate] = OnEntityPhysUpdate,
			[PacketType.netPlayerState] = OnPlayerAnimationStateUpdate,
			[PacketType.netDamageTile] = OnDamageTile,
			[PacketType.netPlaceFurniture] = OnPlaceFurniture,
			[PacketType.netRemoveFurniture] = OnRemoveFurniture,
			[PacketType.sRejectLogin] = OnServerRejectLogin,
			[PacketType.sAcceptLogin] = OnServerAcceptLogin,
			[PacketType.sPlayerPeerJoined] = OnPeerJoined,
			[PacketType.sPlayerPeerLeft]  = OnPeerLeft,
			[PacketType.sRemoveEntity] = OnRemoveEntity,
			[PacketType.sExplosion] = OnExplosion,
			[PacketType.sSpawnEntityGeneric] = OnEntitySpawnGeneric,
			[PacketType.sUpdateTimeOfDay] = OnServerChangedTimeOfDay,
			[PacketType.sSpawnItemStackEntity] = OnItemStackEntitySpawned,
			[PacketType.sGivePlayerItem] = GiveItToMeDaddy,
			[PacketType.sProvokeEntityGeneric] = OnEntityProvokedGeneric,
			
		};

        internal void Dispose()
        {
            //throw new NotImplementedException();
        }

        public float ServerKeepAlive { get; set; }


		public GameClient(CaveGameDesktopClient _game) {
			Game = _game;
			InitNetworkEvents();

			World = new LocalWorld(this);
			Camera = new Camera2D { Zoom = CameraZoom };
			Chat = new GameChat(this);
			PauseMenu = new PauseMenu(this);
			StatusDisplay = new StatusEffectDisplay(this);
			Inventory = new PlayerContainerFrontend();

			// Game Event Scheduler
			ClientTasks = new List<RepeatingIntervalTask>
			{
				new RepeatingIntervalTask(ReplicatePlayerState, 1 / 10.0f),
				new RepeatingIntervalTask(ChunkUnloadingCheck, 1/2.0f),
				new RepeatingIntervalTask(ChunkLoadingCheckUpdate, 1 / 2.0f),
			};

			ChunkingRadius = 1;
		}

		public GameClient(CaveGameDesktopClient _game, AuthDetails login) : this(_game)
		{
			NetworkUsername = login.UserName;
			NetClient = new NetworkClient(login.ServerAddress);
		}


		// Cleanup code after leaving any game server.
		private void Logout()
        {
			PauseMenu.Open = false;

			World?.ClientDisconnect(); // start cleaning up server

			NetClient?.Logout(MyUserID, UserDisconnectReason.LogOff);
			World?.Dispose(); // destroy local world
			OnShutdown?.Invoke(); // bound to localserver (if it exists)
			//Dispose();
		}

		// Fills the player's inventory with test items upon startup.
		// This is testing data, and should be left out of the final game
		// Called after creating a player.
		// Should probably be done on the server...
		private void FillInventory(Container container)
        {
			container.ForceSetSlot(0, 3, ItemStack.Of<Nimdoc>(5));
			container.ForceSetSlot(0, 1, ItemStack.Of<CopperPickaxe>());
			container.ForceSetSlot(0, 1, ItemStack.Of<IronPickaxe>());
			container.ForceSetSlot(0, 2, ItemStack.Of<LeadPickaxe>());			
			container.ForceSetSlot(1, 1, ItemStack.Of<GenericWallScraper>());

			container.ForceSetSlot(0, 0, ItemStack.Of<BombItem>(999));

			container.ForceSetSlot(1, 0, new ItemStack(TileItem.Of<Common.Game.Tiles.OakPlank>(), 999));
			container.ForceSetSlot(2, 0, new ItemStack(TileItem.Of<Common.Game.Tiles.StoneBrick>(), 999));
			container.ForceSetSlot(3, 0, new ItemStack(TileItem.Of<Common.Game.Tiles.ClayBrick>(), 999));
			container.ForceSetSlot(4, 0, new ItemStack(TileItem.Of<GreenTorch>(), 999));
			container.ForceSetSlot(5, 0, new ItemStack(TileItem.Of<RedTorch>(), 999));
			container.ForceSetSlot(6, 0, new ItemStack(TileItem.Of<BlueTorch>(), 999));
			container.ForceSetSlot(7, 0, new ItemStack(TileItem.Of<Lava>(), 999));
			container.ForceSetSlot(8, 0, ItemStack.Of<FurnaceItem>(999));
			container.ForceSetSlot(9, 0, ItemStack.Of<DoorItem>(999));
			container.ForceSetSlot(1, 1, ItemStack.Of<WorkbenchItem>(999));
			//container.ForceSetSlot(5, 1, new ItemStack { Item = new WallItem(new Core.Game.Walls.StoneBrick()), Quantity = 999 });
		}

		public void Disconnect()
		{
			Logout();
			Game.GoToMainMenu();
		}

		public void ForcedDisconnect(string kickReason)
        {
			Logout();
			Game.GoToTimeoutPage(kickReason);
		}

		private void ChunkUnloadingCheck()
		{
			foreach (var chunkpair in World.Chunks)
			{
				if (!World.LoadedChunks.Contains(chunkpair.Key))
				{
					if (World.Chunks.ContainsKey(chunkpair.Key))
					{
						World.Chunks.TryRemove(chunkpair.Key, out _);
						World.Lighting.UnregisterChunk(chunkpair.Key);
						chunkpair.Value.Dispose();
					}
				}
			}
		}
		private void ChunkLoadingCheckUpdate()
		{
			if (MyPlayer!=null)
			{

			
			int camChunkX = (int)Math.Floor(MyPlayer.Position.X / (Globals.ChunkSize * Globals.TileSize));
			int camChunkY = (int)Math.Floor(MyPlayer.Position.Y / (Globals.ChunkSize * Globals.TileSize));

			int rendr = ChunkingRadius;
			World.LoadedChunks.Clear();
			for (int x = -rendr; x < rendr + 1; x++)
			{
				for (int y = -rendr; y < rendr + 1; y++)
				{
					ChunkCoordinates chunkCoords = new ChunkCoordinates(camChunkX + x, camChunkY + y);

					World.LoadedChunks.Add(chunkCoords);// TODO: spooky action at a distance?


					if ((!World.Chunks.ContainsKey(chunkCoords)) && (!World.RequestedChunks.Contains(chunkCoords)))
					{
						NetClient.SendPacket(new RequestChunkPacket(chunkCoords));
						World.RequestedChunks.Add(chunkCoords);
					}
				}
			}
			}
		}

		public void Send(Packet p) => NetClient.SendPacket(p);

		#region NetworkListenerMethods

		public delegate void NetworkListener(NetworkMessage message);

		private void OnPing(NetworkMessage message)
		{ 
			//GameConsole.Log("Ping from server");
		} 

		private void DownloadChunk(NetworkMessage message)
		{
			ChunkDownloadPacket chunkdata = new ChunkDownloadPacket(message.Packet.GetBytes());

			Task.Run(() =>
			{

				Chunk chunk = chunkdata.StoredChunk;
				// Did we ask for this chunk?
				if (World.RequestedChunks.Contains(chunk.Coordinates))
				{
					//World.Chunks. Add(chunk.Coordinates, chunk);
					World.Chunks.TryAdd(chunk.Coordinates, chunk);
					World.RequestedChunks.Remove(chunk.Coordinates);
					World.Lighting.RegisterChunk(chunk);
				}
			});
		}

		private void UpdateTile(NetworkMessage message)
		{
			PlaceTilePacket packet = new PlaceTilePacket(message.Packet.GetBytes());

			Tile t = Tile.FromID(packet.TileID);

			t.Damage = packet.Damage;
			t.TileState = packet.TileState;
			World.SetTile(packet.WorldX, packet.WorldY, t);
			//Debug.WriteLine("T");
		}
		private void UpdateWall(NetworkMessage message)
		{
			PlaceWallPacket packet = new PlaceWallPacket(message.Packet.GetBytes());

			Wall w = Wall.FromID(packet.WallID);
			w.Damage = packet.Damage;
			//t.TileState = packet.TileState;
			World.SetWall(packet.WorldX, packet.WorldY, w);

			//Debug.WriteLine("W");
		}
		private void OnPeerJoined(NetworkMessage message)
		{
			PlayerJoinedPacket packet = new PlayerJoinedPacket(message.Packet.GetBytes());

			var player = new Common.Game.Entities.Player()
			{
				EntityNetworkID = packet.EntityID,
				Color = packet.PlayerColor,
				DisplayName = packet.Username,
				RemoteControlled = true
			};
			World.Entities.Add(player);
		}
		private void OnPeerLeft(NetworkMessage message)
		{
			PlayerLeftPacket packet = new PlayerLeftPacket(message.Packet.GetBytes());

			if (World.FindEntityOfID(packet.EntityID, out IEntity entity))
				World.Entities.Remove(entity);

		}
		private void OnEntityPhysUpdate(NetworkMessage message)
		{
			EntityPhysicsStatePacket packet = new EntityPhysicsStatePacket(message.Packet.GetBytes());

			var entity = World.FindEntityOfID(packet.EntityID);

			if (entity == null)
				return;

			entity.Health = packet.Health;

			if (entity is Player plr)
			{
				if (packet.EntityID == MyPlayerID)
					return;

				plr.NextPosition = packet.NextPosition;
				return;
			}
			entity.Position = packet.Position;
			if (entity is IPhysicsEntity physical)
            {
				physical.Velocity = packet.Velocity;
				physical.NextPosition = packet.NextPosition;
			}
			

				
		}
		private void OnServerSendChatMessage(NetworkMessage message)
		{
			ServerChatMessagePacket packet = new ServerChatMessagePacket(message.Packet.GetBytes());
			Chat.AddMessage(packet.Message, packet.TextColor);
		}
		private void OnPlayerAnimationStateUpdate(NetworkMessage message)
		{
			PlayerStatePacket packet = new PlayerStatePacket(message.Packet.GetBytes());

			IEntity ent = World.FindEntityOfID(packet.EntityID);

			if (ent is Player plr)
			{
				plr.Walking = packet.Walking;
				plr.OnGround = packet.OnGround;
				plr.Facing = packet.Facing;
			}
		}
		private void OnServerRejectLogin(NetworkMessage message)
		{
			RejectJoinPacket packet = new RejectJoinPacket(message.Packet.GetBytes());
			// TODO: send player to the rejection screen

			Game.CurrentGameContext = Game.MenuContext;
			Game.MenuContext.TimeoutMessage = packet.RejectReason;
		}
		private void OnServerAcceptLogin(NetworkMessage message)
		{
			AcceptJoinPacket packet = new AcceptJoinPacket(message.Packet.GetBytes());


			MyUserID = packet.YourUserNetworkID;
			MyPlayerID = packet.YourPlayerNetworkID;

			GameConsole.Log($"{MyUserID}", Color.GreenYellow);
			// create me player
			LocalPlayer myplayer = new LocalPlayer();
			myplayer.EntityNetworkID = MyPlayerID;
			MyPlayer = myplayer;
			World.Entities.Add(myplayer);
			Inventory.Player = myplayer;
			FillInventory(Inventory.Container);
			
		}

		Random r = new Random();
		private void OnExplosion(NetworkMessage msg)
		{
			ExplosionPacket packet = new ExplosionPacket(msg.Packet.GetBytes());

			Explosion explosion = packet.Explosion;

			float DistanceFromCamera = explosion.Position.Distance(Camera.Position);

			if (DistanceFromCamera > 2000) return;
			float ClampedDistance =  (1.0f/Math.Clamp(DistanceFromCamera, 0.2f, 200f)) * 200f;

			Vector2 direction = explosion.Position.LookAt(Camera.Position);

			Camera.Shake(ClampedDistance * direction.X, ClampedDistance * direction.Y);

			World.Explosion(packet.Explosion, packet.DamageTiles, packet.DamageEntities);
		}


		private void SpawnBomb(int ID)     => World.Entities.Add(new BombEntity    {EntityNetworkID = ID});
		private void SpawnWurmhole(int ID) => World.Entities.Add(new WurmholeEntity{EntityNetworkID = ID});
		private void SpawnArrow(int ID)    => World.Entities.Add(new ArrowEntity   {EntityNetworkID = ID});
		private void SpawnDynamite(int ID) => World.Entities.Add(new DynamiteEntity{EntityNetworkID = ID});


		private void OnEntitySpawnGeneric(NetworkMessage message)
        {
			SpawnEntityGenericPacket packet = new(message.Packet.GetBytes());

			if (packet.EntityType == NetEntityType.Bomb)
				SpawnBomb(packet.EntityNetworkID);
			if (packet.EntityType == NetEntityType.Wurmhole)
				SpawnWurmhole(packet.EntityNetworkID);
			if (packet.EntityType == NetEntityType.Arrow)
				SpawnArrow(packet.EntityNetworkID);
			if (packet.EntityType == NetEntityType.Dynamite)
				SpawnDynamite(packet.EntityNetworkID);
		}


		private void OnRemoveEntity(NetworkMessage message)
		{
			DropEntityPacket packet = new DropEntityPacket(message.Packet.GetBytes());
			if (World.FindEntityOfID(packet.EntityID, out IEntity entity))
				World.Entities.Remove(entity);

		}

		private void OnPlaceFurniture(NetworkMessage msg)
		{
			PlaceFurniturePacket packet = new PlaceFurniturePacket(msg.Packet.GetBytes());

			FurnitureTile f = FurnitureTile.FromID(packet.FurnitureID);
			f.FurnitureNetworkID = packet.NetworkID;
			f.Position = new Point(packet.WorldX, packet.WorldY);
			World.Furniture.Add(f);
			Debug.WriteLine("Place {0}", packet.NetworkID);

		}
		private void OnRemoveFurniture(NetworkMessage msg)
		{
			RemoveFurniturePacket packet = new RemoveFurniturePacket(msg.Packet.GetBytes());

			var possibleF = World.GetFurniture(packet.FurnitureNetworkID);
			Debug.WriteLine("REMOVE {0}", packet.FurnitureNetworkID);
			if (possibleF != null)
				World.Furniture.Remove(possibleF);
				

		}

		private void OnPlayerClosesDoor(NetworkMessage msg)
		{
			CloseDoorPacket packet = new CloseDoorPacket(msg.Packet.GetBytes());

			var possibleF = World.GetFurniture(packet.FurnitureNetworkID);
			if (possibleF != null && possibleF is WoodenDoor door)
				door.State = DoorState.Closed;

		}

		private void OnPlayerOpensDoor(NetworkMessage msg)
		{
			OpenDoorPacket packet = new OpenDoorPacket(msg.Packet.GetBytes());

			var possibleF = World.GetFurniture(packet.FurnitureNetworkID);
			if (possibleF != null && possibleF is WoodenDoor door)
			{
				if (packet.Direction == Direction.Left)
					door.State = DoorState.OpenLeft;
				if (packet.Direction == Direction.Right)
					door.State = DoorState.OpenRight;
			}
		}
		private void OnServerChangedTimeOfDay(NetworkMessage msg)
		{
			TimeOfDayPacket packet = new TimeOfDayPacket(msg.Packet.GetBytes());

			World.TimeOfDay = packet.Time;
		}

		private void OnItemStackEntitySpawned(NetworkMessage msg)
        {
			SpawnItemStackPacket packet = new SpawnItemStackPacket(msg.Packet.GetBytes());
			ItemstackEntity itemstackEntity = new ItemstackEntity();
			itemstackEntity.Position = packet.Position;
			itemstackEntity.EntityNetworkID = packet.EntityNetworkID;
			itemstackEntity.ItemStack = packet.ItemStack;
			itemstackEntity.RemoteControlled = true;
			World.Entities.Add(itemstackEntity);
        }

		private void OnDamageTile(NetworkMessage msg)
		{
			//GameConsole.Log("WTF BRYH");
			DamageTilePacket packet = new DamageTilePacket(msg.Packet.GetBytes());

			var tile = World.GetTile(packet.Position.X, packet.Position.Y);

			tile.Damage += (byte)packet.Damage;

			if (tile.Damage >= tile.Hardness)
			{
				World.SetTile(packet.Position.X, packet.Position.Y, new Common.Game.Tiles.Air());
			}
		}

		private void GiveItToMeDaddy(NetworkMessage msg)
        {
			GivePlayerItemPacket packet = new GivePlayerItemPacket(msg.Packet.GetBytes());
			MyPlayer.Inventory.AddItem(packet.Reward);

        }

		private void OnEntityProvokedGeneric(NetworkMessage msg)
		{
			ProvokeEntityGenericPacket packet = new(msg.Packet.GetBytes());
			var entity = World.FindEntityOfID(packet.EntityNetworkID);
			if (entity is IProvokable provokable)
				provokable.Provoke();
		}

		private void OnServerHandshakeReply(NetworkMessage msg)
        {
			HandshakeResponsePacket packet = new(msg.Packet.GetBytes());

			var protocolMatches = (packet.ServerProtocolVersion == Globals.ProtocolVersion);

			if (!protocolMatches)
            {
				GameConsole.Log($"Protocol Mismatch: server {packet.ServerProtocolVersion} vs client {Globals.ProtocolVersion} ");
				Game.CurrentGameContext = Game.MenuContext;
				Game.MenuContext.CurrentPage = Game.MenuContext.Pages["timeoutmenu"];

				if (packet.ServerProtocolVersion > Globals.ProtocolVersion)
					Game.MenuContext.TimeoutMessage = "Your client is running an outdated version!";
				else
					Game.MenuContext.TimeoutMessage = "Server is running on an outdated version!";

				return;
			}


			GameConsole.Log($"Server handshake! sprotocol: {packet.ServerProtocolVersion} (matches)", Color.PaleVioletRed);
			GameConsole.Log($"Connecting to {packet.ServerName}", Color.PaleVioletRed);
			GameConsole.Log($"{packet.ServerName}: {packet.ServerMOTD}", Color.White);
			int currentPlrCount = packet.PlayerList.Where(str => str.Length > 0).Count();
			GameConsole.Log($"Connected players ({currentPlrCount}):", Color.PaleVioletRed);
			foreach (var name in packet.PlayerList.Where(str => str.Length > 0))
				GameConsole.Log($"    {name}", Color.LightBlue);
			GameConsole.Log($"Requesting game session...");
			NetClient.SendPacket(new RequestJoinPacket(NetworkUsername));
		}

		#endregion


		private void ReadIncomingPackets()
		{
			// read all queued packets
			while (NetClient.HaveIncomingMessage())
			{
				NetworkMessage msg = NetClient.GetLatestMessage();
				foreach(var ev in NetworkEvents)
					if (ev.Key == msg.Packet.Type)
                    {
						ServerKeepAlive = 0;
						ev.Value.Invoke(msg);
					}	
			}
		}


		float redrawTimer { get; set; }

		// chunks are drawn to a back buffer, rather than redrawing the chunk each frame
		// this causes quite a few issues.
		private void RedrawChunkBuffers(GraphicsEngine GFX)
		{
			foreach (var chunkpair in World.Chunks)
			{
				var chunk = chunkpair.Value;
				chunk.RedrawWallBuffer(GFX);
				chunk.RedrawTileBuffer(GFX);
			}
		}


		private void DrawEntities(GraphicsEngine gfx)
		{
			foreach (Entity entity in World.Entities)
				entity.Draw(gfx);
		}

		// draw foreground chunk buffers
		// drawn after background
		private void DrawChunkTileTextures(GraphicsEngine gfx)
		{
			foreach (var chunkpair in World.Chunks)
			{

				Chunk chunk = chunkpair.Value;
				Vector2 pos = new Vector2(chunk.Coordinates.X * Globals.ChunkSize * Globals.TileSize, chunk.Coordinates.Y * Globals.ChunkSize * Globals.TileSize);
				if (chunk.TileRenderBuffer != null)
					gfx.Sprite(chunk.TileRenderBuffer, pos, Color.White);

			}
		}

		// draw background chunk buffers
		// drawn before entities & particles
		private void DrawChunkWallTextures(GraphicsEngine gfx)
		{
			foreach (var chunkpair in World.Chunks)
			{

				Chunk chunk = chunkpair.Value;
				Vector2 pos = new Vector2(chunk.Coordinates.X * Globals.ChunkSize * Globals.TileSize, chunk.Coordinates.Y * Globals.ChunkSize * Globals.TileSize);

				if (chunk.WallRenderBuffer != null)
					gfx.Sprite(chunk.WallRenderBuffer, pos, Color.White);
			}
		}

		// render a shitton of debug data
		// todo: move to their own togglable datasets
		private void DrawDebugInfo(GraphicsEngine gfx)
		{
			gfx.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
			MouseState mouse = Mouse.GetState();

			var mp = Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			mp /= 8;
			mp.Floor();
			var tileCoords = mp;
			mp *= 8;

			var tileat = World.GetTile((int)tileCoords.X, (int)tileCoords.Y);
			var wallat = World.GetWall((int)tileCoords.X, (int)tileCoords.Y);

			List<string> debugStats = new List<string>();


			debugStats.Add($"userid {MyUserID} netaddr {ConnectAddress}");


			if (World!=null)
				debugStats.Add($"entities {World.Entities.Count} furn {World.Furniture.Count}");

			if (MyPlayer != null)
				debugStats.Add(String.Format("pos {0} {1} vel {2} {3}, rv {4} {5}",
						Math.Floor(MyPlayer.Position.X / Globals.TileSize),
						Math.Floor(MyPlayer.Position.Y / Globals.TileSize),
						Math.Round(MyPlayer.Velocity.X, 2),
						Math.Round(MyPlayer.Velocity.Y, 2),
						Math.Round(MyPlayer.RecentVelocity.X, 2),
						Math.Round(MyPlayer.RecentVelocity.Y, 2)));


			debugStats.Add(String.Format("tid {0}, state {1} tdmg {2} wid {3} wdmg {4} light {5}",
						tileat.ID,
						tileat.TileState,
						tileat.Damage,
						wallat.ID,
						wallat.Damage,
						World.GetLight((int)tileCoords.X, (int)tileCoords.Y).ToString()));

			int incr = 1;
			foreach(var line in debugStats)
            {
				gfx.Text(line, new Vector2(2, incr * 14));
				incr++;
			}

			gfx.End();
		}

		
		public void Load()
		{
			PauseMenu.LoadShader(Game.Content);

			NetClient.Start();
			NetClient.SendPacket(new InitServerHandshakePacket(Globals.ProtocolVersion));
			//gameClient.SendPacket(new RequestJoinPacket("jooj"));
		}

		// on connection failure
		// show the "OOPS" screen
		private void FailConnect(string reason)
        {
			Game.MenuContext.CurrentPage = Game.MenuContext.TimeoutPage;
			Game.MenuContext.TimeoutMessage = reason;
			Game.CurrentGameContext = Game.MenuContext;
        }

		public void Unload() {}
		MouseState previous = Mouse.GetState();
		private void HotbarUpdate(GameTime gt)
		{
			MouseState mouse = Mouse.GetState();
			if (!Inventory.EquippedItem.Equals(ItemStack.Empty))
			{
				// Just Pressed
				if (mouse.LeftButton == ButtonState.Pressed && previous.LeftButton == ButtonState.Released)
					Inventory.EquippedItem.Item.OnClientLMBDown(MyPlayer, this, Inventory.EquippedItem);

				// being held
				if (mouse.LeftButton == ButtonState.Pressed && previous.LeftButton == ButtonState.Pressed)
					Inventory.EquippedItem.Item.OnClientLMBHeld(MyPlayer, this, Inventory.EquippedItem, gt);

				// released
				if (mouse.LeftButton == ButtonState.Released && previous.LeftButton == ButtonState.Pressed)
					Inventory.EquippedItem.Item.OnClientLMBUp(MyPlayer, this, Inventory.EquippedItem);
			}
			
			// Object interaction
			if (mouse.RightButton == ButtonState.Pressed && previous.RightButton == ButtonState.Released)
			{
				var mp = Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());
				Point pos = new Point(
					(int)Math.Floor(mp.X / Globals.TileSize),
					(int)Math.Floor(mp.Y / Globals.TileSize)
				);
				var maybeFurniture = World.GetFurnitureAt(pos.X, pos.Y);

				if (maybeFurniture!=null)
					maybeFurniture.OnPlayerInteracts(MyPlayer, World, this);

			}
			previous = mouse;

		}

		// send player direction and animation state to the server
		// players are handled slightly differently in that animation state is controlled on the client, and replicated to the server
		// sanity checking is in order
		private void ReplicatePlayerState()
		{
			if (MyPlayer != null)
			{
				NetClient?.SendPacket(new EntityPhysicsStatePacket(MyPlayer));
				NetClient?.SendPacket(new PlayerStatePacket(MyPlayer.Facing, MyPlayer.OnGround, MyPlayer.Walking));
			}
		}


		float scroll = 2;

		// update game camera
		private void UpdateCamera(GameTime gt)
		{
			MouseState mouse = Mouse.GetState();

			// scroll
			if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
				scroll -= 0.01f;
			if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
				scroll += 0.01f;

			//float ZoomFactor = ((mouse.ScrollWheelValue - IntitalScrollValue) * (Senitivity / 120)) + 2;

			Vector2 MouseCameraMovement = ((mouse.Position.ToVector2() / Camera.WindowSize) - new Vector2(0.5f, 0.5f)) * 5.5f;


			Camera.Zoom = Math.Clamp(scroll, 0.05f, 10f);

			if (MyPlayer != null)
			{
				if (((Camera.Position) - MyPlayer.Position).Length() < 500f)
				{
					float speed = (float)(gt.ElapsedGameTime.TotalSeconds * 10.0);
					Camera.Position = Camera.Position.Lerp(MyPlayer.Position + MouseCameraMovement, speed);
				}
				else
				{
					Camera.Position = MyPlayer.Position;
				}
			}
		}


		KeyboardState previousKB = Keyboard.GetState();
		KeyboardState currentKB = Keyboard.GetState();
		private bool PressedThisFrame(Keys key) =>  (currentKB.IsKeyDown(key) && !previousKB.IsKeyDown(key));
		private bool ReleasedThisFrame(Keys key) => (!currentKB.IsKeyDown(key) && previousKB.IsKeyDown(key));


		private void UpdateInputs()
        {
			currentKB = Keyboard.GetState();

			
			if (PressedThisFrame(Keys.P) && !MyPlayer.IgnoreInput)
				if (MyPlayer != null) // teleport
					MyPlayer.NextPosition = Camera.ScreenToWorldCoordinates(Mouse.GetState().Position.ToVector2());



			if (PressedThisFrame(Keys.Escape)) {
				if (Chat.Open == true)
					Chat.Open = false;
				else if (Game.Console.Open == true)
					Game.Console.Open = false;
				else
					PauseMenu.Open = !PauseMenu.Open;
			}

					if (PressedThisFrame(Keys.Tab) && !MyPlayer.IgnoreInput)
				Inventory.InventoryOpen = !Inventory.InventoryOpen;


			if (PressedThisFrame(Keys.F3))
				ShowChunkBoundaries = !ShowChunkBoundaries;


			previousKB = currentKB;
		}

		// hide keyboard events from the game engine while the
		// chat or console are open.
		private void UpdatePlayerIgnoresKeyboardInput()
		{
			// check for player first
			if (MyPlayer != null)
			{
				if (PauseMenu.Open == true || Chat.Open == true || Game.Console.Open == true)
					MyPlayer.IgnoreInput = true;
				else
					MyPlayer.IgnoreInput = false;
			}
		}

		public void Update(GameTime gt)
		{
			redrawTimer += gt.GetDelta();
			ServerKeepAlive += gt.GetDelta();
			UpdateInputs();

			Camera.Update(gt);
			ClientTasks.ForEach(ct => ct.Update(gt));
			Inventory.Update(gt);
			PauseMenu.Update(gt);
            Chat.Update(gt);
			World.Update(gt);
			StatusDisplay.Update(gt);
			HotbarUpdate(gt);
			UpdateCamera(gt);
			ReadIncomingPackets();
		}

		protected void DrawBackgroundLayer(GraphicsEngine GFX) 
		{
			Game.GraphicsDevice.Clear(World.SkyColor);
			GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
			World.Sky.DrawSkyColors(GFX);
			GFX.End();
		}
		protected void DrawGameLayer(GraphicsEngine GFX) 
		{
			// Game Layer
			GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera.View);
			
			if (PauseMenu.Open)
				PauseMenu.DrawWaterPixelsFilter(GFX); // apply background blur

			World.Sky.DrawBackground(GFX);	// 0:  render sky/background
			DrawChunkWallTextures(GFX);		//
			DrawChunkTileTextures(GFX);
			foreach (var furn in World.Furniture)
			{
				furn.Draw(GFX);
			}
			DrawEntities(GFX);
			World.ParticleSystem.Draw(GFX);

			if (!Inventory.EquippedItem.Equals(ItemStack.Empty))
				Inventory.EquippedItem.Item?.OnClientDraw(GFX);

			MouseState mouse = Mouse.GetState();

			var mp = Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			mp /= 8;
			mp.Floor();
			var tileCoords = mp;
			mp *= 8;


			if (mouse.LeftButton == ButtonState.Pressed)
			{
				GFX.Rect(Color.Green, mp, new Vector2(8, 8));
			}
			else
			{
				GFX.Rect(new Color(1, 1, 1, 0.5f), mp, new Vector2(8, 8));
			}

			GFX.End();
		}

		protected void DrawUILayer(GraphicsEngine GFX)
        {
			GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
			Inventory.Draw(GFX);
			StatusDisplay.Draw(GFX);
			PauseMenu.Draw(GFX);
			GFX.End();
			Chat.Draw(GFX);
			

			DrawDebugInfo(GFX);
		}

		public void Draw(GraphicsEngine GFX)
		{
			
			if (redrawTimer > (1.0f / 8.0f)) 
			{
				redrawTimer = 0;
				RedrawChunkBuffers(GFX);
			}

			DrawBackgroundLayer(GFX);
			DrawGameLayer(GFX);
			DrawUILayer(GFX);

			GFX.Begin(SpriteSortMode.Immediate);
		//	FPSGraph.Draw(GFX);
			//FPSGraph.DrawLineGraph(GFX, ImmediateData);
		//	FPSGraph.DrawLineGraph(GFX, AverageData);
			GFX.End();
		}
	}
}
