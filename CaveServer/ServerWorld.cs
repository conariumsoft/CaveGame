using CaveGame.Core;
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
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


		public Dictionary<ChunkCoordinates, bool> LoadedChunks;

		private ServerWorld() : base()
		{
		}


		public ServerWorld(WorldConfiguration config) : this()
		{
			WorldName = config.Name;
			WorldSeed = config.Seed;

			CreateDirectoryIfNull(@"Worlds");
			CreateDirectoryIfNull(@"Worlds\" + WorldName);
			CreateDirectoryIfNull(@"Worlds\" + WorldName + @"\Regions");

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



			WorldSeed = Int32.Parse(worldmeta["Metadata"]["Seed"].InnerText);
		}

		private void CreateDirectoryIfNull(string fname)
		{
			if (!System.IO.Directory.Exists(fname))
				System.IO.Directory.CreateDirectory(fname);
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

		private void ApplyRandomTileTicksToLoadedChunks(GameTime gt)
		{
			Random rng = new Random();
			int UpdatesPerChunk = 10;
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

		public override void Update(GameTime gt)
		{
			internalTimer += (float)gt.ElapsedGameTime.TotalSeconds;
			tileUpdateTimer += (float)gt.ElapsedGameTime.TotalSeconds;
			if (internalTimer > (1 / 1.0f))
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
