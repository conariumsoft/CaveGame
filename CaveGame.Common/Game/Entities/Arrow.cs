using CaveGame.Common.Game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using CaveGame.Common.Extensions;

namespace CaveGame.Common.Game.Entities
{
	[Summonable]
    public class Arrow : PhysicsEntity, IServerPhysicsObserver, IClientPhysicsObserver
    {
        public override float Mass => 0.3f;
		public bool EmbeddedInTile { get; set; }
		public Tile AttachedTile { get; set; }
		public bool EmbeddedInEntity { get; set; }
		public Point AttachedTileLocation { get; set; }

		public IPhysicsEntity AttachedEntity { get; set; }
		public Vector2 EntityAttachLocation { get; set; }

		public Rotation Direction;

        public override Vector2 TopLeft => Position - new Vector2(1, 1);

        public override Vector2 BoundingBox => new Vector2(1, 1);

        public void ClientPhysicsTick(IClientWorld world, float step) => PhysicsStep(world, step);
        public void ServerPhysicsTick(IServerWorld world, float step) => PhysicsStep(world, step);


		float damageWindup;
		public bool CanDamage => (damageWindup <= 0 && !EmbeddedInEntity && !EmbeddedInTile);

		public Arrow() : base()
        {
			damageWindup = 0.25f;
			Health = 5;
        }

		public override int MaxHealth => 5;
		
		public override int Defense => 1;


		public override void Draw(GraphicsEngine gfx)
        {

			gfx.Sprite(
				texture: gfx.ArrowEntity,
				position: Position,
				quad: null,
				color: Color.White,
				rotation: Direction,
				origin: new Vector2(8, 3),
				scale: 1f,
				efx: SpriteEffects.FlipHorizontally,
				layer: 0
			);
			base.Draw(gfx);
        }


        public override void ServerUpdate(IGameServer server, GameTime gt)
        {
			damageWindup -= gt.GetDelta();

			if (!EmbeddedInEntity && !EmbeddedInTile)
				Direction = Rotation.FromUnitVector(Velocity.Unit());

			base.ServerUpdate(server, gt);
        }
		public override void ClientUpdate(IGameClient client, GameTime gt)
		{
			damageWindup -= gt.GetDelta();

			if (!EmbeddedInEntity && !EmbeddedInTile)
				Direction = Rotation.FromUnitVector(Velocity.Unit());
			base.ClientUpdate(client, gt);
		}

		protected override void SolidCollisionReaction(IGameWorld world, Tile t, Vector2 separation, Vector2 normal, Point tilePos)
		{
			AttachedTileLocation = tilePos;
			AttachedTile = t;
			EmbeddedInTile = true;
			if (t.ID == 255)
            {
				Dead = true;
            }
		}

		public void OnCollide(IGameWorld world, PhysicsEntity meatbag, Vector2 separation, Vector2 normal)
		{
			if (CanDamage)
            {
				if (Velocity.Length() < 3)
                {
					if (world.IsServer())
                    {
#if !EDITOR // TODO: Why the fuck does Editor not have Math.Clamp
						int damage = (int)Math.Clamp(Velocity.Length() * 4, 2, 40);
						meatbag.Damage(DamageType.PunctureTrauma, this, damage);
						Health -= 1;
#endif
					}
					
					AttachedEntity = meatbag;
					EmbeddedInEntity = true;
					EntityAttachLocation = (this.Position - meatbag.Position);
				} else
                {
					if (world.IsServer())
                    {
#if !EDITOR
						int damage = (int)Math.Clamp(Velocity.Length() * 8, 2, 120);
						meatbag.Damage(DamageType.PunctureTrauma, this, damage);
						Health -= 1;
#endif
					}
					Velocity = Velocity * 0.95f;
					damageWindup = 0.25f;
				}
			}
		}

		private void CheckEmbeddedTileStatus(IGameWorld world)
        {
			
			if (world.GetTile(AttachedTileLocation.X, AttachedTileLocation.Y) != AttachedTile)
            {
				EmbeddedInTile = false;
				AttachedTile = null;
				damageWindup = 1.0f;
				Velocity = Vector2.Zero;
			}
		}

		public override void PhysicsStep(IGameWorld world, float step)
		{
			// TODO: check if tile is removed...
			if (EmbeddedInTile)
            {
				CheckEmbeddedTileStatus(world);
				return;
			}

			if (EmbeddedInEntity)
            {
				Position = AttachedEntity.Position + EntityAttachLocation;
				Velocity = AttachedEntity.Velocity;
				NextPosition = AttachedEntity.NextPosition + EntityAttachLocation;
				return;
            } 

			foreach (var ent in world.Entities)
			{
				if (ent is PhysicsEntity meatbag && !(meatbag is Arrow) && meatbag != this)
				{
					world.EntityRaycast(Position, Direction, 2);

					if (meatbag != this && CollisionSolver.CheckAABB(NextPosition, BoundingBox, meatbag.Position, meatbag.BoundingBox))
					{
						var separation = CollisionSolver.GetSeparationAABB(NextPosition, BoundingBox, meatbag.Position, meatbag.BoundingBox);
						var normal = CollisionSolver.GetNormalAABB(separation, Velocity - meatbag.Velocity);
						OnCollide(world, meatbag, separation, normal);
					}
				}
			}

			base.PhysicsStep(world, step);
		}

	}
}
