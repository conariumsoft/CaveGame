using CaveGame.Core.Game.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CaveGame.Core.Game.StatusEffects
{

	// POTION SELLER? https://www.youtube.com/watch?v=R_FQU4KzN7A
	public abstract class StatusEffect
	{
		public float Duration { get; set; }
		public abstract string EffectName { get; }
		public abstract string EffectDescription { get; }
		public Entity TargetEntity { get; set; }
		public StatusEffect(float duration)
		{
			Duration = duration;
		}
		public virtual void Tick(IGameWorld world, GameTime gt) { }
		public virtual void OnAdded(Entity victim) { }
		public virtual void OnRemoved(Entity victim) { }
	}


	public class AmphetamineRush : StatusEffect
	{
		public override string EffectName => "Amped";

		public override string EffectDescription { get; }

		public AmphetamineRush(float duration) : base(duration)
		{

		}
	}

	public class Burning : StatusEffect
	{
		public override string EffectName => "Burning";
		public override string EffectDescription => "";
		protected float fireDamageTimer { get; set; }
		float particleTimer;

		public Burning(float duration) : base(duration)
		{
			fireDamageTimer = 0;
		}

		Random rand = new Random();
		public override void Tick(IGameWorld world, GameTime gt)
		{
			particleTimer += gt.GetDelta();

			if (particleTimer > (0.01f))
			{
				if (world is IClientWorld localWorld)
				{
					for (int i = 0; i < 2; i++)
					{
						// calculate random position along the bottom face of the entity
						Vector2 RandomX = new Vector2(rand.Next(0, (int)TargetEntity.BoundingBox.X * 2) - (TargetEntity.BoundingBox.X), TargetEntity.BoundingBox.Y);
						Vector2 spawnCoords = TargetEntity.Position + RandomX;
						localWorld.ParticleSystem.Add(new FireParticle { Position = TargetEntity.Position + RandomX });
					}
				}
				particleTimer = 0;
			}


			fireDamageTimer += gt.GetDelta();

			// damage target 
			if (fireDamageTimer > (0.1f))
			{
				TargetEntity.Damage(DamageType.Fire, null, 1);
				fireDamageTimer = 0;
			}
			
			base.Tick(world, gt);
		}

	}

	//TODO:
	// Regenerate Status Effect
	// Speed Status Effect
	// 

	public class Poisoned : StatusEffect
	{
		public override string EffectName => "Poisoned";
		public override string EffectDescription => "";

		public Poisoned(float duration) : base(duration)
		{
			tickTimer = 0;
		}

		float tickTimer;

		public override void Tick(IGameWorld world, GameTime gt)
		{
			if (tickTimer > (0.1f))
			{
				TargetEntity.Damage(DamageType.Poison, null, 1);

				tickTimer = 0;
			}
			base.Tick(world, gt);
		}
	}
}
