using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CaveGame.Common.Extensions;
using CaveGame.Common.Game.Entities;
using CaveGame.Common.Game.Furniture;
using CaveGame.Common.Game.Tiles;
using CaveGame.Common.Game.Walls;
using CaveGame.Common.Generic;

namespace CaveGame.Common
{
	public struct Explosion : IDamageSource
    {
		public Vector2 Position { get; set; }
		public float BlastRadius { get; set; }
		public float BlastPressure { get; set; }
		public bool Thermal { get; set; }

		public IEntity Source { get; set; }


    }

	public enum NetworkContext
    {
		Server,
		Client
    }

	public interface IGameWorld
	{
		bool IsServer();
		bool IsClient();

		NetworkContext Context { get; }
		float TimeOfDay { get; set; }
		void Explosion(Explosion Blast, bool DamageTiles, bool DamageEntities);
		List<IEntity> Entities { get; }
		List<FurnitureTile> Furniture { get; }
		Tile GetTile(int x, int y);
		void SetTile(int x, int y, Tile t);
		void GetTile(int x, int y, out Tile t);
		Wall GetWall(int x, int y);
		void SetWall(int x, int y, Wall w);
		
		void DoUpdatePropogation(int x, int y);
		void BreakTile(int x, int y);
		void SetTileUpdated(int x, int y);
		bool IsCellOccupied(int x, int y);
		bool IsTile<Type>(int x, int y);
		void SetTileNoLight(int x, int y, Tile t);
		void RemoveFurniture(FurnitureTile furn);
		FurnitureTile GetFurniture(int networkID);
		void Update(GameTime gt);
        TileRaycastResult   TileRaycast(Vector2 origin, Rotation direction, float maxDistance = 100f, bool detectLiquids = false, bool detectNonSolids = false);
		EntityRaycastResult EntityRaycast(Vector2 origin, Rotation direction, float maxDistance = 100f);
	}

	public interface IServerWorld : IGameWorld 
	{
		//void SetTileNetworkUpdated(int x, int y);
		void RequestTileNetworkUpdate(Point p);
		void RequestWallNetworkUpdate(Point p);
	}

	public interface IClientWorld: IGameWorld 
	{
		ParticleEmitter ParticleSystem { get; }
		ILightingEngine Lighting { get; }
	}

	public enum GameSessionType
    {
        Singleplayer,
		Multiplayer,
    }
	public abstract class GameWorld : IGameWorld
	{
		public bool IsServer() => (Context == NetworkContext.Server);
		public bool IsClient() => (Context == NetworkContext.Client);
		
		public float TimeOfDay { get; set; }
		public Light3[] AmbientLights = // depends on time of day
		{
			Light3.Moonlight, //0 or 24
			Light3.Moonlight, //1
			Light3.Moonlight, //2
			Light3.Moonlight, //3
			new Light3(60, 40, 40), //4
			new Light3(70, 70, 40), //5
			new Light3(90, 90, 60), //6
			new Light3(128, 128, 90), //7
			Light3.Daylight, //8
			Light3.Daylight, //9
			Light3.Daylight, //10
			Light3.Daylight, //11
			Light3.Daylight, //12
			Light3.Daylight, //13
			Light3.Daylight, //14
			Light3.Daylight, //15
			Light3.Daylight, //16
			Light3.Daylight, //17
			Light3.Moonlight, //18
			Light3.Moonlight, //19
			Light3.Moonlight, //20
			Light3.Moonlight, //21
			Light3.Moonlight, //22
			Light3.Moonlight, //23
		};

		
		#region PhysicsConstants
		public const float PhysicsStepIncrement = 1 / 100.0f;
		public const float Gravity = 6.0f;
		public const float AirResistance = 1.5f;
		public const float TerminalVelocity = 180.0f;


		#endregion

		protected List<RepeatingIntervalTask> WorldTimedTasks { get; set; }

		public GameWorld()
		{
			TileUpdateQueue = new UniqueQueue<Point>();
			WallUpdateQueue = new UniqueQueue<Point>();
			Entities = new List<IEntity>();
			Chunks = new ConcurrentDictionary<ChunkCoordinates, Chunk>();
			Furniture = new List<FurnitureTile>();

			WorldTimedTasks = new List<RepeatingIntervalTask>();
			WorldTimedTasks.Add(new RepeatingIntervalTask(PhysicsStep, PhysicsStepIncrement, TimeStepProcedure.SubtractIncrement));
		}


		protected UniqueQueue<Point> TileUpdateQueue { get; set; }
		protected UniqueQueue<Point> WallUpdateQueue { get; set; }

		public void RequestTileUpdate(Point position)=>TileUpdateQueue.Enqueue(position);
		public void RequestWallUpdate(Point position) => WallUpdateQueue.Enqueue(position);

		public ConcurrentDictionary<ChunkCoordinates, Chunk> Chunks { get; set; }
		public List<IEntity> Entities { get; protected set; }

		public virtual List<FurnitureTile> Furniture { get; protected set; }

		public GameSessionType SessionType { get; protected set; }
        public NetworkContext Context { get; protected set; }



		public virtual void SetTileNoLight(int x, int y, Tile t)
		{
			throw new NotImplementedException();
		}
		public virtual void BreakTile(int x, int y)
        {

        }


		


		
		public Tile GetTile(int x, int y)
		{
			Coordinates6D coordinates = Coordinates6D.FromWorld(x, y);

			var cc = new ChunkCoordinates(coordinates.ChunkX, coordinates.ChunkY);

			if (Chunks.ContainsKey(cc))
				return Chunks[cc].GetTile(coordinates.TileX, coordinates.TileY);
			return new Game.Tiles.Void();
		}
		public Tile GetTile(Point coords) => GetTile(coords.X, coords.Y);
		public void GetTile(int x, int y, out Tile t)
		{
			Coordinates6D coordinates = Coordinates6D.FromWorld(x, y);

			var cc = new ChunkCoordinates(coordinates.ChunkX, coordinates.ChunkY);
			t = new Game.Tiles.Void();
			if (Chunks.ContainsKey(cc))
				t = Chunks[cc].GetTile(coordinates.TileX, coordinates.TileY);

		}
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
		public void SetTile(Point p, Tile t) => SetTile(p.X, p.Y, t);

		public Wall GetWall(int x, int y)
		{
			Coordinates6D coordinates = Coordinates6D.FromWorld(x, y);

			var cc = new ChunkCoordinates(coordinates.ChunkX, coordinates.ChunkY);

			if (Chunks.ContainsKey(cc))
				return Chunks[cc].GetWall(coordinates.TileX, coordinates.TileY);
			return new Game.Walls.Void();
		}
		public Wall GetWall(Point coords) => GetWall(coords.X, coords.Y);
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
		public void SetWall(Point p, Wall w) => SetWall(p.X, p.Y, w);

		
		
		

		

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
				//chunk.SetTileUpdated(tileX, tileY);
			}
		}

		public void DoUpdatePropogation(int x, int y)
		{
			RequestTileUpdate(new Point(x, y));
			RequestTileUpdate(new Point(x, y + 1));
			RequestTileUpdate(new Point(x, y - 1));
			RequestTileUpdate(new Point(x + 1, y));
			RequestTileUpdate(new Point(x - 1, y));
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

			//Profiler.Start("EntityClear");

			foreach (var ent in Entities.ToArray())
				if (ent.Dead)
					OnCollectDeadEntity(ent);

			foreach (var task in WorldTimedTasks)
				task.Update(gt);
		}

		protected virtual void PhysicsStep() { }

		protected const float MAX_BLAST_DISTANCE = 80;
		protected const float MAX_DAMAGE = 70;




		protected virtual void InflictExplosionDamageOnEntity(Explosion Blast, IEntity Entity, int Damage, Vector2 Direction, float KickBack)
        {
	        if (Entity is IExplosionDamagable explosionDamagableEntity)
		        explosionDamagableEntity.Damage(
			        type: DamageType.Explosion,
			        source: Blast,
			        amount: Damage,
			        direction: Direction
		        );
	        
			if (Entity is IPhysicsEntity MovingEntity)
            {
				
				Logger.LogCurrentContext($"{Direction*KickBack*10000}");
				MovingEntity.Velocity += Direction * KickBack*10000;
			}
			
		}

		protected virtual void ExplodeAffectEntities(Explosion Blast, bool DamageEntities = true)
        {
			foreach (var ent in Entities)
			{
				if (ent == Blast.Source)
					continue; // Skip ourselves
				
				
				if (ent is IPhysicsEntity physicsEntity)
				{
					var dist = physicsEntity.Position.Distance(Blast.Position);
					if (dist < MAX_BLAST_DISTANCE)
                    {
						var power = Math.Min((1 / dist) * Blast.BlastPressure * 7f, 3550);
						var unitVec = (physicsEntity.Position - Blast.Position);
						unitVec.Normalize();
						int damage = (int)Math.Max(MAX_DAMAGE - dist, 1);

						InflictExplosionDamageOnEntity(Blast, ent, damage, unitVec, power);
						
						// TODO: Implement Explosion Force impact on entity velocity
						
					}	
				}
			}
		}

		protected virtual void ExplodeTiles(Explosion Blast, bool DropTiles = true, int MaxSquareRadius = 12)
        {
			for (int x = -MaxSquareRadius; x < MaxSquareRadius; x++)
			{
				for (int y = -MaxSquareRadius; y < MaxSquareRadius; y++)
				{
					Vector2 ThisTilePos = Blast.Position + new Vector2(x * Globals.TileSize, y * Globals.TileSize);

					float dist = (ThisTilePos - Blast.Position).Length() / Globals.TileSize;

					var damage = Math.Max((Blast.BlastPressure * 3) - (dist * 3), 0);

					var centroid = new Point((int)Blast.Position.X / Globals.TileSize, (int)Blast.Position.Y / Globals.TileSize) + new Point(x, y);
					var tile = GetTile(centroid.X, centroid.Y);

					if (tile is ILiquid)
						continue;


					tile.Damage += (byte)Math.Ceiling(damage);

					if (tile.Damage > tile.Hardness)
                    {
						if (DropTiles)
							BreakTile(centroid.X, centroid.Y);
						else
							SetTile(centroid.X, centroid.Y, new Game.Tiles.Air());
					}
				}
			}
		}

		public virtual void Explosion(Explosion Blast, bool DamageTiles, bool DamageEntities) {
			if (DamageTiles)
				ExplodeTiles(Blast);

			ExplodeAffectEntities(Blast, DamageEntities);
		}

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

		public FurnitureTile GetFurniture(int furnitureID)
		{
			foreach (var furn in Furniture.ToArray())
				if (furn.FurnitureNetworkID == furnitureID)
					return furn;
			return null;
		}

		public FurnitureTile GetFurnitureAt(int x, int y)
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


		const float ray_accuracy = 0.15f;


		public TileRaycastResult TileRaycast(Vector2 origin, Rotation direction, float maxDistance = 120, bool detectLiquids = false, bool detectNonSolids = false)
		{
			for (float i = 0; i < maxDistance; i += ray_accuracy)
			{
				Vector2 current_pt = origin + (direction.ToUnitVector() * i);

				Point tile_coords = new Point(
				(int)Math.Floor(current_pt.X / 8),
				(int)Math.Floor(current_pt.Y / 8)
				);

				Tile tileAt = GetTile(tile_coords.X, tile_coords.Y);

				if (tileAt.ID == 0 || (tileAt is INonSolid))
					continue;


                Vector2 tile_corner = (tile_coords.ToVector2() * 8);
                Vector2 tile_size = new Vector2(8, 8);
				LineSegment ray_travel_segment = new LineSegment(origin, current_pt);
				Rectangle tile_rect = new Rectangle(tile_corner.ToPoint(), tile_size.ToPoint());

				if (!CollisionSolver.Intersects(ray_travel_segment, tile_rect, out Vector2 intersection, out Face face))
					continue;


				Vector2 normal = face.ToSurfaceNormal();

				return new TileRaycastResult { Hit = true, Intersection = intersection, SurfaceNormal = normal, TileCoordinates = tile_coords, Face = face, Target = tileAt };
		
			}
			return new TileRaycastResult { Hit = false };
        }

        public EntityRaycastResult EntityRaycast(Vector2 origin, Rotation direction, float maxDistance = 100)
        {
			for (float i = 0; i < maxDistance; i += ray_accuracy)
			{
				Vector2 intersection;
				Face side;
				foreach (IEntity candidate in Entities)
				{
					Vector2 current_pt = origin + (direction.ToUnitVector() * i);
					if (!CollisionSolver.Intersects(new LineSegment(origin, current_pt), candidate.GetCollisionRect(), out intersection, out side))
						continue;
					return new EntityRaycastResult{Hit = true,Face = side, Intersection = intersection,Target = candidate,SurfaceNormal = side.ToSurfaceNormal()};
				}
			}
			return new EntityRaycastResult { Hit = false };
		}


		
	}
}
