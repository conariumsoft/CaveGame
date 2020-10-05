using CaveGame.Core.Entities;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CaveGame.Core
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

	public class LightingEngine
	{

		#region Frontend

		public void RegisterChunk(Chunk chunk)
		{
			AddedChunkQueue.Enqueue(chunk);
		}

		public void UnregisterChunk(ChunkCoordinates coords)
		{
			RemovedChunkQueue.Enqueue(coords);
		}

		public void UpdateTile(int worldx, int worldy, Tile tile)
		{
			ChangedTiles.Enqueue(new Tuple<int, int, Tile>(worldx, worldy, tile));
		}

		public LightMatrix GetLightsForChunk(ChunkCoordinates coords)
		{
			bool success = OutputLights.TryGetValue(coords, out LightMatrix matrix);
			if (success)
			{
				//Debug.Write("GOTIT!");
				return matrix;
			}
				


			return new LightMatrix(32);
		}

		#endregion


		private LocalWorld World;
		private ThreadSafe<bool> running = new ThreadSafe<bool>(false);
		public LightingEngine(LocalWorld world)
		{
			World = world;
			AddedChunkQueue = new ConcurrentQueue<Chunk>();
			RemovedChunkQueue = new ConcurrentQueue<ChunkCoordinates>();
			ChangedTiles = new ConcurrentQueue<Tuple<int, int, Tile>>();
			OutputLights = new Dictionary<ChunkCoordinates, LightMatrix>();
			LocalChunks = new Dictionary<ChunkCoordinates, Chunk>();
			Lights = new Dictionary<ChunkCoordinates, LightMatrix>();
		}

		private ConcurrentQueue<Chunk> AddedChunkQueue;
		private ConcurrentQueue<ChunkCoordinates> RemovedChunkQueue;
		private ConcurrentQueue<Tuple<int, int, Tile>> ChangedTiles;
		private Dictionary<ChunkCoordinates, LightMatrix> OutputLights;

		private Dictionary<ChunkCoordinates, Chunk> LocalChunks;
		private Dictionary<ChunkCoordinates, LightMatrix> Lights;

		private byte Solve(byte tile, byte cur, byte inp)
		{
			byte a = Math.Max(cur, inp);
			byte b = Math.Max(a, tile);
			return b;
		}


		int runCount;

		private void RecursiveFloodFillRGB(int x, int y, Light3 input, int recurseCount)
		{
			if (recurseCount > 10)
				return;

			runCount++;

			var tile = GetTile(x, y);
			var tileLight = Light3.Dark;
			if (tile.ID == 0)
				tileLight = Light3.Ambience;
			else if (tile is ILightEmitter emitter)
				tileLight = emitter.Light;

			
			var current = GetLight(x, y);


			byte red = Solve( tileLight.Red, current.Red, input.Red);
			byte green = Solve( tileLight.Green, current.Green, input.Green);
			byte blue = Solve( tileLight.Blue, current.Blue, input.Blue);

			Light3 solved = new Light3(red, green, blue);

			if (solved.Equals(Light3.Dark))
				return;

			if (solved.Equals(current))
				return;

			if (current.Red > solved.Red && current.Green >= solved.Green && current.Blue >= solved.Blue)
				return;


			SetLight(x, y, solved);
			
			byte tileOpacity = tile.Opacity;

			red = (byte)Math.Max(0, red - tileOpacity);
			green = (byte)Math.Max(0, green - tileOpacity);
			blue = (byte)Math.Max(0, blue - tileOpacity);
			
			var newlight = new Light3(red, green, blue);
			//Debug.WriteLine(newlight.ToString());
			RecursiveFloodFillRGB(x + 1, y, newlight, recurseCount + 1);
			RecursiveFloodFillRGB(x - 1, y, newlight, recurseCount + 1);
			RecursiveFloodFillRGB(x, y + 1, newlight, recurseCount + 1);
			RecursiveFloodFillRGB(x, y - 1, newlight, recurseCount + 1);
			
		}
		
		protected int mod(int x, int m)
		{
			return (x % m + m) % m;
		}


		public Light3 GetLight(int x, int y)
		{
			int chunkX = (int)Math.Floor((float)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = mod(x, Globals.ChunkSize);
			var tileY = mod(y, Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Lights.ContainsKey(coords))
			{
				var matrix = Lights[coords];
				return matrix[tileX, tileY];
			}
			return new Light3(0, 0, 0);
		}
		public void SetLight(int x, int y, Light3 val)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = mod(x, Globals.ChunkSize);
			var tileY = mod(y, Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Lights.ContainsKey(coords))
			{
				var chunk = Lights[coords];
				chunk[tileX, tileY] = val;
			}
		}
		private void SetTile(int x, int y, Tile t)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = mod(x, Globals.ChunkSize);
			var tileY = mod(y, Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (LocalChunks.ContainsKey(coords))
			{
				var chunk = LocalChunks[coords];
				chunk.SetTile(tileX, tileY, t);
			}
			//DoUpdatePropogation(x, y);
		}
		private Tile GetTile(int x, int y)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = mod(x, Globals.ChunkSize);
			var tileY = mod(y, Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (LocalChunks.ContainsKey(coords))
			{
				var chunk = LocalChunks[coords];
				//Debug.WriteLine(String.Format("{0} {1} {2} {3} {4} {5}", x, y, chunkX, chunkY, tileX, tileY));
				return chunk.GetTile(tileX, tileY);
			}
			return new Air();
		}

		Stopwatch stopper = new Stopwatch();


		private void LightingThread()
		{
			while (running.Value)
			{
				bool hasDoneWork = false;
				

				for (int i = 0; i < RemovedChunkQueue.Count; i++)
				{
					bool have = RemovedChunkQueue.TryDequeue(out ChunkCoordinates coords);
					if (have)
					{
						hasDoneWork = true;
						LocalChunks.Remove(coords);
						Lights.Remove(coords);
					}
				}

				for (int i = 0; i < AddedChunkQueue.Count; i++)
				{
					bool have = AddedChunkQueue.TryDequeue(out Chunk chunk1);
					if (have)
					{
						if (!LocalChunks.ContainsKey(chunk1.Coordinates))
						{
							hasDoneWork = true;
							LocalChunks.Add(chunk1.Coordinates, chunk1);
						}
					}
						
				}
				
				for (int i = 0; i < ChangedTiles.Count; i++)
				{
					bool have = ChangedTiles.TryDequeue(out Tuple<int, int, Tile> datum);
					if (have)
					{
						hasDoneWork = true;
						SetTile(datum.Item1, datum.Item2, datum.Item3);
					}
						
				}

				if (hasDoneWork)
				{
					
					Lights.Clear();

					runCount = 0;
					
					foreach (var kvp in LocalChunks)
					{
						Lights.Add(kvp.Key, new LightMatrix(32));
					}
					stopper.Reset();
					stopper.Start();
					Debug.WriteLine(LocalChunks.Count + ", " + Lights.Count + ", " + OutputLights.Count);
					foreach (var kvp in LocalChunks)
					{
						var chunk = kvp.Value;
						for (int x = 0; x < Globals.ChunkSize; x ++)
							for (int y = 0; y < Globals.ChunkSize; y ++)
								RecursiveFloodFillRGB(chunk.Coordinates.X * Globals.ChunkSize + x, chunk.Coordinates.Y * Globals.ChunkSize + y, Light3.Dark, 0);
					}
					stopper.Stop();
					Debug.WriteLine("Lighting Thread cycle completed: Time: "+stopper.Elapsed.TotalSeconds+"s, "+runCount);

					//OutputLights.Clear();
					foreach (var kvp in Lights)
						OutputLights[kvp.Key] = kvp.Value;

					
				} else
				{
					Thread.Sleep(4);
				}
				
				
				//Debug.WriteLine("Finished LightPass");
				
			}
		}

		public void On() {
			running.Value = true;
			Task.Factory.StartNew(LightingThread);
		}
		public void Off()
		{
			//running.Value = false;
		}
		float timer = 0;
		public void Update(GameTime gt) {
			timer += (float)gt.ElapsedGameTime.TotalSeconds;

		//	if (timer > (5))
			//{
			//	timer = 0;
			//	LightingThread();
			//}
		}
	}


	public class LocalWorld : World
	{
		const float PhysicsStepIncrement = 1 / 100.0f;

		float physTiming;

		public List<ChunkCoordinates> RequestedChunks;
		public List<ChunkCoordinates> LoadedChunks;

		public LightingEngine Lighting;

		public LocalWorld() : base()
		{
			Lighting = new LightingEngine(this);
			RequestedChunks = new List<ChunkCoordinates>();
			LoadedChunks = new List<ChunkCoordinates>();
			Lighting.On();
		}

		public void UnloadChunk(ChunkCoordinates coords) {}

		public override void SetTile(int x, int y, Tile t)
		{
			Lighting.UpdateTile(x, y, t);
			base.SetTile(x, y, t);
		}

		public Light3 GetLight(int x, int y)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = mod(x, Globals.ChunkSize);
			var tileY = mod(y, Globals.ChunkSize);

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

			var tileX = mod(x, Globals.ChunkSize);
			var tileY = mod(y, Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				chunk.Lights[tileX, tileY] = val;
			}
		}

		private void PhysicsStep()
		{
			foreach (IEntity entity in Entities)
				if (entity is IPhysicsObject physEntity)
					physEntity.PhysicsStep(this, PhysicsStepIncrement);
		}

		float tick = 0;
		public override void Update(GameTime gt)
		{
			float delta = (float)gt.ElapsedGameTime.TotalSeconds;
			Lighting.Update(gt);

			tick += delta;


			if (tick > (1/10.0f))
			{
				tick = 0;
				foreach(var kvp in Chunks)
				{
					kvp.Value.Lights = Lighting.GetLightsForChunk(kvp.Key).Lights;
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
		}
	}

	
}
