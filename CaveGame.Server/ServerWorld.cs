using CaveGame.Common;
using CaveGame.Common.Game.Entities;
using CaveGame.Common.Game.Furniture;
using CaveGame.Common.Generic;
using CaveGame.Common.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Concurrent;
using CaveGame.Common.Game.Tiles;
using CaveGame.Common.Extensions;
using CaveGame.Common.World;

namespace CaveGame.Server
{
	public class ServerWorld : Common.GameWorld, IServerWorld
	{
		public int WorldSeed => Metadata.Seed;
		public string WorldName => Metadata.Name;

		private ConcurrentQueue<IEntity> EntityQueue { get; set; }
		public void SpawnEntity(IEntity entity) => EntityQueue.Enqueue(entity);

		private Queue<Point> TileNetworkUpdateQueue { get; set; }
		private Queue<Point> WallNetworkUpdateQueue { get; set; }

		public void RequestTileNetworkUpdate(Point position) => TileNetworkUpdateQueue.Enqueue(position);
		public void RequestWallNetworkUpdate(Point position) => WallNetworkUpdateQueue.Enqueue(position);

		public GameServer Server { get; set; }
		public Generator Generator { get; set; }

		public Dictionary<ChunkCoordinates, bool> LoadedChunks;

		protected RepeatingIntervalTask TileUpdateTask { get; set; }
		protected RepeatingIntervalTask RandomTileUpdateTask { get; set; }
		//protected RepeatingIntervalTask 

		

		public override List<FurnitureTile> Furniture { get; protected set; }

		public WorldMetadata Metadata { get; private set; }




		protected void WriteWorldDataToMetadata()
		{
			// Serialize world info into file.
			XmlWriter worldXml = XmlWriter.Create(Path.Combine("Worlds", WorldName, "WorldMetadata.xml"));
			worldXml.WriteStartDocument();
			worldXml.WriteStartElement("Metadata");

			worldXml.WriteElementString("Name", WorldName);
			worldXml.WriteElementString("Seed", WorldSeed.ToString());
			worldXml.WriteElementString("TimeOfDay", TimeOfDay.ToString());
			worldXml.WriteElementString("LastPlayed", DateTime.Now.ToString());
			worldXml.WriteElementString("LastVersion", Globals.CurrentVersionFullString);

			worldXml.WriteEndDocument();
			worldXml.Close();
		}


		public ServerWorld(WorldMetadata metadata) : base()
		{
			TileNetworkUpdateQueue = new Queue<Point>();
			WallNetworkUpdateQueue = new Queue<Point>();

			Context = NetworkContext.Server;
			//SessionType = GameSessionType.


			Metadata = metadata;

            CreateDirectoryIfNull(Path.Combine("Worlds", WorldName));
            CreateDirectoryIfNull(Path.Combine("Worlds", WorldName, "Chunks"));

			WriteWorldDataToMetadata();
            
			WorldTimedTasks.Add(new(ProcessTileUpdateRequests, 1 / 10.0f));
			WorldTimedTasks.Add(new(DoRandomTileTicks, 1 / 5.0f));
			WorldTimedTasks.Add(new(SendTileNetworkUpdates, 1 / 5.0f));


			EntityQueue = new ConcurrentQueue<IEntity>();
			//serverTileUpdateTask = new RepeatingIntervalTask(TileUpdates, 1 / 10.0f);
			
			//serverRandomTileUpdateTask = new RepeatingIntervalTask(ApplyRandomTileTicksToLoadedChunks, 1 / 5.0f);
			Generator = new Generator(WorldSeed);
			Tile.InitializeManager(WorldSeed);
		}


        public override void SetTile(int x, int y, Tile t)
        {
            base.SetTile(x, y, t);
			RequestTileNetworkUpdate(new Point(x, y));
        }

		public virtual void SetTileNetworkUpdated(int x, int y)
		{
			//throw new NotImplementedException();
		}

		public override void BreakTile(int x, int y)
		{
			GetTile(x, y).Drop(Server, this, new Point(x, y));
			SetTile(x, y, new Air());
			base.BreakTile(x, y);
		}

		private void ProcessTileUpdateRequests()
		{
			
			int count = TileUpdateQueue.Count;
			for (int i = 0; i < count-1; i++)
            {
				Point coords = TileUpdateQueue.Dequeue();
				//bool success = TileUpdateQueue.TryDequeue(out coords);
				if (GetTile(coords) is ITileUpdate tile)
					tile.TileUpdate(this, coords.X, coords.Y);


				foreach (var furn in Furniture.ToArray())
					furn.OnTileUpdate(this, coords.X, coords.Y);
			}
		}

		private static void CreateDirectoryIfNull(string fname)
		{
			if (!System.IO.Directory.Exists(fname))
				System.IO.Directory.CreateDirectory(fname);
		}

		public void SaveData()
		{


			CreateDirectoryIfNull(Path.Combine("Worlds", WorldName));

			WriteWorldDataToMetadata();

			foreach (var kvp in Chunks)
			{
				Chunk chunk = kvp.Value;
				File.WriteAllBytes(Path.Combine("Worlds", WorldName, "Chunks", kvp.Key.GetHashCode().ToString()), chunk.ToData());
			}


			//XmlSerializer writer = new XmlSerializer(typeof(FurnitureList));
			//using (FileStream furniture = new FileStream(@"Worlds\" + WorldName +@"\furniture.xml", FileMode.Create))
			//{
			//	writer.Serialize(furniture, new FurnitureList { Furniture = this.Furniture });
			//}
				
		}

		private bool HasChunkOnFile(ChunkCoordinates coords)
		{
			return File.Exists(Path.Combine("Worlds", WorldName, "Chunks",  coords.GetHashCode().ToString()));
		}

		private Chunk RetrieveChunkFromFile(ChunkCoordinates coords)
		{
			Chunk chunk = new Chunk(coords.X, coords.Y);
			chunk.FromData(
				File.ReadAllBytes(
					Path.Combine("Worlds", WorldName, "Chunks", coords.GetHashCode().ToString())
				)
			);

			return chunk;
		}

		public Chunk RetrieveChunk(ChunkCoordinates coords)
		{
			Chunk chunk;
			if (Chunks.ContainsKey(coords))
			{
				chunk = Chunks.GetValueOrDefault(coords);
			} else if (HasChunkOnFile(coords)) {
				chunk = RetrieveChunkFromFile(coords);

				Chunks.TryAdd(coords, chunk);
			}
			else
			{
				chunk = new Chunk(coords.X, coords.Y);
				Generator.HeightPass(ref chunk);
				//World.Chunks.Add(coords, chunk);
				Chunks.TryAdd(coords, chunk);
			}

			if (!chunk.DungeonPassCompleted)
			{
				chunk.DungeonPassCompleted = true;
				// make sure all adjacent chunks are loaded
				for (int x = -1; x <= 1; x++)
				{
					for (int y = -1; y <= 1; y++)
					{
						var newCoords = new ChunkCoordinates(coords.X + x, coords.Y + y);
						if (!Chunks.ContainsKey(newCoords))
						{
							var thischunk = new Chunk(coords.X + x, coords.Y + y);
							//World.Chunks.Add(newCoords, thischunk);
							Chunks.TryAdd(newCoords, thischunk);
							Generator.HeightPass(ref thischunk);
						}
					}
				}

				for (int x = 0; x < Chunk.ChunkSize; x++)
				{
					for (int y = 0; y < Chunk.ChunkSize; y++)
					{
						Generator.StructurePass(this, coords.X * Chunk.ChunkSize + x, coords.Y * Chunk.ChunkSize + y);
					}
				}
			}
			return chunk;
		}


		public Task<Chunk> LoadChunk(ChunkCoordinates coords) { return null; }// TODO: Implement
		public Task<bool> UnloadChunk(ChunkCoordinates coords) { return null; }// TODO: Implement

		protected override void PhysicsStep()
		{
			foreach (IEntity entity in Entities.ToArray())
				if (entity is IServerPhysicsObserver physicsObserver)
					physicsObserver.ServerPhysicsTick(this, PhysicsStepIncrement);
		}

		
        public override void RemoveFurniture(FurnitureTile furn)
		{
			
			Server.SendToAll(new RemoveFurniturePacket(furn.FurnitureNetworkID));
			Furniture.Remove(furn);
			//base.RemoveFurniture(furn);
		}

		private void SendTileNetworkUpdates()
        {
			foreach (var tileCoords in TileNetworkUpdateQueue.DequeueAll())
				Server.SendToAll(new PlaceTilePacket(tileCoords, GetTile(tileCoords)));

			foreach (var wallCoords in WallNetworkUpdateQueue.DequeueAll())
				Server.SendToAll(new PlaceWallPacket(wallCoords, GetWall(wallCoords)));
		}

        

        private async void DoRandomTileTicks()
		{
			await (Task.Run(() =>
		{
				 Random rng = new Random();
				 int UpdatesPerChunk = 15;
				 foreach (var chunkKeyValuePair in Chunks)
				 {
					 Chunk chunk = chunkKeyValuePair.Value;

					 for (int i = 0; i < UpdatesPerChunk; i++)
					 {
						 int x = rng.Next(Globals.ChunkSize);
						 int y = rng.Next(Globals.ChunkSize);
						 Tile t = chunk.GetTile(x, y);

						 int worldX = (chunk.Coordinates.X * Globals.ChunkSize) + x;
						 int worldY = (chunk.Coordinates.Y * Globals.ChunkSize) + y;

						 if (t is RandomTileTickable valid)
							 valid.RandomTick(this, worldX, worldY);
					 }
				 }
			}
			));
		}

        protected override void InflictExplosionDamageOnEntity(Explosion Blast, IEntity Entity, int Damage, Vector2 Direction, float Kickback)
		{
			Entity.Damage(
				type: DamageType.Explosion,
				source: Blast,
				amount: Damage,
				direction: Direction
			);

			base.InflictExplosionDamageOnEntity(Blast, Entity, Damage, Direction, Kickback);
        }

        //public void Explosion(Explosion Blast) => Explosion(Blast, true, true);
        public override void Explosion(Explosion Blast, bool DamageTiles, bool DamageEntities)
		{
			Server.SendToAll(new ExplosionPacket(Blast, DamageTiles, DamageEntities));
			base.Explosion(Blast, DamageTiles, DamageEntities);
		}

		public override void OnCollectDeadEntity(IEntity ent)
		{
			Server.SendToAll(new DropEntityPacket(ent.EntityNetworkID));
			base.OnCollectDeadEntity(ent);
		}

		public override void Update(GameTime gt)
		{

			for (int i = 0; i < EntityQueue.Count; i++)
				if (EntityQueue.TryDequeue(out IEntity entity))
					Entities.Add(entity);


			foreach (var ent in Entities.ToArray())
				ent.ServerUpdate(Server, gt);
				

			base.Update(gt);
		}
	}
}
