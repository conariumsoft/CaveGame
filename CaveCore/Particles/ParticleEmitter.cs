using CaveGame.Core.Entities;
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
		public ParticleEmitter(IGameWorld world)
		{

		}

	}
}
