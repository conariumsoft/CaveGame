using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using CaveGame.Common;
using CaveGame.Common.Extensions;
using CaveGame.Common.Game.Tiles;
using CaveGame.Common.Game.Walls;
using CaveGame.Common.Generic;

// TODO: Refactor and clean 
namespace CaveGame.Client
{
	[StructLayout(LayoutKind.Explicit)]
	public struct Cell: IDisposable
	{
		[FieldOffset(0)] public int X;
		[FieldOffset(4)] public int Y;
		[FieldOffset(8)] public Light3 Light;

		public Cell(int x, int y, Light3 l)
		{
			X = x;
			Y = y;
			Light = l;
		}

        public Cell(Point coords, Light3 l)
        {
            X = coords.X;
            Y = coords.Y;
            Light = l;
        }

		public void Dispose()
		{
			System.GC.SuppressFinalize(this);
		}
	}

	public struct TileChangedCell : IDisposable
	{
		public int X;
		public int Y;
		public Tile Tile;
		public TileChangedCell(int x, int y, Tile t)
		{
			X = x;
			Y = y;
			Tile = t;
		}

		public void Dispose()
		{
			System.GC.SuppressFinalize(this);
		}
	}

	public struct WallChangedCell : IDisposable
	{
		public int X;
		public int Y;
		public Wall Wall;
		public WallChangedCell(int x, int y, Wall w)
		{
			X = x;
			Y = y;
			Wall = w;
		}

		public void Dispose()
		{
			System.GC.SuppressFinalize(this);
		}
	}


    internal struct InvokedLightCell
    {
        public Point Coords { get; set; }
        public Light3 Value { get; set; }
    }


    public class LightingEngine : ILightingEngine
    {
        public Thread LightThread;

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
            ChangedTiles.Enqueue(new TileChangedCell(worldx, worldy, tile));
        }

        public void UpdateWall(int worldx, int worldy, Wall wall)
        {
            ChangedWalls.Enqueue(new WallChangedCell(worldx, worldy, wall));
        }

        public Light3[,] GetLightsForChunk(ChunkCoordinates coords)
        {
            lock (OutputLights)
            {
                bool success = OutputLights.TryGetValue(coords, out Light3[,] matrix);
                if (success)
                {
                    return matrix;
                }
            }
            return new Light3[32, 32];
        }

        #endregion


        private LocalWorld World;
        private ThreadSafeValue<bool> running = new ThreadSafeValue<bool>(false);
        public LightingEngine(LocalWorld world)
        {
            World = world;

            AddedChunkQueue = new ConcurrentQueue<Chunk>();
            RemovedChunkQueue = new ConcurrentQueue<ChunkCoordinates>();
            ChangedTiles = new ConcurrentQueue<TileChangedCell>();
            ChangedWalls = new ConcurrentQueue<WallChangedCell>();
            OutputLights = new Dictionary<ChunkCoordinates, Light3[,]>();
            LocalChunks = new Dictionary<ChunkCoordinates, Chunk>();
            LightCells = new Dictionary<ChunkCoordinates, Light3[,]>();
            InvokedCells = new ConcurrentQueue<InvokedLightCell>();
            UpdatedCells = new Queue<Cell>();
        }

        private ConcurrentQueue<Chunk> AddedChunkQueue;
        private ConcurrentQueue<ChunkCoordinates> RemovedChunkQueue;
        private ConcurrentQueue<TileChangedCell> ChangedTiles;
        private ConcurrentQueue<WallChangedCell> ChangedWalls;
        private ConcurrentQueue<InvokedLightCell> InvokedCells;
        private Dictionary<ChunkCoordinates, Light3[,]> OutputLights;

        private Dictionary<ChunkCoordinates, Chunk> LocalChunks;
        private Dictionary<ChunkCoordinates, Light3[,]> LightCells;

        private Queue<Cell> UpdatedCells;

#if CLIENT
        private byte Solve(byte tile, byte cur, byte inp)
        {
            byte a = Math.Max(cur, inp);
            byte b = Math.Max(a, tile);
            return b;
        }


        public Light3 GetLight(int x, int y)
        {
            int chunkX = (int)Math.Floor((float)x / Globals.ChunkSize);
            int chunkY = (int)Math.Floor((float)y / Globals.ChunkSize);

            var tileX = x.Mod(Globals.ChunkSize);
            var tileY = y.Mod(Globals.ChunkSize);

            var coords = new ChunkCoordinates(chunkX, chunkY);

            if (LightCells.ContainsKey(coords))
            {
                var matrix = LightCells[coords];
                return matrix[tileX, tileY];
            }
            return new Light3(0, 0, 0);
        }

        public Light3 GetLight(Point coords) => GetLight(coords.X, coords.Y);
        private void SetLight(int x, int y, Light3 val)
        {
            int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
            int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

            var tileX = x.Mod(Globals.ChunkSize);
            var tileY = y.Mod(Globals.ChunkSize);

            var coords = new ChunkCoordinates(chunkX, chunkY);

            if (LightCells.ContainsKey(coords))
            {
                var chunk = LightCells[coords];
                chunk[tileX, tileY] = val;
            }
        }

        public void InvokeLight(Point coords, Light3 value)
        {
            UpdatedCells.Enqueue(new Cell(coords, value));
            SetLight(coords, value);
        }

        private void SetLight(Point coords, Light3 val) => SetLight(coords.X, coords.Y, val);
        private void SetTile(int x, int y, Tile t)
        {
            int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
            int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

            var tileX = x.Mod(Globals.ChunkSize);
            var tileY = y.Mod(Globals.ChunkSize);

            var coords = new ChunkCoordinates(chunkX, chunkY);

            if (LocalChunks.ContainsKey(coords))
            {
                var chunk = LocalChunks[coords];
                chunk.SetTile(tileX, tileY, t);
            }
        }
        private Tile GetTile(int x, int y)
        {
            int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
            int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

            var tileX = x.Mod(Globals.ChunkSize);
            var tileY = y.Mod(Globals.ChunkSize);

            var coords = new ChunkCoordinates(chunkX, chunkY);

            if (LocalChunks.ContainsKey(coords))
            {
                var chunk = LocalChunks[coords];
                return chunk.GetTile(tileX, tileY);
            }
            return null;
        }
        private Wall GetWall(int x, int y)
        {
            int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
            int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

            var tileX = x.Mod(Globals.ChunkSize);
            var tileY = y.Mod(Globals.ChunkSize);

            var coords = new ChunkCoordinates(chunkX, chunkY);

            if (LocalChunks.ContainsKey(coords))
            {
                var chunk = LocalChunks[coords];
                return chunk.GetWall(tileX, tileY);
            }
            return null;
        }
        private void SetWall(int x, int y, Wall w)
        {
            int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
            int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

            var tileX = x.Mod(Globals.ChunkSize);
            var tileY = y.Mod(Globals.ChunkSize);

            var coords = new ChunkCoordinates(chunkX, chunkY);

            if (LocalChunks.ContainsKey(coords))
            {
                var chunk = LocalChunks[coords];
                chunk.SetWall(tileX, tileY, w);
            }
        }

        Stopwatch stopwatch = new Stopwatch();
        int iterationCount;

        private void Iterate()
        {
            Cell data;
            Light3 input;
            Tile tile;
            Wall wall;
            int count = Math.Min(UpdatedCells.Count, 10000);
            for (int i = 0; i < count; i++)
            {
                iterationCount++;
                bool zuccess = UpdatedCells.TryDequeue(out data);

                if (!zuccess)
                    continue;
                input = data.Light;



                tile = GetTile(data.X, data.Y);
                wall = GetWall(data.X, data.Y);

                if (tile == null || wall == null)
                    continue;
                byte opacity = Math.Max(tile.Opacity, wall.Opacity);
                var tileLight = Light3.Dark;
                if (tile.ID == 0 && wall.ID == 0)
                    tileLight = World.Ambience;
                else if (tile is ILightEmitter emitter)
                    tileLight = emitter.Light;

                var current = GetLight(data.X, data.Y);

                byte red = Solve(tileLight.Red, current.Red, input.Red);
                byte green = Solve(tileLight.Green, current.Green, input.Green);
                byte blue = Solve(tileLight.Blue, current.Blue, input.Blue);

                Light3 solved = new Light3(red, green, blue);
                SetLight(data.X, data.Y, solved);

                if (solved.Equals(Light3.Dark))
                    continue;

                if (solved.Equals(current))
                    continue;

                if (current.Red >= solved.Red && current.Green >= solved.Green && current.Blue >= solved.Blue)
                    continue;


                var newlight = solved.Absorb(opacity);

                UpdatedCells.Enqueue(new Cell(data.X + 1, data.Y, newlight));
                UpdatedCells.Enqueue(new Cell(data.X, data.Y + 1, newlight));
                UpdatedCells.Enqueue(new Cell(data.X - 1, data.Y, newlight));
                UpdatedCells.Enqueue(new Cell(data.X, data.Y - 1, newlight));
            }
        }

        private void LightingThread()
        {
            while (running.Value)
            {
                for (int i = 0; i < RemovedChunkQueue.Count; i++)
                {
                    bool have = RemovedChunkQueue.TryDequeue(out ChunkCoordinates coords);
                    if (have)
                    {
                        LocalChunks.Remove(coords);
                        LightCells.Remove(coords);
                    }
                }

                for (int i = 0; i < AddedChunkQueue.Count; i++)
                {
                    bool have = AddedChunkQueue.TryDequeue(out Chunk chunk1);
                    if (have)
                    {
                        if (!LocalChunks.ContainsKey(chunk1.Coordinates))
                        {
                            LocalChunks.Add(chunk1.Coordinates, chunk1);
                            LightCells.Add(chunk1.Coordinates, new Light3[32, 32]);

                            for (int x = 0; x < Globals.ChunkSize; x++)
                            {
                                for (int y = 0; y < Globals.ChunkSize; y++)
                                {
                                    UpdatedCells.Enqueue(new Cell(
                                        (chunk1.Coordinates.X * Globals.ChunkSize) + x,
                                        (chunk1.Coordinates.Y * Globals.ChunkSize) + y
                                , Light3.Dark));
                                }
                            }
                        }
                    }
                }

                TileChangedCell recvdata;
                for (int i = 0; i < ChangedTiles.Count; i++)
                {
                    bool have = ChangedTiles.TryDequeue(out recvdata);
                    if (have)
                    {
                        //var prevTile = GetTile(recvdata.Item1, recvdata.Item2);
                        SetTile(recvdata.X, recvdata.Y, recvdata.Tile);
                        //var prevLight = GetLight(recvdata.Item1, recvdata.Item2);
                        //var postOpacity = recvdata.Item3.Opacity;
                        int resetRadius = 7;
                        int recalcRadius = 9;

                        //if (prevTile is ILightEmitter)
                        {
                            //		resetRadius = 10;
                            //		recalcRadius = 12;
                        }

                        //var newL = new Light3(r, g, b);
                        for (int x = -resetRadius; x < resetRadius; x++)
                        {
                            for (int y = -resetRadius; y < resetRadius; y++)
                            {
                                SetLight(recvdata.X + x, recvdata.Y + y, Light3.Dark);
                                //UpdatedCells.Enqueue(new Cell(recvdata.Item1+x, recvdata.Item2+y, Light3.Dark));
                            }
                        }

                        for (int x = -recalcRadius; x < recalcRadius; x++)
                        {
                            for (int y = -recalcRadius; y < recalcRadius; y++)
                            {
                                int mx = recvdata.X + x;
                                int my = recvdata.Y + y;

                                byte absorb = 0;

                                var t = GetTile(mx, my);
                                if (t != null)
                                    absorb = t.Opacity;

                                UpdatedCells.Enqueue(new Cell(mx, my, GetLight(mx + 1, my).Absorb(absorb)));
                                UpdatedCells.Enqueue(new Cell(mx, my, GetLight(mx - 1, my).Absorb(absorb)));
                                UpdatedCells.Enqueue(new Cell(mx, my, GetLight(mx, my + 1).Absorb(absorb)));
                                UpdatedCells.Enqueue(new Cell(mx, my, GetLight(mx, my - 1).Absorb(absorb)));
                            }
                        }

                        //UpdatedCells.Enqueue(new Cell(recvdata.Item1, recvdata.Item2, Light3.Dark));

                    }
                }

                WallChangedCell recv2;
                for (int i = 0; i < ChangedWalls.Count; i++)
                {
                    bool have = ChangedWalls.TryDequeue(out recv2);
                    if (have)
                    {

                        SetWall(recv2.X, recv2.Y, recv2.Wall);
                        //var prevLight = GetLight(recvdata.Item1, recvdata.Item2);
                        //var postOpacity = recvdata.Item3.Opacity;


                        //byte r = (byte)(prevLight.Red - postOpacity);
                        //byte g = (byte)(prevLight.Green - postOpacity);
                        //byte b = (byte)(prevLight.Blue - postOpacity);

                        //var newL = new Light3(r, g, b);
                        for (int x = -4; x < 4; x++)
                        {
                            for (int y = -4; y < 4; y++)
                            {
                                SetLight(recv2.X + x, recv2.Y + y, Light3.Dark);
                                //UpdatedCells.Enqueue(new Cell(recvdata.Item1+x, recvdata.Item2+y, Light3.Dark));
                            }
                        }

                        for (int x = -3; x < 3; x++)
                        {
                            for (int y = -3; y < 3; y++)
                            {
                                int mx = recv2.X + x;
                                int my = recv2.Y + y;

                                byte absorb = 0;

                                var w = GetWall(mx, my);
                                if (w != null)
                                    absorb = w.Opacity;

                                UpdatedCells.Enqueue(new Cell(mx, my, GetLight(mx + 1, my).Absorb(absorb)));
                                UpdatedCells.Enqueue(new Cell(mx, my, GetLight(mx - 1, my).Absorb(absorb)));
                                UpdatedCells.Enqueue(new Cell(mx, my, GetLight(mx, my + 1).Absorb(absorb)));
                                UpdatedCells.Enqueue(new Cell(mx, my, GetLight(mx, my - 1).Absorb(absorb)));
                            }
                        }

                        //UpdatedCells.Enqueue(new Cell(recvdata.Item1, recvdata.Item2, Light3.Dark));

                    }
                }

                stopwatch.Reset();
                stopwatch.Start();
                Iterate();
                stopwatch.Stop();

                if (iterationCount > 100)
                {
                    //Debug.WriteLine("LTTstore.com " + stopwatch.Elapsed.TotalSeconds + "s, " + UpdatedCells.Count);
                    iterationCount = 0;
                    //OutputLights.Clear();
                    foreach (var kvp in LightCells)
                        OutputLights[kvp.Key] = kvp.Value;
                    Thread.Sleep(1);
                    //UpdatedCells.TrimExcess();
                    //GC.Collect();

                }
            }
        }
#endif
        public void On()
        {
#if CLIENT
            running.Value = true;
            //Task.Factory.StartNew(LightingThread);
            LightThread = new Thread(LightingThread);
            LightThread.Priority = ThreadPriority.AboveNormal;
            LightThread.Start();
#endif
        }
        public void Off()
        {
            running.Value = false;
            //LightThread.();
        }
        float timer = 0;
        public void Update(GameTime gt)
        {
            timer += (float)gt.ElapsedGameTime.TotalSeconds;
        }
    }
}
