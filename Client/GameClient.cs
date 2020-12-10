//#define SERVER

using CaveGame;
using CaveGame.Client;
using CaveGame.Client.UI;
using CaveGame.Core;
using CaveGame.Core.Game.Entities;
using CaveGame.Core.Furniture;
using CaveGame.Core.Generic;
using CaveGame.Core.Inventory;
using CaveGame.Core.Network;
using CaveGame.Core.Game.Tiles;
using CaveGame.Core.Game.Walls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Threading;
using CaveGame.Client.Game.Entities;
using CaveGame.Core.Game.Inventory;
using CaveGame.Core.Game.Items;
using CaveGame.Client.DebugTools;
using CaveGame.Core.Network.Packets;
using System.Linq;
using System.Collections.Generic;


namespace CaveGame.Client
{
	using BombEntity = CaveGame.Core.Game.Entities.Bomb;

	public class GameClient : IGameContext, IGameClient
	{
		public static float CameraZoom = 2.0f;
		public bool ChunkLock { get; set; }
		protected ChunkGridLineRenderer chunkGridTool;// = new ChunkGridLineRenderer();
		public bool ShowChunkBoundaries { get; set; }
		public CaveGameGL Game { get; private set; }
		public GameChat Chat { get; private set; }
		public string NetworkUsername { get; set; }
		public string ConnectAddress { get; set; }
		public bool Active { get; set; }
		Microsoft.Xna.Framework.Game IGameContext.Game => Game;
		IClientWorld IGameClient.World => World;
		//public Hotbar Hotbar { get; set; }
		public PlayerContainerFrontend Inventory { get; set; }
		public LocalWorld World { get; private set; }
		private NetworkClient gameClient;
		public Camera2D Camera { get; }
		public int ChunkingRadius { get; set; }

		int MyUserID;
		int MyPlayerID;

		public Game.Entities.LocalPlayer MyPlayer { get; private set; }

		private DelayedTask chunkUnloadingTask;
		DelayedTask playerStateReplicationTask;
		DelayedTask chunkLoadingTask;

		private int IntitalScrollValue;


		PauseMenu PauseMenu { get; set; }
		

		private void onClientExit(TextButton sender, MouseState state)=>OverrideDisconnect();
	
		public void OverrideDisconnect()
		{
			PauseMenu.Open = false;
			Disconnect();
			Game.CurrentGameContext = Game.MenuContext;
		}


		private Dictionary<PacketType, NetworkListener> NetworkEvents;
		private void InitNetworkEvents() => NetworkEvents = new Dictionary<PacketType, NetworkListener>()
		{
			[PacketType.sHandshakeResponse]   = OnServerHandshakeReply,
			[PacketType.sChatMessage] = OnServerSendChatMessage,
			[PacketType.sPlayerState] = OnPlayerAnimationStateUpdate,
			[PacketType.sDownloadChunk] = DownloadChunk,
			[PacketType.sUpdateTile] = UpdateTile,
			[PacketType.sUpdateWall] = UpdateWall,
			[PacketType.sRejectLogin] = OnServerRejectLogin,
			[PacketType.sAcceptLogin] = OnServerAcceptLogin,
			[PacketType.sPlayerPeerJoined] = OnPeerJoined,
			[PacketType.sPlayerPeerLeft]  = OnPeerLeft,
			[PacketType.sRemoveEntity] = OnRemoveEntity,
			[PacketType.sEntityPhysicsUpdate] = OnEntityPhysUpdate,
			[PacketType.sExplosion] = OnExplosion,
			[PacketType.sSpawnEntityGeneric] = OnEntitySpawnGeneric,
			[PacketType.PlaceFurniture] = OnPlaceFurniture,
			[PacketType.RemoveFurniture] = OnRemoveFurniture,
			[PacketType.OpenDoor] = OnPlayerOpensDoor,
			[PacketType.CloseDoor] = OnPlayerClosesDoor,
			[PacketType.TimeOfDay] = OnServerChangedTimeOfDay,
			[PacketType.SpawnItemStackEntity] = OnItemStackEntitySpawned,
			[PacketType.DamageTile] = OnDamageTile,
			[PacketType.GivePlayerItem] = GiveItToMeDaddy,
			[PacketType.SpawnWurmholeEntity] = OnSpawnedWurmhole,
			[PacketType.TriggerWurmholeEntity] = OnWurmholeTriggered,
		};

		public GameClient(CaveGameGL _game)
		{
			InitNetworkEvents();


			Game = _game;

			MouseState mouse = Mouse.GetState();

			World     = new LocalWorld(this);
			Camera    = new Camera2D(Game.GraphicsDevice.Viewport) { Zoom = CameraZoom };
			Chat      = new GameChat(this);
			PauseMenu = new PauseMenu(this);
			Inventory = new PlayerContainerFrontend();

			IntitalScrollValue = mouse.ScrollWheelValue;

			playerStateReplicationTask = new DelayedTask(ReplicatePlayerState, 1 / 10.0f);
			chunkUnloadingTask = new DelayedTask(ChunkUnloadingCheck, 1/2.0f);
			chunkLoadingTask = new DelayedTask(ChunkLoadingCheckUpdate, 1 / 2.0f);


			ChunkingRadius = 1;
			

		}
		public void Send(Packet p)=>gameClient.SendPacket(p);
		public void SendChatMessage(object sender, string message)
		{
			Chat.Open = false;
			gameClient.SendPacket(new ClientChatMessagePacket(message));	
		}
		public void Disconnect()
		{
			if (MyPlayer != null) {

				World.ClientDisconnect();
				gameClient.SendPacket(new DisconnectPacket(MyPlayer.EntityNetworkID, UserDisconnectReason.LogOff));
				gameClient.Stop();
			}

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
					}
				}
			}
			//	}
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
						gameClient.SendPacket(new RequestChunkPacket(chunkCoords));
						World.RequestedChunks.Add(chunkCoords);
					}
				}
			}
			}
		}

		#region NetworkListenerMethods

		public delegate void NetworkListener(NetworkMessage message);

		private void DownloadChunk(NetworkMessage message)
		{
			ChunkDownloadPacket chunkdata = new ChunkDownloadPacket(message.Packet.GetBytes());

			Chunk chunk = chunkdata.StoredChunk;

			// Did we ask for this chunk?
			if (World.RequestedChunks.Contains(chunk.Coordinates))
			{
				//World.Chunks. Add(chunk.Coordinates, chunk);
				World.Chunks.TryAdd(chunk.Coordinates, chunk);
				World.RequestedChunks.Remove(chunk.Coordinates);
				World.Lighting.RegisterChunk(chunk);
			}
			
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

			var player = new Game.Entities.PeerPlayer()
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
			EntityPositionPacket packet = new EntityPositionPacket(message.Packet.GetBytes());

			var entity = World.FindEntityOfID(packet.EntityID);

			if (entity == null)
				return;

			entity.Health = packet.Health;

			if (entity is Player plr )
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
			Inventory.Container.ForceSetSlot(0, 0, new ItemStack { Item = new RaycastTesterItem(), Quantity = 1 });
			//Inventory.Container.ForceSetSlot(0, 0, new ItemStack {Item = new CopperPickaxe(), Quantity = 1 });
			Inventory.Container.ForceSetSlot(0, 1, new ItemStack { Item = new IronPickaxe(), Quantity = 1 });
			Inventory.Container.ForceSetSlot(0, 2, new ItemStack { Item = new LeadPickaxe(), Quantity = 1 });
			Inventory.Container.ForceSetSlot(1, 0, new ItemStack { Item = new TileItem(new Core.Game.Tiles.OakPlank()), Quantity = 999 });
			Inventory.Container.ForceSetSlot(1, 1, new ItemStack { Item = new GenericWallScraper(), Quantity = 1 });
			Inventory.Container.ForceSetSlot(2, 0, new ItemStack { Item = new TileItem(new Core.Game.Tiles.StoneBrick()), Quantity = 999 });
			Inventory.Container.ForceSetSlot(3, 0, new ItemStack { Item = new TileItem(new Core.Game.Tiles.ClayBrick()), Quantity = 999 });
			Inventory.Container.ForceSetSlot(4, 0, new ItemStack { Item = new BombItem(), Quantity = 999 });
			Inventory.Container.ForceSetSlot(5, 0, new ItemStack { Item = new TileItem(new RedTorch()), Quantity = 999 });
			Inventory.Container.ForceSetSlot(6, 0, new ItemStack { Item = new TileItem(new GreenTorch()), Quantity = 999 });
			Inventory.Container.ForceSetSlot(7, 0, new ItemStack { Item = new TileItem(new BlueTorch()), Quantity = 999 });
			Inventory.Container.ForceSetSlot(8, 0, new ItemStack { Item = new TileItem(new Torch()), Quantity = 999 });
			Inventory.Container.ForceSetSlot(9, 0, new ItemStack { Item = new TileItem(new Water()), Quantity = 999 });

			Inventory.Container.ForceSetSlot(2, 1, new ItemStack { Item = new WallItem(new Core.Game.Walls.ClayBrick()), Quantity = 999 });
			Inventory.Container.ForceSetSlot(3, 1, new ItemStack { Item = new WallItem(new Core.Game.Walls.Dirt()), Quantity = 999 });
			Inventory.Container.ForceSetSlot(4, 1, new ItemStack { Item = new WallItem(new Core.Game.Walls.OakPlank()), Quantity = 999 });
			Inventory.Container.ForceSetSlot(5, 1, new ItemStack { Item = new WallItem(new Core.Game.Walls.StoneBrick()), Quantity = 999 });
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


		private void SpawnBomb(int networkID) => World.Entities.Add(new BombEntity { 
			EntityNetworkID = networkID,
			RemoteControlled = true,
		});

		private void SpawnWurmhole(int networkID) => World.Entities.Add(new Wurmhole {
			EntityNetworkID = networkID,
		});

		private void SpawnArrow(int networkID) => World.Entities.Add(new ArrowEntity
        {

        })


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
			// TODO: Implement damage texture onto tile
			// TODO: add audio for tile being damaged

        }

		private void GiveItToMeDaddy(NetworkMessage msg)
        {
			GivePlayerItemPacket packet = new GivePlayerItemPacket(msg.Packet.GetBytes());
			MyPlayer.Inventory.AddItem(packet.Reward);

        }

		private void OnWurmholeTriggered(NetworkMessage msg)
		{
			TriggerWurmholeEntityPacket packet = new TriggerWurmholeEntityPacket(msg.Packet.GetBytes());

			var entity = World.FindEntityOfID(packet.EntityNetworkID);

			if (entity is Wurmhole wurmhole)
            {
				wurmhole.Triggered = true;
            }
		}

		private void OnServerHandshakeReply(NetworkMessage msg)
        {
			ServerInformationReplyPacket packet = new(msg.Packet.GetBytes());

			var protocolMatches = (packet.ServerProtocolVersion == Globals.ProtocolVersion);

			if (!protocolMatches)
            {
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
			gameClient.SendPacket(new RequestJoinPacket(NetworkUsername));
		}

		#endregion
		private void ReadIncomingPackets()
		{
			while (gameClient.HaveIncomingMessage())
			{
				NetworkMessage msg = gameClient.GetLatestMessage();
				foreach(var ev in NetworkEvents)
					if (ev.Key == msg.Packet.Type)
						ev.Value.Invoke(msg);
			}
		}

		private void DrawChunks(GraphicsEngine GFX)
		{
			if (drawTimer > (1/5.0f))
			{
				drawTimer = 0;
				Chunk.RefreshedThisFrame = false;
				foreach (var chunkpair in World.Chunks)
				{
					chunkpair.Value.Draw(GFX);	
				}
			}
		}

		private void EntityRendering(GraphicsEngine gfx)
		{
			foreach (Entity entity in World.Entities)
            {
				entity.Draw(gfx);
			}

		}

		private void DrawChunkFGTextures(GraphicsEngine gfx)
		{
			foreach (var chunkpair in World.Chunks)
			{
				
				Chunk chunk = chunkpair.Value;
				Vector2 pos = new Vector2(chunk.Coordinates.X * Globals.ChunkSize * Globals.TileSize, chunk.Coordinates.Y * Globals.ChunkSize * Globals.TileSize);
				if (chunk.ForegroundRenderBuffer != null)
					gfx.Sprite(chunk.ForegroundRenderBuffer, pos, Color.White);

			}
		}
		private void DrawChunkBGTextures(GraphicsEngine gfx)
		{
			foreach (var chunkpair in World.Chunks)
			{

				Chunk chunk = chunkpair.Value;
				Vector2 pos = new Vector2(chunk.Coordinates.X * Globals.ChunkSize * Globals.TileSize, chunk.Coordinates.Y * Globals.ChunkSize * Globals.TileSize);

				if (chunk.BackgroundRenderBuffer != null)
					gfx.Sprite(chunk.BackgroundRenderBuffer, pos, Color.White);
			}
		}
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


			if (MyPlayer != null)
			{
				string positionData = String.Format("pos {0} {1} vel {2} {3}",
						Math.Floor(MyPlayer.Position.X / Globals.TileSize),
						Math.Floor(MyPlayer.Position.Y / Globals.TileSize),
						Math.Round(MyPlayer.Velocity.X, 2),
						Math.Round(MyPlayer.Velocity.Y, 2)
				);
				
				string networkData = String.Format("pin {0}, pout {1} myid {2} ipaddr {3}",
						gameClient.ReceivedCount,
						gameClient.SentCount,
						MyPlayer?.EntityNetworkID,
						ConnectAddress
				);
				
				string worldData = String.Format("tid {0}, state {1} tdmg {2} wid {3} wdmg {4} light {5}",
						tileat.ID,
						tileat.TileState,
						tileat.Damage,
						wallat.ID,
						wallat.Damage,
						World.GetLight((int)tileCoords.X, (int)tileCoords.Y).ToString()
				);
				

				string mObjectData = String.Format("entities {0} furn {1}", World.Entities.Count, World.Furniture.Count);

				gfx.Text(positionData, new Vector2(2, 12));
				gfx.Text(networkData, new Vector2(2, 24));
				gfx.Text(worldData, new Vector2(2, 36));
				gfx.Text(mObjectData, new Vector2(2, 48));

			}
			gfx.End();
		}

		private void DrawSkyColor(GraphicsEngine GFX)
		{
			for (int y = 0; y<10; y++)
			{


				int hourTime = (int)Math.Floor( ( (World.TimeOfDay+1)%24)  / 2);
				int bottom = hourTime*2;
				int top = (hourTime*2)  + 1;
				//float diff = World.TimeOfDay % 1;
				var thisSection = Color.Lerp(World.SkyColors[bottom], World.SkyColors[top], y/10.0f);

				int prevhourTime = (int)Math.Floor((World.TimeOfDay % 24) / 2);
				int prevbottom = prevhourTime * 2;
				int prevtop = (prevhourTime * 2) + 1;
				//float diff = World.TimeOfDay % 1;
				var prevSection = Color.Lerp(World.SkyColors[prevbottom], World.SkyColors[prevtop], y / 10.0f);

				var finalColor = Color.Lerp(prevSection, thisSection, (World.TimeOfDay % 2.0f) / 2.0f);
				float sliceHeight = Camera._screenSize.Y / 10.0f;
				GFX.Rect(finalColor, new Vector2(0,(sliceHeight*y)), new Vector2(Camera._screenSize.X, sliceHeight + 1));
			}
			
		}

		public void Draw(GraphicsEngine GFX)
		{

			DrawChunks(GFX);

			Game.GraphicsDevice.Clear(World.SkyColor);
			GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
			
			DrawSkyColor(GFX);
			GFX.End();
			GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera.View);
			if (PauseMenu.Open)
				PauseMenu.DrawWaterPixelsFilter(GFX);
				

			
			DrawChunkBGTextures(GFX);
			DrawChunkFGTextures(GFX);

			foreach (var furn in World.Furniture)
			{
				furn.Draw(GFX);
			}
			EntityRendering(GFX);
			
			World.ParticleSystem.Draw(GFX);

			if (!Inventory.EquippedItem.Equals(ItemStack.Empty))
			{
				Inventory.EquippedItem.Item?.OnClientDraw(GFX);
			}

			MouseState mouse = Mouse.GetState();

			var mp = Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			mp /= 8;
			mp.Floor();
			var tileCoords = mp;
			mp *= 8;


			if (mouse.LeftButton == ButtonState.Pressed)
			{
				GFX.Rect(Color.Green, mp, new Vector2(8, 8));
			} else
			{
				GFX.Rect(new Color(1,1,1,0.5f), mp, new Vector2(8, 8));
			}

			GFX.End();
			if (ShowChunkBoundaries)
			{
				GFX.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Camera.View);
				chunkGridTool?.Draw(GFX, Camera);
				GFX.End();
			}

			//	TODO: Consolidate draw calls
			GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
			Inventory.Draw(GFX);
            PauseMenu.Draw(GFX);
			GFX.End();
			DrawDebugInfo(GFX);
			Chat.Draw(GFX);

			
		}

		public void Load()
		{
			PauseMenu.LoadShader(Game.Content);

			gameClient = new NetworkClient(ConnectAddress);
			//gameClient = new NetworkClient("127.0.0.1:40269");
			//gameClient.Output = Game.Console;
			gameClient.Start();
			gameClient.SendPacket(new GetServerInformationPacket(Globals.ProtocolVersion));
			
			//gameClient.SendPacket(new RequestJoinPacket("jooj"));
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
				{
					Inventory.EquippedItem.Item.OnClientLMBDown(MyPlayer, this, Inventory.EquippedItem);
				}
				if (mouse.LeftButton == ButtonState.Pressed && previous.LeftButton == ButtonState.Pressed)
				{
					Inventory.EquippedItem.Item.OnClientLMBHeld(MyPlayer, this, Inventory.EquippedItem, gt);
				}
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


		private void ReplicatePlayerState()
		{
			//Debug.WriteLine("Replicating");
			if (MyPlayer != null)
			{
				gameClient?.SendPacket(
					new EntityPositionPacket(MyPlayer)
				);

				gameClient?.SendPacket(new PlayerStatePacket(MyPlayer.Facing, MyPlayer.OnGround, MyPlayer.Walking));
			}
		}


		float scroll = 2;
		private void UpdateCamera(GameTime gt)
		{
			MouseState mouse = Mouse.GetState();

			if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
			{
				scroll -= 0.01f;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
			{
				scroll += 0.01f;
			}

			//float ZoomFactor = ((mouse.ScrollWheelValue - IntitalScrollValue) * (Senitivity / 120)) + 2;

			Vector2 MouseCameraMovement = ((mouse.Position.ToVector2() / Camera._screenSize) - new Vector2(0.5f, 0.5f)) * 5.5f;


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


		float drawTimer = 0;
		
		public void Update(GameTime gt)
		{
			
			UpdateInputs();

			Camera.Update(gt);
			

			drawTimer += (float)gt.ElapsedGameTime.TotalSeconds;
			
			playerStateReplicationTask.Update(gt);
			chunkUnloadingTask.Update(gt);
			chunkLoadingTask.Update(gt);

			if (MyPlayer != null)
			{
				if (PauseMenu.Open == true || Chat.Open == true || Game.Console.Open == true)
					MyPlayer.IgnoreInput = true;
				else
					MyPlayer.IgnoreInput = false;
			}


			Inventory.Update(gt);
			PauseMenu.Update(gt);
			Chat.Update(gt);
			World.Update(gt);
			HotbarUpdate(gt);
			UpdateCamera(gt);
			ReadIncomingPackets();
		}
	}
}
