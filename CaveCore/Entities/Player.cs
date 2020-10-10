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
using System.Text;

namespace CaveGame.Core.Entities
{
	public enum HorizontalDirection: byte
	{
		Left = 0,
		Right
	}

	

	public abstract class Player : Entity, IPhysicsObject, IPositional, IVelocity, INextPosition, IHorizontalDirectionState
	{
		public User User { get; set; }
		public string DisplayName;

		public HorizontalDirection Facing { get; set; }
		public Vector2 Position { get; set; }
		public Vector2 Velocity { get; set; }
		public Vector2 BoundingBox => new Vector2(6, 12);
		public Vector2 NextPosition { get; set; }

		public Color Color;
		public bool OnGround;

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

		public virtual void PhysicsStep(IGameWorld world, float step)
		{
			

			OnGround = false;
			var tilePosition = new Vector2(Position.X / Globals.TileSize, Position.Y / Globals.TileSize);

			int bb = 4;
			for (int x = -bb; x < bb; x++)
			{
				for (int y = -bb; y < bb; y++)
				{
					Vector2 tileBoxPos = new Vector2(tilePosition.X + x, tilePosition.Y + y);

					var tile = world.GetTile((int)tileBoxPos.X, (int)tileBoxPos.Y);

					if (tile.ID != 0 && !(tile is INonSolid))
					{
						var tileChec = (tileBoxPos * Globals.TileSize) + new Vector2(4, 4);
						var tileBoxSize = new Vector2(4, 4);
						if (CollisionSolver.CheckAABB(NextPosition, BoundingBox, tileChec, tileBoxSize))
						{
							var separation = CollisionSolver.GetSeparationAABB(NextPosition, BoundingBox, tileChec, tileBoxSize);

							var normal = CollisionSolver.GetNormalAABB(separation, Velocity);

							if (normal.X != 0 && normal.Y != 0)
							{

							}
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
							//return;
						}
					}
				}
			}

			


			Velocity = new Vector2(Velocity.X * 0.92f, Velocity.Y);

			float gravity = 4f;
			Velocity = new Vector2(Velocity.X, Velocity.Y + gravity * step);

			Position = NextPosition;
			NextPosition += Velocity;

		}

		public override void Update(IGameWorld world, GameTime gt)
		{
			walkingAnimationTimer += (float)gt.ElapsedGameTime.TotalSeconds * 5;
			base.Update(world, gt);
		}

		public void OnCollide(IGameWorld world, Tile t, Vector2 separation, Vector2 normal)
		{
			throw new NotImplementedException();
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
