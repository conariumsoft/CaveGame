using CaveGame.Core.Game.StatusEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Entities
{

	public class GameEntityAttribute : Attribute
    {

    }
	public class Summonable : Attribute {}

	public interface ICanBleed { }
	
	public interface IProvokable {
		bool Provoked { get; }
		void Provoke();
	}
	public interface IEntity : IDamageSource
	{
		Rectangle GetCollisionRect();
		Vector2 Position { get; set; }
		Vector2 BoundingBox { get; }
		int EntityNetworkID { get; set; }
		float DurationAlive { get; set; }
		bool Dead { get; set; }
		int Health { get; set; }
		int Defense { get; set; }
		int MaxHealth { get; }
		//void Update(IGameWorld world, GameTime gt);
		void ClientUpdate(IGameClient client, GameTime gt);
		void ServerUpdate(IGameServer server, GameTime gt);
		void Draw(GraphicsEngine engine);
		void Damage(DamageType type, IDamageSource source, int amount);
		void Damage(DamageType type, IDamageSource source, int amount, Vector2 direction);
		Light3 Illumination { get; }
	}


	public interface IPhysicsEntity : IEntity
    {
		Vector2 Velocity { get; set; }
		Vector2 NextPosition { get; set; }
		float Mass { get; }
		Vector2 Friction { get; }
	}
	public interface IThinker
    {
		float Anger { get; }
		float Fear { get; }
		float ResponseTime { get; } // time between Think() ticks
		int IQ { get; } // affects properties within Think() such as accuracy of pathfinding
		void Think(IGameServer server, GameTime gt);
    }

	public class Entity: IEntity
	{
		
		public virtual Vector2 TopLeft => Position - BoundingBox;

		#region Override Entity Properties
		public virtual Vector2 BoundingBox { get; }
		public virtual int Health { get; set; }

		public virtual int Defense { get; set; }
		public virtual int MaxHealth { get; }
		public virtual Vector2 Position { get; set; }
		#endregion

		public float DurationAlive { get; set; }
		public bool Dead { get; set; } // Gets collected on death
		public bool RemoteControlled { get; set; }
		public int EntityNetworkID { get; set; }
		
		public List<StatusEffect> ActiveEffects { get; private set; }

		public Light3 Illumination { get; set; }

		public Rectangle GetCollisionRect() => new Rectangle(TopLeft.ToPoint(), (BoundingBox*2).ToPoint());
        public void ClearActiveEffects() => ActiveEffects.Clear();

		public void AddEffect(StatusEffect effect)
		{

		}

		public Entity() {
			ActiveEffects = new List<StatusEffect>();
		}


		public virtual void Damage(DamageType type, IDamageSource source, int amount)
		{
			if (type == DamageType.ActOfGod)
            {
				Health -= amount;
				return;
            }


			amount = Math.Max(1, amount - Defense);

			Health -= amount;
		}

		public virtual void Damage(DamageType type, IDamageSource source, int amount, Vector2 direction)
        {
			Damage(type, source, amount);

        }

		public virtual void ClientUpdate(IGameClient client, GameTime gt) {
			DurationAlive += gt.GetDelta();

			Illumination = client.World.Lighting.GetLight(Position.ToTileCoords());
		}

		public virtual void ServerUpdate(IGameServer server, GameTime gt) {
			DurationAlive += gt.GetDelta();


			foreach (StatusEffect effect in ActiveEffects.ToArray())
			{
				effect.Duration -= gt.GetDelta();
				effect.Tick(gt);
			}
			ActiveEffects.RemoveAll(e => e.Duration < 0);
		}

		

		public virtual void Draw(GraphicsEngine gfx) { }

       
    }
}
