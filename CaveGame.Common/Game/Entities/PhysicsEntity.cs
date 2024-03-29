﻿using CaveGame.Common.Game.Furniture;
using CaveGame.Common.Game.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace CaveGame.Common.Game.Entities
{
	
	public interface IServerPhysicsObserver {
		void ServerPhysicsTick(IServerWorld world, float step);
	}

	public interface IClientPhysicsObserver {
		void ClientPhysicsTick(IClientWorld world, float step);
	}
	public abstract class PhysicsEntity : Entity, IPhysicsEntity
	{
		public virtual Vector2 NextPosition { get; set; }

		public virtual Vector2 RecentVelocity { get; set; }

		public virtual Vector2 Velocity { get; set; }
		public virtual Vector2 Friction { get; }
		public abstract float Mass { get; }

		

		public bool FallThrough { get; set; }
		public virtual bool InLiquid { get; set; }
		public float LiquidViscosity { get; set; }
		
		

		public virtual float Buoyancy => Mass;


		public bool OnGround { get; set; }


		protected virtual void DrawHealth(GraphicsEngine GFX)
		{
			string hptext = Health + "/" + MaxHealth + " HP";
			Vector2 namebounds = GFX.Fonts.Arial8.MeasureString(Health + "/" + MaxHealth + " HP");
			GFX.Text(GFX.Fonts.Arial8, hptext, Position - new Vector2(0, (namebounds.Y*2) + BoundingBox.Y), Color.White, TextXAlignment.Center, TextYAlignment.Top);
		}

		protected void InterpolateServerPosition() => Position = Vector2.Lerp(Position, NextPosition, 0.5f);

		public virtual void ApplyGravity(IGameWorld world, float step)
		{
			if (InLiquid)
			{

 
				if (Mass > 1)
				{
					if (Velocity.Y < 0.25)
						Velocity = new Vector2(Velocity.X, Velocity.Y + (GameWorld.Gravity * (Buoyancy/2) * step));
				}
				if (Mass <= 1)
				{
					if (Velocity.Y > -0.25)
						Velocity = new Vector2(Velocity.X, Velocity.Y - (GameWorld.Gravity * (Buoyancy/2) * step));
				}
				return;
			}

			if (Velocity.Y < GameWorld.TerminalVelocity)
				Velocity = new Vector2(Velocity.X, Velocity.Y + (GameWorld.Gravity * Mass * step));
		}

		public virtual void ApplyAirResistance(IGameWorld world, float step)
		{
			var xForce = Velocity.X * GameWorld.AirResistance * Friction.X * Mass * step;
			var yForce = Velocity.Y * GameWorld.AirResistance * Friction.Y * Mass * step;
			Velocity = new Vector2(Velocity.X - xForce, Velocity.Y - yForce);
		}

		private void PlatformCollide(float platformBottom, Vector2 separation, Vector2 normal)
		{
			// Collision with PlatformTiles
			if (normal.X == 0 && normal.Y == -1 && FallThrough == false)
			{
				var bottom = Position.Y + BoundingBox.Y;

				if (bottom <= platformBottom)
				{
					NextPosition = NextPosition + separation;
					Velocity = new Vector2(Velocity.X * 0.99f, 0);
					OnGround = true;
				}
			}
		}

		public virtual void OnCollide(IGameWorld world, Furniture.FurnitureTile f, Vector2 separation, Vector2 normal)
		{

			if (f is Workbench)
			{
				var blockbottom = f.Position.Y * Globals.TileSize + (f.BoundingBox.Y);

				PlatformCollide(blockbottom, separation, normal);
				return;
			}
			if (f is WoodenDoor door)
			{
				if (normal.X != 0 && normal.Y == 0 && door.State == DoorState.Closed)
				{
					NextPosition += separation;
				}
			}
		}

		protected virtual bool LavaCollisionTest(IGameWorld world, Tile t, Vector2 separation, Vector2 normal, Point tilePos)
        {
			if (t is Lava _)
            {

				// Add burning effect
				if (!AffectedBy<StatusEffects.Burning>())
					AddEffect(new StatusEffects.Burning(5));

				// continually reset burning duration to 5 while touching lava
				ActiveEffects.Find(e => e is StatusEffects.Burning).Duration = 5;
				
				return true;
			}
			return false;
		}

		protected virtual bool PlatformCollisionTest(IGameWorld world, Tile t, Vector2 separation, Vector2 normal, Point tilePos)
        {
			if (t is IPlatformTile _)
			{
				var blockbottom = tilePos.Y * Globals.TileSize + (Globals.TileSize / 2);
				PlatformCollide(blockbottom, separation, normal);
				return true;
			}
			return false;
		}

		protected virtual bool LiquidCollisionTest(IGameWorld world, Tile t, Vector2 separation, Vector2 normal, Point tilePos)
        {
			if (t is ILiquid liquid)
			{
				if (normal.X != 0 || normal.Y != 0 && tilePos.Y * 8 <= Position.Y)
				{
					InLiquid = true;
					LiquidViscosity = liquid.Viscosity;

					Velocity = new Vector2(Velocity.X / liquid.Viscosity, Velocity.Y);
				}
				return true;
			}
			return false;
		}
		protected virtual void SolidCollisionReaction(IGameWorld world, Tile t, Vector2 separation, Vector2 normal, Point tilePos)
        {
			if (normal.Y == 1)
			{
				Velocity = new Vector2(Velocity.X, -Velocity.Y * 0.5f);
			}
			if (normal.Y == -1)
			{
				Velocity = new Vector2(Velocity.X * 0.99f, 0); // TODO: tile friction
				OnGround = true;
			}
		}

		public virtual void OnCollide(IGameWorld world, Tile t, Vector2 separation, Vector2 normal, Point tilePos)
		{
			if (LiquidCollisionTest(world, t, separation, normal, tilePos))
            {
				LavaCollisionTest(world, t, separation, normal, tilePos);
				return;
			}
			
			if (PlatformCollisionTest(world, t, separation, normal, tilePos))
				return;


			SolidCollisionReaction(world, t, separation, normal, tilePos);
			NextPosition += separation;
		}


		public virtual void CollisionTest(IGameWorld world, float step)
		{
			OnGround = false;
			InLiquid = false;
			foreach (var furniture in world.Furniture)
			{
				var fpos = (furniture.Position.ToVector2()*Globals.TileSize) + furniture.BoundingBox/2;
				if (CollisionSolver.CheckAABB(NextPosition, BoundingBox, fpos, furniture.BoundingBox))
				{
					var separation = CollisionSolver.GetSeparationAABB(NextPosition, BoundingBox, fpos, furniture.BoundingBox);

					var normal = CollisionSolver.GetNormalAABB(separation, Velocity);
					OnCollide(world, furniture, separation, normal);
				}
			}

			var tilePosition = new Point(
				(int)Math.Floor(Position.X / Globals.TileSize),
				(int)Math.Floor(Position.Y / Globals.TileSize)
			);

			int bb = 3;
			for (int x = -bb; x < bb; x++)
			{
				for (int y = -bb; y < bb; y++)
				{
					Point tileBoxPos = new Point(tilePosition.X + x, tilePosition.Y + y);

					var tile = world.GetTile(tileBoxPos.X, tileBoxPos.Y);

					if (tile.ID != 0 && (!(tile is INonSolid) || tile is ILiquid))
					{
						var tileChec = (tileBoxPos.ToVector2() * Globals.TileSize) + new Vector2(4, 4);
						var tileBoxSize = new Vector2(4, 4);
						if (CollisionSolver.CheckAABB(NextPosition, BoundingBox, tileChec, tileBoxSize))
						{
							var separation = CollisionSolver.GetSeparationAABB(NextPosition, BoundingBox, tileChec, tileBoxSize);

							var normal = CollisionSolver.GetNormalAABB(separation, Velocity);

							if (tile.ID == 255)
							{
								NextPosition = Position;
								Velocity = Vector2.Zero;
								continue;
							}
							OnCollide(world, tile, separation, normal, tileBoxPos);
						}
					}
				}
			}
		}



		Vector2 previousPos = Vector2.Zero;
		public virtual void PhysicsStep(IGameWorld world, float step)
		{
			CollisionTest(world, step);
			ApplyAirResistance(world, step);
			ApplyGravity(world, step);


			
			RecentVelocity = previousPos - Position;
			previousPos = Position;
			Position = NextPosition;
			NextPosition += Velocity;


		}
	}
}
