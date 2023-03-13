using CaveGame.Common.Game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using CaveGame.Common.Generic;

#if CLIENT
using CaveGame.Client;
#endif

namespace CaveGame.Common.Game.Entities
{

	[Summonable]
	public class Bomb : PhysicsEntity, IServerPhysicsObserver, IClientPhysicsObserver
	{

		public void ClientPhysicsTick(IClientWorld world, float step) => PhysicsStep(world, step);
		public override float Mass => 0.8f;

        public override int MaxHealth => 30;
		public override int Defense => 5;

        public override Vector2 BoundingBox => new Vector2(6, 6);

		public override Vector2 Friction => new Vector2(0.95f, 0.95f);

		private float detonationCountdown;

		// collides with other bomb
		public void OnCollide(IGameWorld world, Bomb family, Vector2 separation, Vector2 normal)
		{
			family.Velocity += normal*(Velocity);
		}

        protected override void SolidCollisionReaction(IGameWorld world, Tile t, Vector2 separation, Vector2 normal, Point tilePos)
        {
			if (normal.Y == 1)
			{
				Velocity = new Vector2(Velocity.X * 0.95f, -Velocity.Y * 0.6f);
			}
			if (normal.Y == -1)
			{
				Velocity = new Vector2(Velocity.X * 0.90f, -Velocity.Y * 0.8f);
				//OnGround = true;
			}
			if (normal.X == -1)
			{
				Velocity = new Vector2(-Velocity.X * 0.75f, Velocity.Y * 0.95f);
			}
			if (normal.X == 1)
			{
				Velocity = new Vector2(-Velocity.X * 0.75f, Velocity.Y * 0.95f);
			}
		}


        public Bomb()
		{
			Health = MaxHealth;
		}

		void Explode(IGameWorld world)
		{
			Explosion explosion = new Explosion
			{
				Position = this.Position,
				Thermal = false, // TODO: sets shit on fire?
				BlastPressure = 5,
				BlastRadius = 5,
				Source = this,
			};

			world.Explosion(explosion, true, true);
			//world.Explosion(this.Position, 5, 2, true, true);
			this.Dead = true;
		}

		public override void PhysicsStep(IGameWorld world, float step)
		{
			foreach(var ent in world.Entities)
			{
				if (ent is Bomb family)
				{
					
					if (family!=this && CollisionSolver.CheckAABB(NextPosition, BoundingBox, family.Position, family.BoundingBox))
					{
						var separation = CollisionSolver.GetSeparationAABB(NextPosition, BoundingBox, family.Position, family.BoundingBox);

						var normal = CollisionSolver.GetNormalAABB(separation, Velocity-family.Velocity);
						OnCollide(world, family, separation, normal);
					}
				}
			}
			base.PhysicsStep(world, step);
		}

        public override void ClientUpdate(IGameClient client, GameTime gt)
        {
			//client.World.Lighting.InvokeLight(Position.ToTileCoords(), new Light3(64, 32, 0));
            base.ClientUpdate(client, gt);
        }

        public override void ServerUpdate(IGameServer server, GameTime gt)
		{
			detonationCountdown += (float)gt.ElapsedGameTime.TotalSeconds*4;


			if (detonationCountdown > Health)
				Explode(server.World);

			base.ServerUpdate(server, gt);
		}

		public void ServerPhysicsTick(IServerWorld world, float step) => PhysicsStep(world, step);

		public override void Draw(GraphicsEngine gfx)
		{
			gfx.Sprite(
				texture: gfx.BombSprite,
				position: TopLeft,
				quad: null,
				color: Illumination.MultiplyAgainst(Color.White),
				rotation: Rotation.Zero,
				origin: Vector2.Zero,
				scale: 0.75f,
				efx: SpriteEffects.None,
				layer: 0
			);
			//DrawHealth(gfx);
		}
    }
}
