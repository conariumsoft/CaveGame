using CaveGame.Core.Game.Entities;
using CaveGame.Core.Furniture;
using CaveGame.Core.Generic;
using CaveGame.Core.Game.Tiles;
using CaveGame.Core.Game.Walls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DataManagement;

namespace CaveGame.Core
{
	

	public interface IGameWorld
	{
		float TimeOfDay { get; set; }
		void Explosion(Vector2 pos, float strength, float radius, bool damageTiles, bool damageEntities);
		List<IEntity> Entities { get; }
		List<Furniture.FurnitureTile> Furniture { get; }
		Tile GetTile(int x, int y);
		void SetTile(int x, int y, Tile t);
		void GetTile(int x, int y, out Tile t);
		Wall GetWall(int x, int y);
		void SetWall(int x, int y, Wall w);
		void SetTileNetworkUpdated(int x, int y);
		void DoUpdatePropogation(int x, int y);
		void BreakTile(int x, int y);
		void SetTileUpdated(int x, int y);
		bool IsCellOccupied(int x, int y);
		bool IsTile<Type>(int x, int y);
		void SetTileNoLight(int x, int y, Tile t);
		void RemoveFurniture(FurnitureTile furn);
		FurnitureTile GetFurniture(int networkID);
		void Update(GameTime gt);
	}

	public interface IServerWorld : IGameWorld { }

	public interface IClientWorld: IGameWorld { }

	public abstract class World : IGameWorld
	{

		public float TimeOfDay { get; set; }
		public Light3[] AmbientLights = // depends on time of day
		{
			new Light3(0,0,0), //0 or 24
			new Light3(0,0,0), //1
			new Light3(0,0,0), //2
			new Light3(0,0,0), //3
			new Light3(0,0,0), //4
			new Light3(0,0,0), //5
			new Light3(0,0,0), //6
			new Light3(0,0,0), //7
			new Light3(0,0,0), //8
			new Light3(0,0,0), //9
			new Light3(0,0,0), //10
			new Light3(0,0,0), //11
			new Light3(0,0,0), //12
			new Light3(0,0,0), //13
			new Light3(0,0,0), //14
			new Light3(0,0,0), //15
			new Light3(0,0,0), //16
			new Light3(0,0,0), //17
			new Light3(0,0,0), //18
			new Light3(0,0,0), //19
			new Light3(0,0,0), //20
			new Light3(0,0,0), //21
			new Light3(0,0,0), //22
			new Light3(0,0,0), //23
		};

		public Color[] SkyColors =
		{
			new Color(0, 5, 12), new Color(0, 10, 64), //0 or 24
			new Color(5, 10, 12), new Color(10, 20, 92), //2
			new Color(32, 32, 32), new Color(48, 48, 130), //4
			new Color(35, 30, 51), new Color(76, 50, 150),  //6
			new Color(40, 60, 90), new Color(90, 90, 190), //8
			new Color(70, 90, 130), new Color(110, 110, 230), //10
			new Color(70, 80, 170), new Color(150, 150, 255), //12
			new Color(80, 100, 140), new Color(110, 110, 220), //14
			new Color(35, 41, 60), new Color(60, 80, 140), //14
			new Color(50, 32, 50), new Color(170, 100, 70), // 18
			new Color(25, 25, 55), new Color(92, 52, 23), //20
			new Color(5, 7, 14),  new Color(9, 23, 45), //22
		};
		#region PhysicsConstants
		public const float PhysicsStepIncrement = 1 / 100.0f;
		public const float Gravity = 6.0f;
		public const float AirResistance = 1.5f;
		public const float TerminalVelocity = 180.0f;
		

		#endregion

		public ConcurrentDictionary<ChunkCoordinates, Chunk> Chunks { get; set; }
		public List<IEntity> Entities { get; protected set; }

		public virtual List<Furniture.FurnitureTile> Furniture { get; protected set; }

		public void SetTileNetworkUpdated(int x, int y)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);
			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				chunk.NetworkUpdated[tileX, tileY] = true;
			}
		}

		public virtual void SetTileNoLight(int x, int y, Tile t)
		{
			throw new NotImplementedException();
		}


		public virtual void BreakTile(int x, int y)
        {

        }

		public void SetTile(Point p, Tile t)=>SetTile(p.X, p.Y, t);
		public Tile GetTile (Point p) => GetTile(p.X, p.Y);

		public virtual void SetTile(int x, int y, Tile t)
		{
			
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				chunk.SetTile(tileX, tileY, t);
			}
			DoUpdatePropogation(x, y);
		}
		public void SetTileUpdated(int x, int y)
		{
			int chunkX = (int)Math.Floor((double)x / Globals.ChunkSize);
			int chunkY = (int)Math.Floor((double)y / Globals.ChunkSize);

			var tileX = x.Mod(Globals.ChunkSize);
			var tileY = y.Mod(Globals.ChunkSize);

			var coords = new ChunkCoordinates(chunkX, chunkY);

			if (Chunks.ContainsKey(coords))
			{
				var chunk = Chunks[coords];
				chunk.SetTileUpdated(tileX, tileY);
			}
		}
		public void DoUpdatePropogation(int x, int y)
		{
			SetTileUpdated(x, y);
			SetTileUpdated(x, y + 1);
			SetTileUpdated(x, y - 1);
			SetTileUpdated(x + 1, y);
			SetTileUpdated(x - 1, y);
		}
		public Tile GetTile(int x, int y)
		{
			Coordinates6D coordinates = Coordinates6D.FromWorld(x, y);

			var cc = new ChunkCoordinates(coordinates.ChunkX, coordinates.ChunkY);

			if (Chunks.ContainsKey(cc))
				return Chunks[cc].GetTile(coordinates.TileX, coordinates.TileY);
			return new Game.Tiles.Void();
		}
		public void GetTile(int x, int y, out Tile t)
		{
			Coordinates6D coordinates = Coordinates6D.FromWorld(x, y);

			var cc = new ChunkCoordinates(coordinates.ChunkX, coordinates.ChunkY);
			t = new Game.Tiles.Void();
			if (Chunks.ContainsKey(cc))
				t = Chunks[cc].GetTile(coordinates.TileX, coordinates.TileY);
			
		}
		public Wall GetWall(int x, int y)
		{
			Coordinates6D coordinates = Coordinates6D.FromWorld(x, y);

			var cc = new ChunkCoordinates(coordinates.ChunkX, coordinates.ChunkY);

			if (Chunks.ContainsKey(cc))
				return Chunks[cc].GetWall(coordinates.TileX, coordinates.TileY);
			return new Game.Walls.Void();
		}
		public void GetWall(int x, int y, out Wall w)
		{
			Coordinates6D coordinates = Coordinates6D.FromWorld(x, y);

			var cc = new ChunkCoordinates(coordinates.ChunkX, coordinates.ChunkY);
			w = new Game.Walls.Void();
			if (Chunks.ContainsKey(cc))
				w = Chunks[cc].GetWall(coordinates.TileX, coordinates.TileY);
			
		}

		public virtual void SetWall(int x, int y, Wall w)
		{
			Coordinates6D coordinates = Coordinates6D.FromWorld(x, y);

			var cc = new ChunkCoordinates(coordinates.ChunkX, coordinates.ChunkY);

			if (Chunks.ContainsKey(cc))
				Chunks[cc].SetWall(coordinates.TileX, coordinates.TileY, w);
		}

		public virtual void OnCollectDeadEntity(IEntity ent)
		{
			Entities.Remove(ent);
		}

		public virtual void RemoveFurniture(FurnitureTile furn)
		{
			throw new NotImplementedException();
		}

		public IEntity FindEntityOfID(int id)
		{
			foreach (var entity in Entities)
				if (entity.EntityNetworkID == id)
					return entity;
			return null;
		}

		public bool FindEntityOfID(int id, out IEntity e)
		{
			foreach (var entity in Entities)
			{
				if (entity.EntityNetworkID == id)
				{
					e = entity;
					return true;
				}
			}
			e = null;
			return false;
		}

		public bool FindEntityOfID<T>(int id, out T e) where T : Entity, IEntity
		{
			foreach (var entity in Entities)
			{
				if (entity.EntityNetworkID == id)
				{
					e = (T)entity;
					return true;
				}
			}
			e = null;
			return false;
		}

		public virtual void Update(GameTime gt) {

			TimeOfDay += (float)gt.ElapsedGameTime.TotalSeconds/30.0f;

			foreach (var ent in Entities.ToArray())
				if (ent.Dead)
					OnCollectDeadEntity(ent);

			//Entities.RemoveAll(e => e.Dead);

			physicsTask.Update(gt);
		}

		protected virtual void PhysicsStep() { }

		public virtual void Explosion(Vector2 pos, float strength, float radius, bool damageTiles, bool damageEntities) { }

		public bool ContainsFurniture(int x, int y) {
			foreach (var furn in Furniture)
				for (int fx = 0; fx < furn.OccupationBox.X; fx++)
					for (int fy = 0; fy < furn.OccupationBox.Y; fy++)
						if (furn.TopLeft.X + fx == x && furn.TopLeft.Y+fy == y)
							return true;
			return false;
		}

		public bool IsCellOccupied(int x, int y)
		{
			if (ContainsFurniture(x, y))
				return true;
			if (IsTile<Game.Tiles.Air>(x, y))
				return false;
			if (IsTile<INonSolid>(x, y))
				return false;

			return true;
		}

		public Furniture.FurnitureTile GetFurniture(int furnitureID)
		{
			foreach (var furn in Furniture.ToArray())
				if (furn.FurnitureNetworkID == furnitureID)
					return furn;
			return null;
		}

		public Furniture.FurnitureTile GetFurnitureAt(int x, int y)
		{
			foreach (var furn in Furniture)
				for (int fx = 0; fx < furn.OccupationBox.X; fx++)
					for (int fy = 0; fy < furn.OccupationBox.Y; fy++)
						if (furn.TopLeft.X + fx == x && furn.TopLeft.Y+fy == y)
							return furn;

			return null;
		}

		private Tile _is_tile_reference;
		public bool IsTile<Type>(int x, int y)
		{
			GetTile(x, y, out _is_tile_reference);
			if (_is_tile_reference is Type)
				return true;
			return false;
		}
		private Wall _is_wall_reference;
		public bool IsWall<Type>(int x, int y)
		{
			GetWall(x, y, out _is_wall_reference);
			if (_is_wall_reference is Type)
				return true;
			return false;
		}

		protected DelayedTask physicsTask;

		public World()
		{
			physicsTask = new DelayedTask(PhysicsStep, PhysicsStepIncrement, TimeStepProcedure.SubtractIncrement);

			Entities = new List<IEntity>();
			Chunks = new ConcurrentDictionary<ChunkCoordinates, Chunk>();
			Furniture = new List<Furniture.FurnitureTile>();
		}
	}
}
