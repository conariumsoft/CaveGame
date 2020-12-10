using CaveGame.Core.Game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Collections.Concurrent;

namespace CaveGame.Core
{
	
	public abstract class Particle
	{
		public virtual void Draw(GraphicsEngine gfx) { }
		public virtual void Update(GameTime gt) { }
		public virtual void PhysicsStep(IGameWorld world, float step) { }
		public virtual void OnCollide(IGameWorld world, Tile t, Vector2 separation, Vector2 Normal) { }
		public bool Dead { get; set; }
		public float ParticleAge { get; set; }
		public virtual float MaxParticleAge { get;  }


	}

	public class ObjectPool<T>
	{
		private readonly ConcurrentBag<T> _objects;
		private readonly Func<T> _objectGenerator;

		public ObjectPool(Func<T> objectGenerator)
		{
			_objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
			_objects = new ConcurrentBag<T>();
		}

		public T Get() => _objects.TryTake(out T item) ? item : _objectGenerator();

		public void Return(T item) => _objects.Add(item);
	}


	public class SmokeParticle : Particle
	{
		public static Rectangle Quad = new Rectangle(8, 0, 4, 4);
		public static Vector2 Origin = new Vector2(2, 2);
		public static Vector2 Friction = new Vector2(0.8f, 0.8f);
		public static float Mass = 0.1f;
		public override float MaxParticleAge => 2.0f;

		public Rotation Rotation { get; set; }
        public Vector2 Position { get; set; }

		public Color Color { get; set; }
		public float Scale { get; set; }
		public Vector2 NextPosition;
		public Vector2 Velocity;
		public Vector2 Accelleration;

		public SmokeParticle() { }



		public void Initialize(Vector2 _position, Color _color, Rotation _rotation, float _scale, Vector2 _accel)
        {
			ParticleAge = 0;
			Position = _position;
			Color = _color;
			Rotation = _rotation;
			Scale = _scale;
			Accelleration = _accel;
			//velocity = _velocity;
			NextPosition = _position;
			Dead = false;

		}

		public override void Update(GameTime gt)
		{
			ParticleAge += (float)gt.ElapsedGameTime.TotalSeconds;
			base.Update(gt);
		}

		public override void PhysicsStep(IGameWorld world, float step)
		{
			Velocity += (Accelleration * step*3);
			Accelleration -= (Accelleration * step*3);


			Velocity = new Vector2(Velocity.X * Friction.X, Velocity.Y * Friction.Y);

			Position = NextPosition;
			NextPosition += Velocity;

			//base.PhysicsStep(world, step);
		}
		public override void Draw(GraphicsEngine gfx)
		{
			float alpha = Math.Min(1, (1- (ParticleAge / MaxParticleAge))*2);


			gfx.Sprite(gfx.ParticleSet, Position, Quad, Color*alpha, Rotation, Origin, Scale, SpriteEffects.None, 0);
		}

		public override void OnCollide(IGameWorld world, Tile t, Vector2 separation, Vector2 Normal)
		{
			// Do nothing
		}
	}


	public class TileBloodSplatterParticle : Particle
	{


		public static Rectangle[] Quads =
		{
			new Rectangle(8, 8, 8, 2),
			new Rectangle(8, 10, 8, 2),
			new Rectangle(8, 12, 8, 2),
			new Rectangle(8, 14, 8, 2),
		};

		public static Vector2 Friction = new Vector2(0.8f, 0.8f);
		public static float Mass = 0.1f;

		int index;

		public TileBloodSplatterParticle()
        {
			index = Tile.RNG.Next(0, 3);
        }

		public override float MaxParticleAge => 100.0f;

		public Point AttachedTo { get; set; }
		public Vector2 Position { get; set; }
		public Face Face { get; set; }

		public Color Color => Color.Red*0.3f;

		public override void PhysicsStep(IGameWorld world, float step)
		{
			if (world.GetTile(AttachedTo.X, AttachedTo.Y) is INonSolid)
            {
				ParticleAge += 100;
			}
		}
		public override void Update(GameTime gt)
        {

			ParticleAge += (float)gt.ElapsedGameTime.TotalSeconds;


			base.Update(gt);
        }

		public override void Draw(GraphicsEngine GFX)
        {
			Vector2 pos = Position;//(AttachedTo.ToVector2() * Globals.TileSize) + new Vector2(4, 4);
			Rectangle quad = Quads[index];

			float particleAlpha = Math.Min(1.0f, 1 - (ParticleAge / MaxParticleAge) );

			if (Face == Face.Top)
				GFX.Sprite(GFX.ParticleSet, pos, quad, Color.White* particleAlpha, Rotation.FromDeg(270), new Vector2(8, 1), 1, SpriteEffects.None, 0);
            if (Face == Face.Left)
				GFX.Sprite(GFX.ParticleSet, pos, quad, Color.White* particleAlpha, Rotation.FromDeg(180), new Vector2(8, 1), 1, SpriteEffects.None, 0);
			if (Face == Face.Bottom)
				GFX.Sprite(GFX.ParticleSet, pos, quad, Color.White* particleAlpha, Rotation.FromDeg(90), new Vector2(8, 1), 1, SpriteEffects.None, 0);
			if (Face == Face.Right)
				GFX.Sprite(GFX.ParticleSet, pos, quad, Color.White* particleAlpha, Rotation.Zero, new Vector2(8, 1), 1, SpriteEffects.None, 0);
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


		ObjectPool<SmokeParticle> SmokeParticlePool = new ObjectPool<SmokeParticle>(() => new SmokeParticle());


		private List<Particle> Particles;
		public IGameWorld World { get; set; }


		public ParticleEmitter(IGameWorld world)
		{
			World = world;
			Particles = new List<Particle>();
		}

		public void Add(Particle p) => Particles.Add(p);


		public void EmitSmokeParticle(Vector2 position, Color color, Rotation rotation, float scale, Vector2 accel)
		{
			var myParticle = SmokeParticlePool.Get();

			myParticle.Initialize(position, color, rotation, scale, accel);
			Add(myParticle);
		}

		public void Update(GameTime gt)
		{
			foreach (var particle in Particles.ToArray())
			{
				if (particle == null)
					continue;

				if (particle.ParticleAge > particle.MaxParticleAge)
					particle.Dead = true;

				if (particle.Dead && particle is SmokeParticle smokey)
				{
					SmokeParticlePool.Return(smokey);
					Particles.Remove(particle);
					continue;
				}
					
				particle.Update(gt);
			}
		}

		public void Draw(GraphicsEngine gfx)
		{
			foreach (Particle particle in Particles)
			{
				if (particle == null)
					continue;
				if (particle.Dead)
					continue;

				particle.Draw(gfx);
			}
		}

		public void PhysicsStep(IGameWorld world, float step)
		{
			foreach (Particle particle in Particles)
			{
				if (particle == null)
					continue;

				if (particle.Dead)
					continue;

				particle.PhysicsStep(world, step);
			}
		}

	}
}
