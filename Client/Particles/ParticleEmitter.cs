using CaveGame.Core.Entities;
using CaveGame.Core.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Particles
{
	public interface IParticle: IPositional, IVelocity, INextPosition
	{

	}

	public abstract class Particle
	{

	}

	public class ParticleEmitter
	{
		private CircularArray<IParticle> particles;

		public ParticleEmitter(IGameWorld world)
		{

		}

		public void Update(GameTime gt)
		{

		}

		public void Draw(SpriteBatch sb)
		{

		}

	}
}
