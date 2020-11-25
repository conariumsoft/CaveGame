﻿using CaveGame.Core.Game.StatusEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Entities
{

	public interface IReplicatedProperty { }
	public interface IPositional: IReplicatedProperty { 
		Vector2 Position { get; set; }
	}
	public interface IVelocity : IReplicatedProperty
	{
		Vector2 Velocity { get; set; }
	}
	public interface INextPosition : IReplicatedProperty
	{
		Vector2 NextPosition { get;set; }
	}
	public interface IBoundingBox { 
		Vector2 BoundingBox { get; }
	}
	public interface IHorizontalDirectionState : IReplicatedProperty {
		HorizontalDirection Facing { get; set; }
	}
	public interface IPhysicsObject {
		//void PhysicsStep(IGameWorld world, float step);
		//void OnCollide(IGameWorld world, Tiles.Tile t, Vector2 separation, Vector2 normal, Point tilePos);
		float Mass { get; }

	}
	public interface IFriction
	{
		Vector2 Friction { get; }
	}
	public interface IClientLogical {
		void ClientUpdate(IClientWorld world, GameTime gt);
	}
	public interface IServerLogical {
		void ServerUpdate(IServerWorld world, GameTime gt);
	}
	public interface IEntity
	{
		int EntityNetworkID { get; set; }
		float DurationAlive { get; }
		bool Dead { get; }
		//void Update(IGameWorld world, GameTime gt);
		void ClientUpdate(IClientWorld world, GameTime gt);
		void ServerUpdate(IServerWorld world, GameTime gt);
	}
	public interface IExpirationTime{
		float ExpirationTicks { get; set; }
	}

	public class Entity: IEntity, IServerLogical, IClientLogical
	{
		public float DurationAlive { get; set; }
		public bool Dead { get; set; } // Gets collected on death
		public bool RemoteControlled { get; set; }
		public int EntityNetworkID { get; set; }
		public int Health { get; set; }
		public virtual int MaxHealth { get; }

		public List<StatusEffect> ActiveEffects { get; private set; }

		public void ClearActiveEffects()
		{
			ActiveEffects.Clear();
		}
		public void AddEffect(StatusEffect effect)
		{

		}

		public Entity() {
			ActiveEffects = new List<StatusEffect>();
		}


		public virtual void ClientUpdate(IClientWorld world, GameTime gt) {
			DurationAlive += gt.GetDelta();
		}

		public virtual void ServerUpdate(IServerWorld world, GameTime gt) {
			DurationAlive += gt.GetDelta();


			foreach (StatusEffect effect in ActiveEffects.ToArray())
			{
				effect.Duration -= gt.GetDelta();
				effect.Tick(gt);
			}
			ActiveEffects.RemoveAll(e => e.Duration < 0);
		}
		public virtual void Draw(SpriteBatch sb) { }
	}
}
