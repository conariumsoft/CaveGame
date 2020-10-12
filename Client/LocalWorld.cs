﻿using CaveGame.Core;
using CaveGame.Core.Entities;
using CaveGame.Core.Particles;
using CaveGame.Core.Tiles;
using CaveGame.Core.Walls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CaveGame.Client
{
	
	public struct LightMatrix
	{
		public Light3[,] Lights;


		public LightMatrix(int size)
		{
			Lights = new Light3[size, size];

		}

		public Light3 this[int x, int y]
		{
			get { return Lights[x, y]; }
			set { Lights[x, y] = value; }
		}

	}

	


	public class LocalWorld : World
	{

		public ParticleEmitter ParticleSystem;

		float physTiming;

		public List<ChunkCoordinates> RequestedChunks;
		public List<ChunkCoordinates> LoadedChunks;

		public LightingEngine Lighting;

		public LocalWorld() : base()
		{
			ParticleSystem = new ParticleEmitter(this);
			Lighting = new LightingEngine(this);
			RequestedChunks = new List<ChunkCoordinates>();
			LoadedChunks = new List<ChunkCoordinates>();
			Lighting.On();
		}

		public void UnloadChunk(ChunkCoordinates coords) {}

		Random r = new Random();

		private static void Repeat(Action a, int times)
		{
			for (int i = 0; i<times; i++)
			{
				a.Invoke();
			}
		}


		public override void SetTile(int x, int y, Tile t)
		{
			Repeat(() =>
			ParticleSystem.Add(new SmokeParticle(
				new Vector2(x, y) * 8, Color.White, Rotation.FromDeg(r.Next(0, 360)), (float)(r.NextDouble()+0.5f), new Vector2(
					(float)((r.NextDouble() - 0.5f) * 2),
					-2
				)
			)), 4);
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

		private void PhysicsStep()
		{
			ParticleSystem.PhysicsStep(this, PhysicsStepIncrement);

			foreach (IEntity entity in Entities)
				if (entity is IPhysicsObject physEntity)
					physEntity.PhysicsStep(this, PhysicsStepIncrement);
		}

		float tick = 0;
		public override void Update(GameTime gt)
		{
			float delta = (float)gt.ElapsedGameTime.TotalSeconds;
			ParticleSystem.Update(gt);
			Lighting.Update(gt);

			tick += delta;

			if (tick > (1/20.0f))
			{
				tick = 0;
				foreach(var kvp in Chunks)
				{
					kvp.Value.Lights = Lighting.GetLightsForChunk(kvp.Key);
				}
			}

			physTiming += delta;

			while (physTiming >= PhysicsStepIncrement)
			{
				physTiming -= PhysicsStepIncrement;
				PhysicsStep();
			}

			foreach (IEntity entity in Entities)
				entity.Update(this, gt);


			tileUpdateTimer += (float)gt.ElapsedGameTime.TotalSeconds;
			if (tileUpdateTimer > (1 / 10.0f))
			{
				tileUpdateTimer = 0;
				ApplyTileUpdates(gt);
			}
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

		private void ApplyTileUpdates(GameTime gt)
		{
			Chunk chunk;
			foreach (var chunkKeyValuePair in Chunks)
			{
				chunk = chunkKeyValuePair.Value;
				LocalTileUpdates(chunk);

			}
		}
		float tileUpdateTimer = 0;


	}

}
