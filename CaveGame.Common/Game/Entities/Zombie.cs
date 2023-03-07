using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CaveGame.Common.Extensions;

namespace CaveGame.Common.Game.Entities
{


	public enum ThinkerState
	{
		IDLE,
		SEARCHING,
		ATTACKWINDUP,
		ATTACK,
		ATTACKTRANSITION,
		STUN,
		DEAD,

	}

	public class Zombie : PhysicsEntity, IServerPhysicsObserver, ICanBleed, IThinker
	{
		public static Random RNG = new Random();
		public override Vector2 BoundingBox => new Vector2(8, 12);
		public override float Mass => 1.4f;
		public override Vector2 Friction => new Vector2(5.0f, 1f);
		public override int MaxHealth => 20;
		public float ReactionTime => 0.2f;


		public ThinkerState StateOfMind { get; set; }
		public float ThinkingTimer { get; set; }
		public static Rectangle[] ANIMATIONS =
		{

		};

		
		public Entity Target { get; set; }
		public float TargetTime { get; set; }
		

		public Zombie()
		{

		}

		public override void Damage(DamageType type, IDamageSource source, int amount)
		{
			if (source is Entity assaulter)
			{
				Target = assaulter;
				TargetTime = 10;
			}
			base.Damage(type, source, amount);
		}


		public override void ServerUpdate(IGameServer server, GameTime gt)
		{
			TargetTime -= gt.GetDelta();

			ThinkingTimer += gt.GetDelta();
			if (ThinkingTimer > ReactionTime)
			{
				ThinkingTimer = 0;
				Think(server, gt);
			}

			base.ServerUpdate(server, gt);
		}

		public void Think(IGameServer server, GameTime gt)
		{
			// stop paying attention to dead entities
			if (Target == null || Target.Dead == true)
				TargetTime = 0;

			// look for new target
			if (TargetTime <= 0)
			{
				// pick nearest player
				Player closest = null;
				foreach (var ent in server.World.Entities)
				{
					if (!(ent is Player plr))
						continue;

					if (closest == null)
					{
						closest = plr;
						continue;
					}
					if (closest.Position.Distance(Position) > plr.Position.Distance(Position))
						closest = plr;	
				}
			}
		}

		public override void ClientUpdate(IGameClient client, GameTime gt)
		{
			base.ClientUpdate(client, gt);
		}


		/*public override void Draw(GraphicsEngine GFX)
		{
			Rectangle spriteFrame = SP_IDLE;

			int flipSprite = 0;
			if (Facing == Direction.Left)
				flipSprite = 0;
			if (Facing == Direction.Right)
				flipSprite = 1;


			if (Walking)
			{
				spriteFrame = WALK_CYCLE.GetSpriteFrame(animationTimer * walk_anim_time);
				if (Pushing)
					spriteFrame = SP_PUSH0; //PUSH_CYCLE.GetSpriteFrame(animationTimer * push_anim_time);
			}
			if (!OnGround)
			{
				if (Velocity.Y > 0)
					spriteFrame = SP_JUMP;
				else
					spriteFrame = SP_FALL;
			}



			DrawHealth(gfx);
			DrawName(gfx);

			GFX.Sprite(GFX.Zombie, Position, spriteFrame, Illumination.MultiplyAgainst(Color), Rotation.Zero, new Vector2(8, 12), 1, (SpriteEffects)flipSprite, 0);
			base.Draw(GFX);
		}*/

		public void ServerPhysicsTick(IServerWorld world, float step)
		{
			throw new NotImplementedException();
		}

		
	}
}
