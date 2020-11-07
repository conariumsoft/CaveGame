using CaveGame.Core.Game.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.StatusEffects
{

	public abstract class StatusEffect
	{
		public float Duration { get; set; }
		public abstract string EffectName { get; }
		public abstract string EffectDescription { get; }

		public Entity TargetEntity { get; set; }

		public StatusEffect(float duration)
		{

		}

		public virtual void Tick(GameTime gt)
		{

		}

	}



	public class Burning : StatusEffect
	{
		public override string EffectName => "Burning";
		public override string EffectDescription => "";


		public Burning(float duration) : base(duration)
		{

		}

	}

	public class Poisoned : StatusEffect
	{
		public override string EffectName => "Poisoned";
		public override string EffectDescription => "";

		public Poisoned(float duration) : base(duration)
		{

		}

	}
}
