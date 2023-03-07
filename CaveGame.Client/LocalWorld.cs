using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using CaveGame.Client.DebugTools;
using CaveGame.Common;
using CaveGame.Common.Extensions;
using CaveGame.Common.Game.Entities;
using CaveGame.Common.Game.Tiles;
using CaveGame.Common.Game.Walls;
using CaveGame.Common.Generic;

namespace CaveGame.Client
{

	public class LocalWorld : GameWorld, IClientWorld
	{

		public ParticleEmitter ParticleSystem { get; set; }


		public List<ChunkCoordinates> RequestedChunks;
		public List<ChunkCoordinates> LoadedChunks;

		public LightingEngine Lighting { get; set; }
		ILightingEngine IClientWorld.Lighting => Lighting;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
		public GameClient Client { get; set; }
		public Sky Sky { get; private set; }

        public LocalWorld(GameClient client) : base()
		{
			Context = NetworkContext.Client;

			Client = client;

			Sky = new Sky(this);
			ParticleSystem = new ParticleEmitter(this);
			Lighting = new LightingEngine(this);
			Lighting.On();

			WorldTimedTasks.Add(new RepeatingIntervalTask(ApplyVisualTileUpdates, 1 / 10.0f));
			WorldTimedTasks.Add(new RepeatingIntervalTask(GetLatestDataFromLightingThread, 1 / 20.0f));
			
			RequestedChunks = new List<ChunkCoordinates>();
			LoadedChunks = new List<ChunkCoordinates>();
			
			Tile.InitializeManager(420);
		}


		public void ClientDisconnect()
		{
			Lighting.Off();
		}

		public void Dispose()
        {

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

		public Color SkyColor => Sky.SkyColor;

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
			//Profiler.Start("WorldTasks");
			//base.Update(gt);
			//localTileUpdateTask.Update(gt);
			//localLightingUpdateTask.Update(gt);
			//Profiler.End();

			Profiler.Start("Particles");
			ParticleSystem.Update(gt);
			Profiler.End();

			Profiler.Start("Lighting");
			Lighting.Update(gt);
			Profiler.End();

			Profiler.Start("EntityUpdate");
			foreach (IEntity entity in Entities)
				entity.ClientUpdate(Client, gt);

			Profiler.End();

			TimeOfDay += (float)gt.ElapsedGameTime.TotalSeconds / 30.0f;


			//Entities.RemoveAll(e => e.Dead);
			//Profiler.Start("PhysicsTask");
			//physicsTask.Update(gt);
			//Profiler.End();
			base.Update(gt);

		}


		private void SpawnExplosionParticles(Explosion blast)
        {
			for (int i = 0; i < 360; i += 20)
			{
				float randy = r.Next(0, 10) - 5;
				Rotation rotation = Rotation.FromDeg(i + randy);
				Vector2 direction = new Vector2((float)Math.Sin(rotation.Radians), (float)Math.Cos(rotation.Radians));
				float size = ((float)r.NextDouble() * 0.2f) + (blast.BlastPressure * 0.1f);
				ParticleSystem.EmitSmokeParticle(blast.Position, Color.Gray, Rotation.FromDeg(0), new Vector2(size, size), direction * (blast.BlastRadius * ((float)r.NextDouble())));
			}
			ParticleSystem.EmitExplosionParticle(blast.Position);
		}

		public override void Explosion(Explosion Blast, bool DamageTiles, bool DamageEntities)
		{
			Random rand = new Random();
			ExplodeTiles(Blast, false);
			SpawnExplosionParticles(Blast);

			foreach (var entity in Entities)
            {
				if (entity is IPhysicsEntity physicsEntity  && entity is ICanBleed && physicsEntity.Position.Distance(Blast.Position) < 60)
                {
					for (int i = -45; i < 45; i += 2)
					{
						var blast_direction = Blast.Position.LookAt(entity.Position);
						var result = TileRaycast(physicsEntity.Position, Rotation.FromDeg(Rotation.FromUnitVector(blast_direction).Degrees + i), rand.Next(10, 60));
						if (result.Hit)
						{
							Vector2 rounded = result.Intersection.ToPoint().ToVector2();

							ParticleSystem.Add(new TileBloodSplatterParticle { AttachedTo = result.TileCoordinates, Position = rounded, Face = result.Face });
						}
					}
				}
            }
		}

        private void ApplyVisualTileUpdates()
		{
			
			int count = TileUpdateQueue.Count;
			for (int i = 0; i < count; i++)
			{
				Point coords = TileUpdateQueue.Dequeue();
				if (GetTile(coords) is ILocalTileUpdate tile)
					tile.LocalTileUpdate(this, coords.X, coords.Y);
			}
		}
	}
}

