using CaveGame.Core;
using CaveGame.Core.Entities;
using CaveGame.Core.Furniture;
using CaveGame.Core.Generic;
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CaveGame.Server
{
	public class WorldConfiguration
	{
		public int Seed { get; set; }
		public string Name { get; set; }
		public float TimeOfDay { get; set; }

		[XmlArray("FurnitureList")]
		[XmlArrayItem("Furniture")]
		public List<FurnitureTile> Furniture { get; set; }
	}

	public class ServerWorld : Core.World, IServerWorld
	{
		public int WorldSeed { get; }
		public string WorldName { get; }
		public GameServer Server { get; set; }
		public Generator Generator { get; set; }

		public Dictionary<ChunkCoordinates, bool> LoadedChunks;

		protected DelayedTask serverTileUpdateTask;
		protected DelayedTask serverRandomTileUpdateTask;


		public override List<FurnitureTile> Furniture { get; protected set; }

		private ServerWorld() : base()
		{
			serverTileUpdateTask = new DelayedTask(ApplyTileUpdates, 1 / 10.0f);
			serverRandomTileUpdateTask = new DelayedTask(ApplyRandomTileTicksToLoadedChunks, 1 / 5.0f);
			Generator = new Generator();
		}


		public ServerWorld(WorldConfiguration config) : this()
		{
			WorldName = config.Name;
			WorldSeed = config.Seed;

			CreateDirectoryIfNull(@"Worlds");
			CreateDirectoryIfNull(@"Worlds\" + WorldName);
			CreateDirectoryIfNull(@"Worlds\" + WorldName + @"\Chunks");

			// Serialize world info into file.
			XmlWriter worldXml = XmlWriter.Create(@"Worlds\" + WorldName + @"\WorldMetadata.xml");
			worldXml.WriteStartDocument();
			worldXml.WriteStartElement("Metadata");

			worldXml.WriteElementString("Name", WorldName);
			worldXml.WriteElementString("Seed", WorldSeed.ToString());
			worldXml.WriteElementString("TimeOfDay", TimeOfDay.ToString());


			worldXml.WriteEndDocument();
			worldXml.Close();
		}

		// Load from file 
		public ServerWorld(string worldname) : this()
		{


			XmlDocument worldmeta = new XmlDocument();
			worldmeta.Load(@"Worlds\" + worldname + @"\WorldMetadata.xml");

			WorldName = worldname;
			WorldSeed = Int32.Parse(worldmeta["Metadata"]["Seed"].InnerText);
		}

		private void CreateDirectoryIfNull(string fname)
		{
			if (!System.IO.Directory.Exists(fname))
				System.IO.Directory.CreateDirectory(fname);
		}

		public void SaveData()
		{
			foreach(var kvp in Chunks)
			{
				Chunk chunk = kvp.Value;
				File.WriteAllBytes(@"Worlds\" + WorldName + @"\Chunks\" + kvp.Key.GetHashCode(), chunk.ToData());
			}


			//XmlSerializer writer = new XmlSerializer(typeof(FurnitureList));
			//using (FileStream furniture = new FileStream(@"Worlds\" + WorldName +@"\furniture.xml", FileMode.Create))
			//{
			//	writer.Serialize(furniture, new FurnitureList { Furniture = this.Furniture });
			//}
				
		}

		private bool HasChunkOnFile(ChunkCoordinates coords)
		{
			return File.Exists(@"Worlds\" + WorldName + @"\Chunks\" + coords.GetHashCode());
		}

		private Chunk RetrieveChunkFromFile(ChunkCoordinates coords)
		{
			Chunk chunk = new Chunk(coords.X, coords.Y);
			chunk.FromData(File.ReadAllBytes(@"Worlds\" + WorldName + @"\Chunks\" + coords.GetHashCode()));

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
				chunk.ClearUpdateQueue();
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
							thischunk.ClearUpdateQueue();
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


		public override void RemoveFurniture(Core.Furniture.FurnitureTile furn)
		{
			
			Server.SendToAll(new RemoveFurniturePacket(furn.FurnitureNetworkID));
			Furniture.Remove(furn);
			//base.RemoveFurniture(furn);
		}


		private void TileUpdates(Chunk chunk)
		{
			for (int x = 0; x < Globals.ChunkSize; x++)
			{
				for (int y = 0; y < Globals.ChunkSize; y++)
				{
					if (chunk.TileUpdate[x, y] == true)
					{
						chunk.TileUpdate[x, y] = false;

						int worldX = (chunk.Coordinates.X * Globals.ChunkSize) + x;
						int worldY = (chunk.Coordinates.Y * Globals.ChunkSize) + y;
						foreach (var furn in Furniture.ToArray())
							furn.OnTileUpdate(this, worldX, worldY);


						if (chunk.GetTile(x, y) is ITileUpdate tile)
							tile.TileUpdate(this, worldX, worldY);
					}
				}
			}
		}

		private void ApplyTileUpdates()
		{
			foreach (var kvp in Chunks)
				TileUpdates(kvp.Value);
		}

		private async void ApplyRandomTileTicksToLoadedChunks()
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

						 if (t is IRandomTick valid)
							 valid.RandomTick(this, worldX, worldY);
					 }
				 }
			}
			));
		}

		public override void Explosion(Vector2 pos, float strength, float radius, bool damageTiles, bool damageEntities)
		{
			Server.SendToAll(new ExplosionPacket(pos, radius, strength, damageTiles, damageEntities));

			if (damageEntities)
			{
				foreach (var ent in Entities)
				{
					if (ent is IPositional entpos && ent is IVelocity vel)
					{

						var dist = (entpos.Position - pos).Length();
						var power = Math.Min((1 / dist) * strength * 8, 300);
						var unitVec = (entpos.Position - pos);
						unitVec.Normalize();

						vel.Velocity += unitVec * power;// * power);

					}
				}
			}

			if (damageTiles)
			{
				for (int x = -12; x < 12; x++)
				{
					for (int y = -12; y < 12; y++)
					{
						Vector2 thisPosVec = pos + new Vector2(x * Globals.TileSize, y * Globals.TileSize);

						float dist = (thisPosVec - pos).Length() / Globals.TileSize;

						var damage = Math.Max((strength*3) - (dist*3), 0);

						var centroid = new Point((int)pos.X / Globals.TileSize, (int)pos.Y / Globals.TileSize) + new Point(x, y);
						var tile = GetTile(centroid.X, centroid.Y);
						tile.Damage += (byte)Math.Ceiling(damage);

						if (tile.Damage > tile.Hardness)
						{
							SetTile(centroid.X, centroid.Y, new Air());
						}
					}
				}
			}

		}

		public override void OnCollectDeadEntity(IEntity ent)
		{
			Server.SendToAll(new DropEntityPacket(ent.EntityNetworkID));
			base.OnCollectDeadEntity(ent);
		}

		public override void Update(GameTime gt)
		{
			serverRandomTileUpdateTask.Update(gt);
			serverTileUpdateTask.Update(gt);

			

			foreach (var ent in Entities)
				ent.ServerUpdate(this, gt);

			base.Update(gt);
		}
	}
}
