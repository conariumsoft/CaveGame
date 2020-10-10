//#define SERVER

using Cave;
using CaveGame.Client.UI;
using CaveGame.Core;
using CaveGame.Core.Entities;
using CaveGame.Core.Inventory;
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using CaveGame.Server;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Threading;

namespace CaveGame.Client
{
	public class TempItemDef
	{
		public Rectangle Quad;
		public byte TileID;
		public Color Color;
	}

	// The testbench for building the server/client archite ture
	public class Hotbar
	{
		

		public int Index { get; set; }

		public TempItemDef[] ItemSet =
		{
			new TempItemDef{ Quad=TileMap.Stone, Color = Color.Gray, TileID = (byte)TileID.Stone },
			new TempItemDef{ Quad=TileMap.Soil, Color = Color.Green, TileID = (byte)TileID.Grass },
			new TempItemDef{ Quad=TileMap.Soil, Color = TileDefinitions.Dirt.Color, TileID = (byte)TileID.Dirt },
			new TempItemDef{ Quad=TileMap.Leaves, Color = Color.Green, TileID = (byte)TileID.Leaves},
			new TempItemDef{ Quad=TileMap.Brick, Color = TileDefinitions.ClayBrick.Color, TileID = (byte)TileID.ClayBrick},
			new TempItemDef{ Quad=TileMap.Brick, Color = TileDefinitions.StoneBrick.Color, TileID = (byte)TileID.StoneBrick},
			new TempItemDef{ Quad=TileMap.Default, Color = Color.Blue, TileID = (byte)TileID.Water},
			new TempItemDef{ Quad=Torch.AnimStates[0], Color = TileDefinitions.Torch.Color, TileID = (byte)TileID.Torch},
			new TempItemDef{ Quad=Torch.AnimStates[0], Color = TileDefinitions.RedTorch.Color, TileID = (byte)TileID.RedTorch},
			new TempItemDef{ Quad=Torch.AnimStates[0], Color = TileDefinitions.BlueTorch.Color, TileID = (byte)TileID.BlueTorch},
			new TempItemDef{ Quad=Torch.AnimStates[0], Color = TileDefinitions.GreenTorch.Color, TileID = (byte)TileID.GreenTorch},
			new TempItemDef{ Quad=Torch.AnimStates[0], Color = TileDefinitions.YellowTorch.Color, TileID = (byte)TileID.YellowTorch},
			new TempItemDef{ Quad=TileMap.Plank, Color = TileDefinitions.OakPlank.Color, TileID = (byte)TileID.OakPlank},
			new TempItemDef{ Quad=TileMap.Ore, Color = TileDefinitions.CopperOre.Color, TileID = (byte)TileID.CopperOre},
			new TempItemDef{ Quad=TileMap.Ore, Color = TileDefinitions.IronOre.Color, TileID = (byte)TileID.IronOre},
			new TempItemDef{ Quad=TileMap.Ore, Color = TileDefinitions.GoldOre.Color, TileID = (byte)TileID.GoldOre},
			new TempItemDef{ Quad=TileMap.Ore, Color = TileDefinitions.CobaltOre.Color, TileID = (byte)TileID.CobaltOre},
			new TempItemDef{ Quad=TileMap.Ore, Color = TileDefinitions.LeadOre.Color, TileID = (byte)TileID.LeadOre},
			new TempItemDef{ Quad=TileMap.Ore, Color = TileDefinitions.UraniumOre.Color, TileID = (byte)TileID.UraniumOre},
		};

		private int lastScroll = 0;

		public void Update(GameTime gt)
		{
			MouseState mouse = Mouse.GetState();

			
			var scroll = (mouse.ScrollWheelValue / 120) - (lastScroll/120); // why is MouseScroll in 120 increments???
			lastScroll = mouse.ScrollWheelValue;

			Index -= scroll;
			Index = Index.Mod(ItemSet.Length);

		}


		public void Draw(SpriteBatch sb)
		{
			sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
			int travel = 0;

			Vector2 pos = new Vector2(GameGlobals.Width - (30* ItemSet.Length), GameGlobals.Height - 30);

			for (int i = 0; i < ItemSet.Length; i++)
			{
				if (Index == i)
				{

					sb.Rect(Color.Gray, pos+new Vector2(travel, -4), new Vector2(32, 32));
					sb.Draw(GameTextures.TileSheet,
						pos+new Vector2(travel, -4), ItemSet[i].Quad, ItemSet[i].Color, 0, Vector2.Zero, 4, SpriteEffects.None, 0);
					travel += 32;
				}
				else
				{
					sb.Rect(Color.Gray, pos + new Vector2(travel, 0), new Vector2(24, 24));
					sb.Draw(GameTextures.TileSheet,
						pos + new Vector2(travel, 0), ItemSet[i].Quad, ItemSet[i].Color, 0, Vector2.Zero, 3, SpriteEffects.None, 0);
					travel += 24;
				}
			}

			sb.End();
		}
	}




	public class GameClient : IGameContext
	{
		public static float CameraZoom = 2.0f;

		public CaveGameGL Game { get; private set; }

		public GameChat Chat { get; private set; }

		public string NetworkUsername { get; set; }
		public string ConnectAddress { get; set; }

		public bool Active { get; set; }

		Game IGameContext.Game => Game;

		public Hotbar Hotbar { get; set; }

		public LocalWorld World { get; private set; }
		private NetworkClient gameClient;
		private GameServer gameServer;

		public Camera2D Camera;

		int MyUserID;
		int MyPlayerID;

		public LocalPlayer myPlayer;

		private DelayedTask chunkUnloadingTask;
		DelayedTask playerStateReplicationTask;

		private int IntitalScrollValue;

		public GameClient(CaveGameGL _game)
		{
			Game = _game;

			MouseState mouse = Mouse.GetState();


			World = new LocalWorld();
			Camera = new Camera2D(Game.GraphicsDevice.Viewport) { Zoom = CameraZoom };
			Chat = new GameChat(this);
			IntitalScrollValue = mouse.ScrollWheelValue;

			playerStateReplicationTask = new DelayedTask(ReplicatePlayerState, 1 / 10.0f);
			chunkUnloadingTask = new DelayedTask(ChunkUnloadingCheck, 1/20.0f);
		}


		public void SendChatMessage(object sender, string message)
		{
			Chat.Open = false;
			gameClient.SendPacket(new ClientChatMessagePacket(message));	
		}
		private IEntity FindEntityOfID(int id)
		{
			foreach (var entity in World.Entities)
				if (entity.EntityNetworkID == id)
					return entity;
			return null;
		}
		public void Disconnect()
		{
			if (myPlayer != null)
			{
				gameClient.SendPacket(new QuitPacket(myPlayer.EntityNetworkID));
			}

			Thread.Sleep(50);
			//gameClient.Stop();
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

		private void ChunkLoadingCheckUpdate(GameTime gameTime)
		{
			int camChunkX = (int)Math.Floor(Camera.ScreenCenterToWorldSpace.X / (Globals.ChunkSize * Globals.TileSize));
			int camChunkY = (int)Math.Floor(Camera.ScreenCenterToWorldSpace.Y / (Globals.ChunkSize * Globals.TileSize));

			int rendr = 1;
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

		#region NetworkListenerMethods


		private void DownloadChunk(NetworkMessage message)
		{
			ChunkDownloadPacket chunkdata = new ChunkDownloadPacket(message.Packet.GetBytes());

			Chunk chunk = chunkdata.StoredChunk;

			// did we ask for it?
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


			//Debug.WriteLine(String.Format("CTile {0} {1} id {2}", packet.WorldX, packet.WorldY, packet.TileID));
			t.Damage = packet.Damage;
			t.TileState = packet.TileState;
			World.SetTile(packet.WorldX, packet.WorldY, t);
		}
		private void OnPeerJoined(NetworkMessage message)
		{
			PlayerJoinedPacket packet = new PlayerJoinedPacket(message.Packet.GetBytes());

			var player = new Player()
			{
				EntityNetworkID = packet.EntityID,
				Color = packet.PlayerColor,
				DisplayName = packet.Username,
				NotMyProblem = true
			};
			World.Entities.Add(player);
		}
		private void OnPeerLeft(NetworkMessage message)
		{
			PlayerLeftPacket packet = new PlayerLeftPacket(message.Packet.GetBytes());

			var entity = FindEntityOfID(packet.EntityID);

			if (entity != null)
			{
				World.Entities.Remove(entity);
			}
		}
		private void OnPeerPosition(NetworkMessage message)
		{
			EntityPositionPacket packet = new EntityPositionPacket(message.Packet.GetBytes());

			var entity = FindEntityOfID(packet.EntityID);

			if (entity == null)
				return;



			if (entity is Player plr )
			{
				if (packet.EntityID == MyPlayerID)
					return;

				plr.NextPosition = new Vector2(packet.NextX, packet.NextY);
				return;
			}
	

			if (entity is IPositional positional)
				positional.Position = new Vector2(packet.PosX, packet.PosY);

			if (entity is IVelocity velocity)
				velocity.Velocity = new Vector2(packet.VelX, packet.VelY);

			if (entity is INextPosition next)
				next.NextPosition = new Vector2(packet.NextX, packet.NextY);
		}
		private void OnChatMessage(NetworkMessage message)
		{
			ServerChatMessagePacket packet = new ServerChatMessagePacket(message.Packet.GetBytes());
			Chat.AddMessage(packet.Message, packet.TextColor);
		}
		private void OnPlayerState(NetworkMessage message)
		{
			PlayerStatePacket packet = new PlayerStatePacket(message.Packet.GetBytes());

			IEntity ent = FindEntityOfID(packet.EntityID);

			if (ent is Player plr)
			{
				plr.Walking = packet.Walking;
				plr.OnGround = packet.OnGround;
				plr.Facing = packet.Facing;
			}
		}
		private void OnServerDenyJoining(NetworkMessage message)
		{
			RejectJoinPacket packet = new RejectJoinPacket(message.Packet.GetBytes());
			// TODO: send player to the rejection screen

			Game.CurrentGameContext = Game.ServerKickedContext;
			Game.ServerKickedContext.Message = packet.RejectReason;
		}
		private void OnServerAcceptJoining(NetworkMessage message)
		{
			AcceptJoinPacket packet = new AcceptJoinPacket(message.Packet.GetBytes());


			MyUserID = packet.YourUserNetworkID;
			MyPlayerID = packet.YourPlayerNetworkID;

			// create me player
			LocalPlayer myplayer = new LocalPlayer();
			myplayer.EntityNetworkID = MyPlayerID;
			myPlayer = myplayer;
			World.Entities.Add(myplayer);
		}


		#endregion

		private void ReadIncomingPackets()
		{
			while (gameClient.HaveIncomingMessage())
			{
				NetworkMessage msg = gameClient.GetLatestMessage();

				if (msg.Packet.Type == PacketType.SChatMessage)
					OnChatMessage(msg);
				if (msg.Packet.Type == PacketType.PlayerState)
					OnPlayerState(msg);
				if (msg.Packet.Type == PacketType.SDownloadChunk)
					DownloadChunk(msg);
				if (msg.Packet.Type == PacketType.PlaceTile)
					UpdateTile(msg);
				if (msg.Packet.Type == PacketType.SDenyJoin)
					OnServerDenyJoining(msg);
				if (msg.Packet.Type == PacketType.SAcceptJoin)
					OnServerAcceptJoining(msg);
				if (msg.Packet.Type == PacketType.SPlayerJoined)
					OnPeerJoined(msg);
				if (msg.Packet.Type == PacketType.SPlayerLeft)
					OnPeerLeft(msg);
				if (msg.Packet.Type == PacketType.SPlayerPosition)
					OnPeerPosition(msg);
			}
		}


		private void DrawChunkGridLines(SpriteBatch spriteBatch)
		{
			int camChunkX = (int)Math.Floor(Camera.ScreenCenterToWorldSpace.X / (Globals.ChunkSize * Globals.TileSize));
			int camChunkY = (int)Math.Floor(Camera.ScreenCenterToWorldSpace.Y / (Globals.ChunkSize * Globals.TileSize));

			int rendr = 4;

			for (int x = -rendr; x <= rendr; x++)
			{
				spriteBatch.Line(Color.White, new Vector2(
					(camChunkX + x) * (Globals.ChunkSize * Globals.TileSize),
					(camChunkY - rendr) * (Globals.ChunkSize * Globals.TileSize)
				), new Vector2(
					(camChunkX + x) * (Globals.ChunkSize * Globals.TileSize),
					(camChunkY + rendr) * (Globals.ChunkSize * Globals.TileSize)
				), 0.5f);
			}

			for (int y = -rendr; y <= rendr; y++)
			{
				spriteBatch.Line(Color.White, new Vector2(
					(camChunkX - rendr) * (Globals.ChunkSize * Globals.TileSize),
					(camChunkY + y) * (Globals.ChunkSize * Globals.TileSize)
				), new Vector2(
					(camChunkX + rendr) * (Globals.ChunkSize * Globals.TileSize),
					(camChunkY + y) * (Globals.ChunkSize * Globals.TileSize)
				), 0.5f);
			}

			for (int x = -rendr; x <= rendr; x++)
			{
				for (int y = -rendr; y <= rendr; y++)
				{
					var pos = new Vector2(camChunkX + x, camChunkY + y) * (Globals.ChunkSize * Globals.TileSize);
					spriteBatch.Print(GameFonts.Arial8, new Color(1, 1, 1, 0.5f), pos, (camChunkX + x) + ", " + (camChunkY + y));
				}
			}

		}

		
		private void DrawChunks(SpriteBatch sb)
		{
			if (drawTimer > (1/2.0f))
			{
				drawTimer = 0;
				Chunk.RefreshedThisFrame = false;
				foreach (var chunkpair in World.Chunks)
				{
					chunkpair.Value.Draw(GameTextures.TileSheet, Game.GraphicsDevice, sb);	
				}
			}
		}

		private void EntityRendering(SpriteBatch sb)
		{
			foreach (Entity entity in World.Entities)
			{
				//Re.Draw(sb);
				//if (entity is LocalPlayer player)
				//{
					//sb.Rect(Color.Red, player.TopLeft, player.BoundingBox * 2);
				//}
				if (entity is Player plr)
				{
					plr.Draw(sb);
					//sb.Rect(plr.Color, plr.TopLeft, plr.BoundingBox * 2);
				}
			}
		}

		private void DrawChunkFGTextures(SpriteBatch sb)
		{
			foreach (var chunkpair in World.Chunks)
			{
				
				Chunk chunk = chunkpair.Value;
				Vector2 pos = new Vector2(chunk.Coordinates.X * Globals.ChunkSize * Globals.TileSize, chunk.Coordinates.Y * Globals.ChunkSize * Globals.TileSize);
				if (chunk.ForegroundRenderBuffer != null)
					sb.Draw(chunk.ForegroundRenderBuffer, pos, Color.White);

			}
		}
		private void DrawChunkBGTextures(SpriteBatch sb)
		{
			foreach (var chunkpair in World.Chunks)
			{

				Chunk chunk = chunkpair.Value;
				Vector2 pos = new Vector2(chunk.Coordinates.X * Globals.ChunkSize * Globals.TileSize, chunk.Coordinates.Y * Globals.ChunkSize * Globals.TileSize);

				if (chunk.BackgroundRenderBuffer != null)
					sb.Draw(chunk.BackgroundRenderBuffer, pos, Color.White);
			}
		}
		private void DrawDebugInfo(SpriteBatch sb)
		{
			sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
			MouseState mouse = Mouse.GetState();

			var mp = Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			mp /= 8;
			mp.Floor();
			var tileCoords = mp;
			mp *= 8;


			var tileat = World.GetTile((int)tileCoords.X, (int)tileCoords.Y);
			var wallat = World.GetWall((int)tileCoords.X, (int)tileCoords.Y);


			if (myPlayer != null)
			{
				sb.Print(Color.White, new Vector2(2, 12),
					String.Format("pos {0} {1} vel {2} {3}",
						Math.Floor(myPlayer.Position.X / Globals.TileSize),
						Math.Floor(myPlayer.Position.Y / Globals.TileSize),
						Math.Round(myPlayer.Velocity.X, 2),
						Math.Round(myPlayer.Velocity.Y, 2)
					)
				);
				sb.Print(Color.White, new Vector2(2, 24),
					String.Format("pin {0}, pout {1} myid {2}",
						gameClient.ReceivedCount,
						gameClient.SentCount,
						myPlayer?.EntityNetworkID
					)
				);
				sb.Print(Color.White, new Vector2(2, 36),
					String.Format("tid {0}, state {1} tdmg {2} wid {3} wdmg {4} light {5}",
						tileat.ID,
						tileat.TileState,
						tileat.Damage,
						wallat.ID,
						wallat.Damage,
						World.GetLight((int)tileCoords.X, (int)tileCoords.Y).ToString()
					)
				);

			}
			sb.End();
		}


		private SpriteBatch sbReference;
		public void Draw(SpriteBatch sb)
		{
			if (sbReference == null)
				sbReference = sb;


			DrawChunks(sb);

			Game.GraphicsDevice.Clear(Color.CornflowerBlue);
			sb.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera.View);
			EntityRendering(sb);
			DrawChunkFGTextures(sb);

			MouseState mouse = Mouse.GetState();

			var mp = Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			mp /= 8;
			mp.Floor();
			var tileCoords = mp;
			mp *= 8;


			if (mouse.LeftButton == ButtonState.Pressed)
			{
				sb.Rect(Color.Green, mp, new Vector2(8, 8));
			} else
			{
				sb.Rect(new Color(1,1,1,0.5f), mp, new Vector2(8, 8));
			}

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.View);
			DrawChunkGridLines(sb);
			sb.End();

			Hotbar.Draw(sb);

			DrawDebugInfo(sb);
			Chat.Draw(sb);
		}

		public void Load()
		{
			Hotbar = new Hotbar();
			ServerConfig localServerConf = new ServerConfig
			{
				Port = 40269,
				World = "LocalWorld",
				MaxPlayers = 2,
				ServerName = "LocalServer",
				TickRate = 1,
			};

#if SERVER
			gameServer = new GameServer(localServerConf);
			gameServer.Output = Game.Console;

			gameServer.Start();
			gameServer.ClientRun();
#endif
			//	gameClient = new NetworkClient("72.243.56.7", 40269);
			gameClient = new NetworkClient(ConnectAddress);

			gameClient.Output = Game.Console;
			gameClient.Start();
			gameClient.SendPacket(new RequestJoinPacket(NetworkUsername));
		}

		public void Unload() {}

		private void TilePlacementUpdate(GameTime gt)
		{
			MouseState mouse = Mouse.GetState();
			
			if (mouse.LeftButton == ButtonState.Pressed)
			{
				var mp = Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

				mp /= 8;
				mp.Floor();
				int x = (int)mp.X;
				int y = (int)mp.Y;
				byte tileID = Hotbar.ItemSet[Hotbar.Index].TileID;
				if (World.GetTile(x, y).ID != tileID)
				{
					gameClient?.SendPacket(new PlaceTilePacket(tileID, 0,0, x, y));
					World.SetTile(x, y, Tile.FromID(tileID));
				}
				

			} else if (mouse.RightButton == ButtonState.Pressed)
			{
				var mp = Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

				mp /= 8;
				mp.Floor();
				int x = (int)mp.X;
				int y = (int)mp.Y;
				if (World.GetTile(x, y).ID != 0)
				{
					World.SetTile(x, y, new Air());
					gameClient?.SendPacket(new PlaceTilePacket(0, 0, 0, x, y));
				}
			}
		}


		private void ReplicatePlayerState()
		{
			//Debug.WriteLine("Replicating");
			if (myPlayer != null)
			{
				gameClient?.SendPacket(
					new EntityPositionPacket(myPlayer.EntityNetworkID, myPlayer.Position.X, myPlayer.Position.Y,
						myPlayer.Velocity.X, myPlayer.Velocity.Y, myPlayer.NextPosition.X, myPlayer.NextPosition.Y)
				);

				gameClient?.SendPacket(new PlayerStatePacket(myPlayer.Facing, myPlayer.OnGround, myPlayer.Walking));
			}
		}



		private void UpdateCamera(GameTime gt)
		{
			MouseState mouse = Mouse.GetState();


			float Senitivity = (float)0.05;
			float ZoomFactor = ((mouse.ScrollWheelValue - IntitalScrollValue) * (Senitivity / 120)) + 2;

			Vector2 MouseCameraMovement = ((mouse.Position.ToVector2() / Camera._screenSize) - new Vector2((float)0.5, (float)0.5)) * (float)0.1;

			Camera.Zoom = ZoomFactor;
			Camera.Position += MouseCameraMovement;

			if (myPlayer != null)
			{
				if (((Camera.Position - MouseCameraMovement) - myPlayer.Position).Length() < 500f)
				{
					float speed = (float)(gt.ElapsedGameTime.TotalSeconds * 10.0);
					Camera.Position = Camera.Position.Lerp(myPlayer.Position, speed);
				}
				else
				{
					Camera.Position = myPlayer.Position;
				}
			}
		}

		float drawTimer = 0;
		public void Update(GameTime gt)
		{

			drawTimer += (float)gt.ElapsedGameTime.TotalSeconds;
			
			playerStateReplicationTask.Update(gt);
			chunkUnloadingTask.Update(gt);

			Chat.Update(gt);
			Hotbar.Update(gt);
			World.Update(gt);

			
			TilePlacementUpdate(gt);
			UpdateCamera(gt);
			//gameServer?.Update(gt);
			ChunkLoadingCheckUpdate(gt);
			//ChunkUnloadingCheckUpdate(gt);
			ReadIncomingPackets();


			if (Keyboard.GetState().IsKeyDown(Keys.Escape))
				Game.CurrentGameContext = Game.HomePageContext;
		}
	}
}
