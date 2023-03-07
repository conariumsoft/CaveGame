using CaveGame.Common.Game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using CaveGame.Common.Extensions;

namespace CaveGame.Common
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
		public static Vector2 BoundingBox = new Vector2(2, 2);
		public override float MaxParticleAge => 2.0f;

		public Rotation Rotation { get; set; }
        public Vector2 Position { get; set; }

		public Color Color { get; set; }
		public Vector2 Scale { get; set; }
		public Vector2 NextPosition;
		public Vector2 Velocity;
		public Vector2 Accelleration;

		public SmokeParticle() { }



		public void Initialize(Vector2 _position, Color _color, Rotation _rotation, Vector2 _scale, Vector2 _accel)
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
			var tilePosition = new Point(
				(int)Math.Floor(Position.X / Globals.TileSize),
				(int)Math.Floor(Position.Y / Globals.TileSize)
			);

			int bb = 2;
			for (int x = -bb; x < bb; x++)
			{
				for (int y = -bb; y < bb; y++)
				{
					Point tileBoxPos = new Point(tilePosition.X + x, tilePosition.Y + y);

					var tile = world.GetTile(tileBoxPos.X, tileBoxPos.Y);

					if (tile.ID != 0 && (!(tile is INonSolid) || tile is ILiquid))
					{
						var tileChec = (tileBoxPos.ToVector2() * Globals.TileSize) + new Vector2(4, 4);
						var tileBoxSize = new Vector2(4, 4);
						if (CollisionSolver.CheckAABB(NextPosition, BoundingBox * Scale, tileChec, tileBoxSize))
						{
							var separation = CollisionSolver.GetSeparationAABB(NextPosition, BoundingBox, tileChec, tileBoxSize);
							var normal = CollisionSolver.GetNormalAABB(separation, Velocity);
							if (tile.ID > 0 && !(tile is INonSolid))
							{
								NextPosition += separation;
							}
						}
					}
				}
			}


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

	public class ExplosionParticle : Particle
    {
		public static Rectangle SP_EXPLOSION0 = new Rectangle(0, 0, 32, 32);
		public static Rectangle SP_EXPLOSION1 = new Rectangle(32, 0, 32, 32);
		public static Rectangle SP_EXPLOSION2 = new Rectangle(64, 0, 32, 32);
		public static Rectangle SP_EXPLOSION3 = new Rectangle(96, 0, 32, 32);
		public static Rectangle SP_EXPLOSION4 = new Rectangle(128, 0, 32, 32);


		public static Rectangle[] ANIM =
		{
			SP_EXPLOSION0,
			SP_EXPLOSION1,
			SP_EXPLOSION2,
			SP_EXPLOSION3,
			SP_EXPLOSION4,

		};

		public static Vector2 Origin => new Vector2(16, 16);

		public override float MaxParticleAge => 0.5f;


		public Vector2 Position { get; set; }
		public Vector2 Scale { get; set; }
		public Color Color { get; set; }
		Rotation Rotation { get; set; }

		Random RNG = new Random();

		public ExplosionParticle()
		{
			Rotation = Rotation.FromDeg(RNG.Next(0, 360));
		}

        public override void Update(GameTime gt)
        {
			ParticleAge += (float)gt.ElapsedGameTime.TotalSeconds;
        }

        public override void PhysicsStep(IGameWorld world, float step)
        {
         //   base.PhysicsStep(world, step);
        }

        public override void Draw(GraphicsEngine gfx)
        {

			var quad = ANIM.GetSpriteFrame( (ParticleAge/MaxParticleAge) * (ANIM.Length-1));

			gfx.Sprite(gfx.Explosion, Position, quad, Color.White, Rotation, Origin, Scale, SpriteEffects.None, 0);
		}

		public void Initialize(Vector2 _position, Color _color, Rotation _rotation, Vector2 _scale)
		{
			ParticleAge = 0;
			Position = _position;
			Color = _color;
			Scale = _scale;
			Dead = false;
		}
	}


	public class FireParticle : Particle
	{
		public static Vector2 Origin => new Vector2(16, 16);

		public override float MaxParticleAge => 0.3f;


		public Vector2 Position { get; set; }
		public Vector2 Scale { get; set; }
		public Color Color { get; set; }
		Rotation Rotation { get; set; }

		Random RNG = new Random();

		public static Color[] ColorGradient =
		{
			new Color(1, 1, 0), // fire starts yellow
			new Color(1, 0, 0), // turns red
		};

		public Color GetColor()
		{
			int alpha = (int)((this.ParticleAge * 2.0f) % 2);

			return Color.Lerp(Color.Yellow, Color.Red, this.ParticleAge * 3.0f);
		}


		public static float[] SizeGradient = { 2.0f, 3.5f, 1.0f };

		public Vector2 GetSizeV2()
		{
			int alpha = (int)(this.ParticleAge * 2) % 2;

			return new Vector2(2, 2);//Vector2(SizeGradient[alpha], SizeGradient[alpha]);
		}


		float rngRise;
		public FireParticle()
		{
			Dead = false;
			ParticleAge = 0;
			Rotation = Rotation.FromDeg(RNG.Next(0, 360));
			rngRise = (float)RNG.NextDouble()+1.0f;
		}

		public override void Update(GameTime gt)
		{
			ParticleAge += (float)gt.ElapsedGameTime.TotalSeconds;

			count += gt.GetDelta();
			Position -= new Vector2(0, gt.GetDelta() * ((rngRise+1f)*24f));
		}

		float count = 0;

		public override void PhysicsStep(IGameWorld world, float step)
		{
			
		}

		public override void Draw(GraphicsEngine gfx)
		{
			gfx.Rect(GetColor(), Position, new Vector2(2, 2));
		}

		public void Initialize(Vector2 _position, Color _color, Rotation _rotation, Vector2 _scale)
		{
			ParticleAge = 0;
			Position = _position;
			Color = _color;
			Scale = _scale;
			Dead = false;
		}
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


		private List<Particle> Particles;
		public IGameWorld World { get; set; }


		public ParticleEmitter(IGameWorld world)
		{
			World = world;
			Particles = new List<Particle>();
		}

		public void Add(Particle p) => Particles.Add(p);


		public void EmitSmokeParticle(Vector2 position, Color color, Rotation rotation, Vector2 scale, Vector2 accel)
		{
			var myParticle = new SmokeParticle();

			myParticle.Initialize(position, color, rotation, scale, accel);
			Add(myParticle);
		}
		public void EmitExplosionParticle(Vector2 position)
        {
			var myParticle = new ExplosionParticle();
			myParticle.Initialize(position, Color.White, Rotation.Zero, new Vector2(2.0f));
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

				if (particle.Dead)
				{

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
