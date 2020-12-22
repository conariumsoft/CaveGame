using CaveGame.Core;
using CaveGame.Core.Game.Entities;
using CaveGame.Core.Game.Inventory;
using CaveGame.Core.Game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CaveGame.Client.Game.Entities
{



	public class LocalPlayer : Player, IClientPhysicsObserver
	{
		float jumpEnergy;

		



		public bool IgnoreInput { get; set; }

		private void GodPhysicsStep(IGameWorld world, float step)
		{
			KeyboardState kb = Keyboard.GetState();

			float moveX=0;
			float moveY = 0;
			if (kb.IsKeyDown(GameSettings.CurrentSettings.MoveUpKey))
				moveY--;

			if (kb.IsKeyDown(GameSettings.CurrentSettings.MoveDownKey))
				moveY++;

			if (kb.IsKeyDown(GameSettings.CurrentSettings.MoveLeftKey))
				moveX--;
			if (kb.IsKeyDown(GameSettings.CurrentSettings.MoveRightKey))
				moveX++;

			NextPosition += new Vector2(moveX, moveY) *(4.0f);
			Position += new Vector2(moveX, moveY) * (4.0f);


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

			if (!(t is INonSolid solid) && !(t is ILiquid liquid))
			{
				if (normal.Y == 0 && normal.X != 0)
				{
					if ((Velocity.X > 0.05f || Velocity.X < -0.05f))
					{
						var tryY = tilePos.Y * Globals.TileSize;
						if (tryY > NextPosition.Y)
						{
							Velocity = new Vector2(Velocity.X, Velocity.Y - 0.25f);
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
				if (!IgnoreInput) 
				{
					if (kb.IsKeyDown(GameSettings.CurrentSettings.MoveDownKey))
						velY -= 0.1f;
					if (kb.IsKeyDown(GameSettings.CurrentSettings.MoveUpKey))
						velY += 0.1f;
				}
			} else {
				Walking = false;
				
				if (!IgnoreInput) {
					if (kb.IsKeyDown(GameSettings.CurrentSettings.MoveLeftKey))
					{
						Walking = true;
						if (velX > -1.0f)
						{
							velX -= step * horizspeed;
						}

						Facing = Direction.Left;
					}


					if (kb.IsKeyDown(GameSettings.CurrentSettings.MoveRightKey))
					{
						Walking = true;
						if (velX < 1.0f)
						{
							velX += step * horizspeed;
						}
						Facing = Direction.Right;
					}
				}
				

				
			}


			if (OnGround)
			{
				jumpEnergy = 0.22f;
			}
			if (IgnoreInput) {
				FallThrough = false;
			} else {

				FallThrough = kb.IsKeyDown(GameSettings.CurrentSettings.MoveDownKey);

				if (kb.IsKeyDown(GameSettings.CurrentSettings.JumpKey))
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
			}

			Velocity += new Vector2(velX, velY);

			base.PhysicsStep(world, step);
		}

		public override void ClientPhysicsTick(IClientWorld world, float step) => PhysicsStep(world, step);

	}
}
