using CaveGame.Core;
using CaveGame.Core.Game.Entities;
using CaveGame.Core.Generic;
using CaveGame.Core.Particles;
using CaveGame.Core.Game.Tiles;
using CaveGame.Core.Game.Walls;
using DataManagement;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CaveGame.Core.Game.Walls;

namespace CaveGame.Client
{

	public class LocalWorld : World, IClientWorld
	{

		public ParticleEmitter ParticleSystem { get; set; }


		public List<ChunkCoordinates> RequestedChunks;
		public List<ChunkCoordinates> LoadedChunks;

		public LightingEngine Lighting;

		protected DelayedTask localTileUpdateTask;
		protected DelayedTask localLightingUpdateTask;


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
		public GameClient Client { get; set; }

        public LocalWorld(GameClient client) : base()
		{
			Client = client;
			localTileUpdateTask = new DelayedTask(ApplyVisualTileUpdates, 1 / 10.0f);
			localLightingUpdateTask = new DelayedTask(GetLatestDataFromLightingThread, 1 / 20.0f);
			ParticleSystem = new ParticleEmitter(this);
			Lighting = new LightingEngine(this);
			RequestedChunks = new List<ChunkCoordinates>();
			LoadedChunks = new List<ChunkCoordinates>();
			Lighting.On();
			Tile.InitializeManager(420);
		}


		public void ClientDisconnect()
		{
			Lighting.Off();
		}

		public void UnloadChunk(ChunkCoordinates coords) {}

		Random r = new Random();

		public Light3 Ambience
		{ 
			get {
				int wrapped = ((int)Math.Floor(TimeOfDay)).Mod(24);
				return AmbientLights[wrapped];
			}
		}
		public Color SkyColor
		{
			get {
				int wrapped = ((int)Math.Floor(TimeOfDay)).Mod(24);
				int last = ((int)Math.Floor(TimeOfDay) - 1).Mod(24);
				float diff = TimeOfDay % 1;
				return Color.Lerp(SkyColors[last], SkyColors[wrapped], diff);
			}
		}

		public override void SetTileNoLight(int x, int y, Tile t)
		{
			base.SetTile(x, y, t);
		}

		public override void SetTile(int x, int y, Tile t)
		{
			Lighting.UpdateTile(x, y, t);
			base.SetTile(x, y, t);
		}

		public override void SetWall(int x, int y, Wall w)
		{
			Lighting.UpdateWall(x, y, w);
			base.SetWall(x, y, w);
		}

		public Light3 GetLight(int x, int y)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				return chunk.Lights[tileX, tileY];
			}
			return new Light3(0,0,0);
		}
		public void SetLight(int x, int y, Light3 val)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				chunk.Lights[tileX, tileY] = val;
			}
		}

		protected override void PhysicsStep()
		{
			ParticleSystem.PhysicsStep(this, PhysicsStepIncrement);
			//base.PhysicsStep();
			foreach (IEntity entity in Entities.ToArray())
				if (entity is IClientPhysicsObserver physicsObserver)
					physicsObserver.ClientPhysicsTick(this, PhysicsStepIncrement);
		}

		private void GetLatestDataFromLightingThread()
		{
			foreach (var kvp in Chunks)
				kvp.Value.Lights = Lighting.GetLightsForChunk(kvp.Key);
		}

		public override void Update(GameTime gt)
		{
			localTileUpdateTask.Update(gt);
			localLightingUpdateTask.Update(gt);
			ParticleSystem.Update(gt);
			Lighting.Update(gt);


			foreach (IEntity entity in Entities)
				entity.ClientUpdate(Client, gt);
				

			base.Update(gt);
		}

		private void LocalTileUpdates(Chunk chunk)
		{
			for (int x = 0; x < Globals.ChunkSize; x++)
			{
				for (int y = 0; y < Globals.ChunkSize; y++)
				{
					if (chunk.TileUpdate[x, y] == true)
					{
						chunk.TileUpdate[x, y] = false;


						if (chunk.GetTile(x, y) is ILocalTileUpdate tile)
						{
							int worldX = (chunk.Coordinates.X * Globals.ChunkSize) + x;
							int worldY = (chunk.Coordinates.Y * Globals.ChunkSize) + y;

							tile.LocalTileUpdate(this, worldX, worldY);

						}
					}
				}
			}
		}

		private void ApplyVisualTileUpdates()
		{
			foreach (var kvp in Chunks)
			{
				LocalTileUpdates(kvp.Value);
			}
		}
	}
}
