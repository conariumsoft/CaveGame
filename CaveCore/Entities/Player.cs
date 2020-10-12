#if CLIENT
using CaveGame.Client;
#endif
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Text;

namespace CaveGame.Core.Entities
{
	public enum HorizontalDirection: byte
	{
		Left = 0,
		Right
	}

	public abstract class PhysicsEntity : Entity, IPhysicsObject, IPositional, IVelocity, INextPosition, IBoundingBox, IFriction
	{
		public abstract float Mass { get; }
		public abstract Vector2 BoundingBox { get; }
		public abstract Vector2 Friction { get; }

		public virtual Vector2 Velocity { get; set; }
		public virtual Vector2 Position { get; set; }
		public virtual Vector2 NextPosition { get; set; }
		

		public bool OnGround { get; set; }
		

		public virtual void ApplyGravity(IGameWorld world, float step)
		{
			if (Velocity.Y < World.TerminalVelocity)
				Velocity = new Vector2(Velocity.X, Velocity.Y + (World.Gravity * Mass * step));
		}

		public virtual void ApplyAirResistance(IGameWorld world, float step)
		{
			var xForce = World.AirResistance * Friction.X;
			var yForce = World.AirResistance * Friction.Y;
			Velocity = new Vector2(Velocity.X * xForce, Velocity.Y);
		}

		public virtual void OnCollide(IGameWorld world, Tile t, Vector2 separation, Vector2 normal)
		{
			if (normal.Y == 1)
			{
				Velocity = new Vector2(Velocity.X, -Velocity.Y * 0.5f);
			}
			if (normal.Y == -1)
			{
				Velocity = new Vector2(Velocity.X, 0);
				OnGround = true;
			}
			NextPosition += separation;
		}

		public override void Update(IGameWorld world, GameTime gt)
		{
			base.Update(world, gt);
		}

		public virtual void CollisionTest(IGameWorld world, float step)
		{
			OnGround = false;
			var tilePosition = new Point(
				(int)Math.Floor(Position.X / Globals.TileSize),
				(int)Math.Floor(Position.Y / Globals.TileSize)
			);
			int bb = 4;
			for (int x = -bb; x < bb; x++)
			{
				for (int y = -bb; y < bb; y++)
				{
					Point tileBoxPos = new Point(tilePosition.X + x, tilePosition.Y + y);

					var tile = world.GetTile(tileBoxPos.X, tileBoxPos.Y);

					if (tile.ID != 0 && !(tile is INonSolid))
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
							OnCollide(world, tile, separation, normal);
						}
					}
				}
			}
		}

		public virtual void PhysicsStep(IGameWorld world, float step)
		{
			CollisionTest(world, step);
			ApplyAirResistance(world, step);
			ApplyGravity(world, step);

			Position = NextPosition;
			NextPosition += Velocity;

			
		}
	}
	

	public class Player : PhysicsEntity, IPhysicsObject, IPositional, IVelocity, INextPosition, IHorizontalDirectionState
	{
		private static Vector2 _boundingBox = new Vector2(6, 12);
		private static float _mass = 1.5f;
		private static Vector2 _friction = new Vector2(0.98f, 1.0f);

		public override Vector2 BoundingBox => _boundingBox;
		public override float Mass => _mass;
		public override Vector2 Friction => _friction;

		public User User { get; set; }
		public string DisplayName;

		public HorizontalDirection Facing { get; set; }

		public Color Color;

		public bool Walking { get; set; }

		protected float walkingAnimationTimer;

		public Vector2 TopLeft
		{
			get { return Position - BoundingBox; }
		}

		public Player()
		{
			Position = new Vector2(0, -200);
			Velocity = Vector2.Zero;
			OnGround = false;
			Color = Color.White;
		}



		public override void Update(IGameWorld world, GameTime gt)
		{
			walkingAnimationTimer += (float)gt.ElapsedGameTime.TotalSeconds * 5;
			base.Update(world, gt);
		}

#if CLIENT
		public virtual void Draw(SpriteBatch sb) {

			Rectangle spriteFrame = new Rectangle(0, 0, 16, 24);

			int flipSprite = 0;
			if (Facing == HorizontalDirection.Left)
			{
				flipSprite = 0;
			}
			if (Facing == HorizontalDirection.Right)
			{
				flipSprite = 1;
			}


			if (Walking)
			{
				spriteFrame = new Rectangle(16, 0, 16, 24);
				if (walkingAnimationTimer % 2 >= 1)
				{
					spriteFrame = new Rectangle(32, 0, 16, 24);
				}
			}

			if (!OnGround)
			{
				spriteFrame = new Rectangle(48, 0, 16, 24);
			}

			


			sb.Draw(GameTextures.Player, TopLeft, spriteFrame, Color, 0, new Vector2(0,0), 1, (SpriteEffects)flipSprite, 0);
		}
#endif

	}

	public class PeerPlayer : Player
	{
		public override void PhysicsStep(IGameWorld world, float step)
		{
			Position = Position.Lerp(NextPosition, 0.5f);
			return;
		}
	}

	public class LocalPlayer : Player
	{
		float jumpEnergy;

		public override void PhysicsStep(IGameWorld world, float step)
		{
			KeyboardState kb = Keyboard.GetState();

			float velX = 0;
			float velY = 0;

			float speed = 6;
			float horizspeed = 8;

			if (OnGround)
			{
				jumpEnergy = 0.2f;
			}


			if (kb.IsKeyDown(Keys.Space))
			{
				if (OnGround)
				{
					velY -= step * speed * 16;
				} else
				{
					jumpEnergy -= step;
					if (jumpEnergy > 0)
					{
						velY -= step * speed * 1.0f;
					}
					
				}
			}

			Walking = false;

			if (kb.IsKeyDown(Keys.A))
			{
				Walking = true;
				velX -= step * horizspeed;
				Facing = HorizontalDirection.Left;
			}
				

			if (kb.IsKeyDown(Keys.D))
			{
				Walking = true;
				velX += step * horizspeed;
				Facing = HorizontalDirection.Right;
			}
			
			Velocity += new Vector2(velX, velY);

			base.PhysicsStep(world, step);
		}

		public override void Update(IGameWorld world, GameTime gt)
		{
			
			base.Update(world, gt);
		}
	}

}
