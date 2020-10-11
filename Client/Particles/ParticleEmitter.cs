using CaveGame.Core.Entities;
using CaveGame.Core.Generic;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Particles
{
	public interface IParticle: IPositional, IVelocity, INextPosition, IPhysicsObject
	{
		void Draw(SpriteBatch sb) { }
		void Update(GameTime gt) { }
	}

	public abstract class Particle : IParticle
	{
		public Vector2 Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Vector2 Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Vector2 NextPosition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public virtual void Draw(SpriteBatch sb) { }

		public void OnCollide(IGameWorld world, Tile t, Vector2 separation, Vector2 normal)
		{
			throw new NotImplementedException();
		}

		public void PhysicsStep(IGameWorld world, float step)
		{
			throw new NotImplementedException();
		}

		public virtual void Update(GameTime gt) { }
	}

	public class SmokeParticle
	{

	}
	public class FireParticle
	{

	}
	public class DustParticle
	{

	}
	public class SplashParticle
	{

	}

	public class ParticleEmitter
	{
		const int MAX_PARTICLES = 4096;
		private CircularArray<IParticle> particles;
		public IGameWorld World { get; set; }

		public ParticleEmitter(IGameWorld world)
		{
			World = world;
			particles = new CircularArray<IParticle>(MAX_PARTICLES);
		}

		public void Update(GameTime gt)
		{
			IParticle particle;
			for (int i = 0; i <particles.Size; i++)
			{
				particle = null;
				particles.Get(i, out particle);
				if (particle == null)
					continue;

				particle.Update(gt);
			}
		}

		public void Draw(SpriteBatch sb)
		{
			IParticle particle;
			for (int i = 0; i < particles.Size; i++)
			{
				particle = null;
				particles.Get(i, out particle);
				if (particle == null)
					continue;

				particle.Draw(sb);
			}
		}

		public void PhysicsStep(IGameWorld world, float step)
		{

		}

	}
}
