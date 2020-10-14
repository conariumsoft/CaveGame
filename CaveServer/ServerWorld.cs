using CaveGame.Core;
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CaveGame.Server
{
	public class WorldConfiguration
	{
		public int Seed { get; set; }
		public string Name { get; set; }
	}

	public class ServerWorld : Core.World
	{
		public int WorldSeed { get; }
		public string WorldName { get; }
		public GameServer Server { get; set; }

		public Generator Generator { get; set; }


		public Dictionary<ChunkCoordinates, bool> LoadedChunks;

		private ServerWorld() : base()
		{
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

			worldXml.WriteStartElement("Name");
			worldXml.WriteString(WorldName);
			worldXml.WriteEndElement();

			worldXml.WriteStartElement("Seed");
			worldXml.WriteString(WorldSeed.ToString());
			worldXml.WriteEndElement();


			//	worldXml.WriteAttributeString("Name", WorldName);
			//worldXml.WriteAttributeString("Seed", WorldSeed.ToString());

			worldXml.WriteEndDocument();
			worldXml.Close();
		}

		// Load from file 
		public ServerWorld(string worldname) : this()
		{
			// they should already exist
			//CreateDirectoryIfNull(@"Worlds");
			//CreateDirectoryIfNull(@"Worlds\" + worldname);
			//CreateDirectoryIfNull(@"Worlds\" + worldname + @"\Regions");

			if (System.IO.File.Exists(@"Worlds\" + worldname + @"WorldInfo.Xml"))
			{
				// throw error or something?
			}

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
		}

		private bool HasChunkOnFile(ChunkCoordinates coords)
		{
			if (System.IO.File.Exists(@"Worlds\" + WorldName + @"\Chunks\" + coords.GetHashCode()))
			{
				return true;
			}
			return false;
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

		float internalTimer = 0;
		float tileUpdateTimer = 0;

		private void TileUpdates(Chunk chunk)
		{
			for (int x = 0; x < Globals.ChunkSize; x++)
			{
				for (int y = 0; y < Globals.ChunkSize; y++)
				{
					if (chunk.TileUpdate[x, y] == true)
					{
						chunk.TileUpdate[x, y] = false;


						if (chunk.GetTile(x, y) is ITileUpdate tile)
						{
							int worldX = (chunk.Coordinates.X * Globals.ChunkSize) + x;
							int worldY = (chunk.Coordinates.Y * Globals.ChunkSize) + y;

							tile.TileUpdate(this, worldX, worldY);

							//var newt = chunk.GetTile(x, y);
							//Server.SendToAll(new PlaceTilePacket(newt.ID, newt.TileState, newt.Damage, worldX, worldY));// chunk.GetTile(x, y).Serialize);
						}
					}
				}
			}
		}

		private void ApplyTileUpdates(GameTime gt)
		{
			
			foreach (var chunkKeyValuePair in Chunks)
			{
				Chunk chunk = chunkKeyValuePair.Value;
				TileUpdates(chunk);
				
			}
		}

		private void RandomUpdates(Chunk chunk) {
		
		}

		private async void ApplyRandomTileTicksToLoadedChunks(GameTime gt)
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

			if (damageTiles)
			{
				for(int x = -10; x < 10; x++)
				{
					for (int y = -10; y < 10; y++)
					{
						Vector2 thisPosVec = pos + new Vector2(x * Globals.TileSize, y * Globals.TileSize);

						float dist = (thisPosVec - pos).Length()/Globals.TileSize;

						var damage = Math.Max(strength - dist, 0);

						var centroid = new Point((int)pos.X / Globals.TileSize, (int)pos.Y / Globals.TileSize)+new Point(x, y);
						var tile = GetTile(centroid.X, centroid.Y);
						tile.Damage += (byte)damage;
						
						if (tile.Damage > tile.Hardness)
						{
							SetTile(centroid.X, centroid.Y, new Air());
						}
					}
				}
			}

		}

		public override void Update(GameTime gt)
		{
			internalTimer += (float)gt.ElapsedGameTime.TotalSeconds;
			tileUpdateTimer += (float)gt.ElapsedGameTime.TotalSeconds;
			if (internalTimer > (1 / 5.0f))
			{
				internalTimer = 0;
				ApplyRandomTileTicksToLoadedChunks(gt);
			}
			if (tileUpdateTimer > (1/10.0f))
			{
				tileUpdateTimer = 0;
				ApplyTileUpdates(gt);
			}



			base.Update(gt);
		}
	}
}
