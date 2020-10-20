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
using System.Diagnostics;
using System.Text;

namespace CaveGame.Core.Entities
{
	public class Player : PhysicsEntity, IPhysicsObject, IPositional, IVelocity, INextPosition, IHorizontalDirectionState
	{

		public bool God { get; set; }
		public bool OnRope { get; set; }
		private static Vector2 _boundingBox = new Vector2(6, 12);
		private static float _mass = 1.5f;

		public override Vector2 BoundingBox => _boundingBox;
		public override float Mass => _mass;
		public override Vector2 Friction => new Vector2(5.0f, 1f);

		public User User { get; set; }
		public string DisplayName;

		public HorizontalDirection Facing { get; set; }

		public Color Color;

		public bool Walking { get; set; }

		protected float walkingAnimationTimer;

		public Player()
		{
			God = false;
			Position = new Vector2(0, -200);
			Velocity = Vector2.Zero;
			OnGround = false;
			Color = Color.White;
		}

		public override void ClientUpdate(IClientWorld world, GameTime gt)
		{
			walkingAnimationTimer += (float)gt.ElapsedGameTime.TotalSeconds * 5;
			base.ClientUpdate(world, gt);
		}

#if CLIENT
		public override void Draw(SpriteBatch sb) {

			Rectangle spriteFrame = new Rectangle(0, 0, 16, 24);

			int flipSprite = 0;
			if (Facing == HorizontalDirection.Left)
				flipSprite = 0;
			if (Facing == HorizontalDirection.Right)
				flipSprite = 1;


			if (Walking)
			{
				spriteFrame = new Rectangle(16, 0, 16, 24);
				if (walkingAnimationTimer % 2 >= 1)
					spriteFrame = new Rectangle(32, 0, 16, 24);
			}

			if (!OnGround)
				spriteFrame = new Rectangle(48, 0, 16, 24);

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

		public override void Draw(SpriteBatch sb)
		{
#if CLIENT
			sb.Print(GameFonts.Arial8,Color.White, Position - new Vector2(20, 30), DisplayName);
			base.Draw(sb);
#endif
		}
	}

	public class LocalPlayer : Player
	{
		float jumpEnergy;

		private void GodPhysicsStep(IGameWorld world, float step)
		{
			KeyboardState kb = Keyboard.GetState();

			float moveX=0;
			float moveY = 0;
			if (kb.IsKeyDown(Keys.W))
				moveY--;

			if (kb.IsKeyDown(Keys.S))
				moveY++;

			if (kb.IsKeyDown(Keys.A))
				moveX--;
			if (kb.IsKeyDown(Keys.D))
				moveX++;

			NextPosition += new Vector2(moveX, moveY) / (2.0f);
			Position += new Vector2(moveX, moveY) / (2.0f);
		}

		public override void OnCollide(IGameWorld world, Tile t, Vector2 separation, Vector2 normal, Point tilePos)
		{
			KeyboardState kb = Keyboard.GetState();
			if (t is Rope rope)
			{
				Debug.WriteLine("OnRope");
				if (kb.IsKeyDown(Keys.S))
					OnRope = true;
				if (kb.IsKeyDown(Keys.W))
					OnRope = true;
			}

			if (!(t is INonSolid solid))
			{
				if (normal.Y == 0 && normal.X != 0)
				{
					if ((Velocity.X > 0.05f || Velocity.X < -0.05f))
					{
						var tryY = tilePos.Y * Globals.TileSize;
						if (tryY > NextPosition.Y)
						{
							Velocity = new Vector2(Velocity.X, Velocity.Y-0.25f);
							//NextPosition = new Vector2(NextPosition.X, tryY - BoundingBox.Y - 0.5f);
						}
					}
				}
			}

			base.OnCollide(world, t, separation, normal, tilePos);
		}

		public override void PhysicsStep(IGameWorld world, float step)
		{
			if (God)
			{
				GodPhysicsStep(world, step);
				return;
			}
			KeyboardState kb = Keyboard.GetState();

			float velX = 0;
			float horizspeed = 13;
			float velY = 0;
			float jump = 150;

			if (OnRope)
			{
				
				OnGround = true;

				if (kb.IsKeyDown(Keys.S))
					velY -= 0.1f;
				if (kb.IsKeyDown(Keys.W))
					velY += 0.1f;
			} else {
				Walking = false;
				

				if (kb.IsKeyDown(Keys.A))
				{
					Walking = true;
					if (velX > -1.0f)
					{
						velX -= step * horizspeed;
					}

					Facing = HorizontalDirection.Left;
				}


				if (kb.IsKeyDown(Keys.D))
				{
					Walking = true;
					if (velX < 1.0f)
					{
						velX += step * horizspeed;
					}
					Facing = HorizontalDirection.Right;
				}

				
			}


			if (OnGround)
			{
				jumpEnergy = 0.22f;
			}

			FallThrough = kb.IsKeyDown(Keys.S);

			if (kb.IsKeyDown(Keys.Space))
			{
				if (OnGround)
				{
					velY -= step * jump;
					OnRope = false;
				} else
				{
					jumpEnergy -= step;
					if (jumpEnergy > 0)
					{
						velY -= step * jump * 0.07f;
					}
				}
			}



			Velocity += new Vector2(velX, velY);

			base.PhysicsStep(world, step);
		}

	}

}
