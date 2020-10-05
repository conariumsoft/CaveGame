using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Entities
{
	public class Player : Entity, IPhysicsObject, IPositional, IVelocity, INextPosition
	{

		public string DisplayName;

		public Vector2 Position { get; set; }
		public Vector2 Velocity { get; set; }
		public Vector2 BoundingBox;
		public Vector2 NextPosition { get; set; }
		public Color Color;
		public bool OnGround;

		public bool NotMyProblem { get; set; }

		public Vector2 TopLeft
		{
			get { return Position - BoundingBox; }
		}

		public Player()
		{
			Position = new Vector2(0, -100);
			Velocity = Vector2.Zero;
			BoundingBox = new Vector2(8, 12);
			OnGround = false;
			Color = Color.Red;
		}

		public virtual void PhysicsStep(IGameWorld world, float step)
		{
			if (NotMyProblem)
			{
				Position = Position.Lerp(NextPosition, 0.5f);

				return;
			}

			OnGround = false;
			var tilePosition = new Vector2(Position.X / Globals.TileSize, Position.Y / Globals.TileSize);
			tilePosition.Floor();

			int bb = 4;
			for (int x = -bb; x < bb; x++)
			{
				for (int y = -bb; y < bb; y++)
				{
					Vector2 tileBoxPos = new Vector2(tilePosition.X + x, tilePosition.Y + y);

					var tile = world.GetTile((int)tileBoxPos.X, (int)tileBoxPos.Y);

					if (tile.ID != 0)
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

			Velocity = new Vector2(Velocity.X * 0.95f, Velocity.Y);

			float gravity = 3f;
			Velocity = new Vector2(Velocity.X, Velocity.Y + gravity * step);

			Position = NextPosition;
			NextPosition += Velocity;

		}

		public override void Update(IGameWorld world, GameTime gt)
		{
			
			base.Update(world, gt);
		}
	}

	public class LocalPlayer : Player
	{

		public override void PhysicsStep(IGameWorld world, float step)
		{
			
			KeyboardState kb = Keyboard.GetState();

			float velX = 0;
			float velY = 0;

			float speed = 10;
			if (kb.IsKeyDown(Keys.Up))
				velY -= step * speed*1;


			if (kb.IsKeyDown(Keys.Left))
				velX -= step * speed;

			if (kb.IsKeyDown(Keys.Right))
				velX += step * speed;

			
			Velocity += new Vector2(velX, velY);

			base.PhysicsStep(world, step);
		}

		public override void Update(IGameWorld world, GameTime gt)
		{
			base.Update(world, gt);
		}
	}

}
