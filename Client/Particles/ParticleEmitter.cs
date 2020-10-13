using CaveGame.Client;
using CaveGame.Core.Entities;
using CaveGame.Core.Generic;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CaveGame.Core.Particles
{
	public struct Rotation : IEquatable<Rotation>
	{
		public const float PI = MathHelper.Pi;
		private float _radians;

		public float Radians
		{
			get { return _radians; }
			set { _radians = value; }
		}
		public float Degrees
		{
			get { return _radians * (180.0f / PI); }
			set { _radians = value * (PI / 180.0f); }
		}

		public static Rotation FromRad(float radians)
		{
			return new Rotation { Radians = radians };
		}

		public static Rotation FromDeg(float degree)
		{
			return new Rotation { Degrees = degree };
		}

		public bool EqualsRadians(float rad, float toleranceDegrees = 1.0f)
		{
			return Math.Abs(Radians - rad) < (toleranceDegrees * (PI / 180.0f));
		}
		public bool EqualsDegrees(float deg, float toleranceDegrees = 1.0f)
		{
			return Math.Abs(Degrees - deg) < toleranceDegrees;
		}

		public bool Equals(Rotation other)
		{
			return EqualsRadians(other.Radians);
		}
	}

	public abstract class Particle
	{
		public virtual void Draw(SpriteBatch sb) { }
		public virtual void Update(GameTime gt) { }
		public virtual void PhysicsStep(IGameWorld world, float step) { }
		public virtual void OnCollide(IGameWorld world, Tile t, Vector2 separation, Vector2 Normal) { }
		public bool Dead { get; set; }
		public float ParticleAge { get; set; }
		public virtual float MaxParticleAge { get;  }
	}


	public class SmokeParticle : Particle
	{
		public static Rectangle Quad = new Rectangle(8, 0, 4, 4);
		public static Vector2 Origin = new Vector2(2, 2);
		public static Vector2 Friction = new Vector2(0.95f, 0.95f);
		public static float Mass = 0.1f;
		public override float MaxParticleAge => 1.0f;

		private Rotation rotation;
		private Vector2 position;
		private Color color;
		private float scale;
		private Vector2 nextPosition;
		private Vector2 velocity;



		public SmokeParticle(Vector2 _position, Color _color, Rotation _rotation, float _scale, Vector2 _velocity)
		{
			position = _position;
			color = _color;
			rotation = _rotation;
			scale = _scale;
			velocity = _velocity;
			Dead = false;
			nextPosition = _position;
		}

		public override void Update(GameTime gt)
		{
			ParticleAge += (float)gt.ElapsedGameTime.TotalSeconds;
			base.Update(gt);
		}

		public override void PhysicsStep(IGameWorld world, float step)
		{
			
			velocity = new Vector2(velocity.X * Friction.X, velocity.Y * Friction.Y);

			position = nextPosition;
			nextPosition += velocity;

			//base.PhysicsStep(world, step);
		}
		public override void Draw(SpriteBatch sb)
		{
			float alpha = Math.Min(1, (1- (ParticleAge / MaxParticleAge))*2);


			sb.Draw(GameTextures.ParticleSet, position, Quad, color*alpha, rotation.Radians, Origin, scale, SpriteEffects.None, 0);
		}

		public override void OnCollide(IGameWorld world, Tile t, Vector2 separation, Vector2 Normal)
		{
			// Do nothing
		}
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
	public class RainParticle : Particle
	{

	}

	public class ParticleEmitter
	{
		const int MAX_PARTICLES = 4096;
		private CircularArray<Particle> particles;
		public IGameWorld World { get; set; }

		public ParticleEmitter(IGameWorld world)
		{
			World = world;
			particles = new CircularArray<Particle>(MAX_PARTICLES);
		}

		public void Add(Particle p)
		{
			particles.Next(p);
		}

		public void Update(GameTime gt)
		{
			Particle particle;
			for (int i = 0; i <particles.Size; i++)
			{
				particles.Get(i, out particle);
				if (particle == null)
					continue;

				if (particle.ParticleAge > particle.MaxParticleAge)
					particle.Dead = true;

				if (particle.Dead)
					continue;
					

				

				particle.Update(gt);
			}
		}

		public void Draw(SpriteBatch sb)
		{
			Particle particle;
			for (int i = 0; i < particles.Size; i++)
			{
				particles.Get(i, out particle);
				if (particle == null)
					continue;
				if (particle.Dead)
					continue;

				particle.Draw(sb);
			}
		}

		public void PhysicsStep(IGameWorld world, float step)
		{
			Particle particle;
			for (int i = 0; i < particles.Size; i++)
			{
				particles.Get(i, out particle);
				if (particle == null)
					continue;

				if (particle.Dead)
					continue;

				particle.PhysicsStep(world, step);
			}
		}

	}
}
