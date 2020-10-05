//#define SERVER

using Cave;
using CaveGame.Core;
using CaveGame.Core.Entities;
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using CaveGame.Server;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CaveGame.Client
{

	// The testbench for building the server/client archite ture

	public class GameClient : IGameContext
	{
		public static float CameraZoom = 2.0f;

		public CaveGameGL Game { get; private set; }

		public string NetworkUsername { get; set; }
		public string ConnectAddress { get; set; }

		public bool Active { get; set; }

		Game IGameContext.Game => Game;

		public LocalWorld World { get; private set; }
		private NetworkClient gameClient;
		private GameServer gameServer;

		public Camera2D Camera;

		int MyUserID;
		int MyPlayerID;

		public LocalPlayer myPlayer;

		float chunkUnloadCheck;

		public GameClient(CaveGameGL _game)
		{
			Game = _game;

			chunkUnloadCheck = 0;

			World = new LocalWorld();
			Camera = new Camera2D(Game.GraphicsDevice.Viewport) { Zoom = CameraZoom };
			//
		}

		#region network listeners
		private void ChunkUnloadingCheckUpdate(GameTime gameTime)
		{
			chunkUnloadCheck += (float)gameTime.ElapsedGameTime.TotalSeconds;

		//	if (chunkUnloadCheck > (1 / 5.0f))
		//	{
			//	chunkUnloadCheck = 0;

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

			int rendr = 2;
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
		#endregion
		private void NetworkUpdate()
		{
			while (gameClient.HaveIncomingMessage())
			{
				NetworkMessage msg = gameClient.GetLatestMessage();

				if (msg.Packet.Type == PacketType.SDownloadChunk)
				{
					DownloadChunk(msg);
					return;
				}

				if (msg.Packet.Type == PacketType.PlaceTile)
				{
					UpdateTile(msg);
					return;
				}

				if (msg.Packet.Type == PacketType.SDenyJoin)
				{
					RejectJoinPacket packet = new RejectJoinPacket(msg.Packet.GetBytes());
					// TODO: send player to the rejection screen

					Game.CurrentGameContext = Game.ServerKickedContext;
					Game.ServerKickedContext.Message = packet.RejectReason;
				}

				if (msg.Packet.Type == PacketType.SAcceptJoin)
				{
					AcceptJoinPacket packet = new AcceptJoinPacket(msg.Packet.GetBytes());


					MyUserID = packet.YourUserNetworkID;
					MyPlayerID = packet.YourPlayerNetworkID;

					// create me player
					LocalPlayer myplayer = new LocalPlayer();
					myplayer.EntityNetworkID = MyPlayerID;
					myPlayer = myplayer;
					World.Entities.Add(myplayer);
				}

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
				));
			}

			for (int y = -rendr; y <= rendr; y++)
			{
				spriteBatch.Line(Color.White, new Vector2(
					(camChunkX - rendr) * (Globals.ChunkSize * Globals.TileSize),
					(camChunkY + y) * (Globals.ChunkSize * Globals.TileSize)
				), new Vector2(
					(camChunkX + rendr) * (Globals.ChunkSize * Globals.TileSize),
					(camChunkY + y) * (Globals.ChunkSize * Globals.TileSize)
				));
			}

			for (int x = -rendr; x <= rendr; x++)
			{
				for (int y = -rendr; y <= rendr; y++)
				{
					var pos = new Vector2(camChunkX + x, camChunkY + y) * (Globals.ChunkSize * Globals.TileSize);
					spriteBatch.Print(new Color(1, 1, 1, 0.5f), pos, (camChunkX + x) + ", " + (camChunkY + y));
				}
			}

		}


		private void DrawChunks(SpriteBatch sb)
		{
			// Poll chunk render buffer;
			Chunk.RefreshedThisFrame = false;
			foreach (var chunkpair in World.Chunks)
			{
				chunkpair.Value.Draw(CaveGameGL.tiles, Game.GraphicsDevice, sb);
			}
		}

		private void EntityRendering(SpriteBatch sb)
		{
			foreach (Entity entity in World.Entities)
			{
				//Re.Draw(sb);
				if (entity is LocalPlayer player)
				{
					sb.Rect(Color.Red, player.TopLeft, player.BoundingBox * 2);
				}
				else if (entity is Player plr)
				{
					sb.Rect(plr.Color, plr.TopLeft, plr.BoundingBox * 2);
				}
			}
		}

		private void DrawChunkCachedTextures(SpriteBatch sb)
		{
			foreach (var chunkpair in World.Chunks)
			{
				
				Chunk chunk = chunkpair.Value;
				Vector2 pos = new Vector2(chunk.Coordinates.X * Globals.ChunkSize * Globals.TileSize, chunk.Coordinates.Y * Globals.ChunkSize * Globals.TileSize);
				if (chunk.RenderBuffer != null)
				{
					sb.Draw(
						chunk.RenderBuffer,
						pos,
						Color.White
					);
				}
				
			}
		}

		public void Draw(SpriteBatch sb)
		{
			DrawChunks(sb);

			Game.GraphicsDevice.Clear(Color.CornflowerBlue);
			sb.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera.View);
			DrawChunkCachedTextures(sb);
			EntityRendering(sb);

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

			sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

			var tileat = World.GetTile((int)tileCoords.X, (int)tileCoords.Y);


			if (myPlayer != null)
			{
				sb.Print(Color.White, new Vector2(2, 12),
					String.Format("pos {0} {1} vel {2} {3}", 
						Math.Floor(myPlayer.Position.X/Globals.TileSize), 
						Math.Floor(myPlayer.Position.Y/Globals.TileSize), 
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
					String.Format("tid {0}, state {1} dmg {2} light {3}",
						tileat.ID,
						tileat.TileState,
						tileat.Damage,
						World.GetLight((int)tileCoords.X, (int)tileCoords.Y).ToString()
					)
				);

			}
			sb.End();
		}

		public void Load()
		{

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

		public void Unload()
		{

		}

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
				byte stoneID = (byte)TileID.Stone;
				if (World.GetTile(x, y).ID != stoneID)
				{
					gameClient?.SendPacket(new PlaceTilePacket(stoneID, 0,0, x, y));
					World.SetTile(x, y, Tile.FromID(stoneID));
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

		float sendpostimer;

		public void Update(GameTime gt)
		{
			World.Update(gt);
			float delta = (float)gt.ElapsedGameTime.TotalSeconds;
			sendpostimer += delta;

			if (sendpostimer > (1/20.0f))
			{
				sendpostimer = 0;
				if (myPlayer != null)
				{
					gameClient?.SendPacket(
						new EntityPositionPacket(myPlayer.EntityNetworkID, myPlayer.Position.X, myPlayer.Position.Y,
							myPlayer.Velocity.X, myPlayer.Velocity.Y, myPlayer.NextPosition.X, myPlayer.NextPosition.Y)
					);
				}
			}

			KeyboardState kb = Keyboard.GetState();
			TilePlacementUpdate(gt);
			// exit game
			if (kb.IsKeyDown(Keys.Escape))
				Game.CurrentGameContext = Game.HomePageContext;

			if (myPlayer != null) {
				if ( (Camera.Position - myPlayer.Position).Length() < 500f )
				{
					float speed = (float)(gt.ElapsedGameTime.TotalSeconds * 10.0);
					Camera.Position = Camera.Position.Lerp(myPlayer.Position, speed);
				} else
				{
					Camera.Position = myPlayer.Position;
				}
			}

			gameServer?.Update(gt);
			ChunkLoadingCheckUpdate(gt);
			ChunkUnloadingCheckUpdate(gt);
			NetworkUpdate();
		}
	}
}
