using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Entities
{
	public interface IPositional
	{
		Vector2 Position { get; set; }
	}

	public interface IVelocity
	{
		Vector2 Velocity { get; set; }
	}

	public interface INextPosition
	{
		Vector2 NextPosition { get;set; }
	}

	public interface IPhysicsObject
	{
		void PhysicsStep(IGameWorld world, float step);
	}

	public interface IEntity
	{
		void Update(IGameWorld world, GameTime gt);
		void ServerUpdate(IGameWorld world, GameTime gt);
		void ClientUpdate(IGameWorld world, GameTime gt);
		int EntityNetworkID { get; set; }
	}

	public class Entity: IEntity
	{
		public int EntityNetworkID { get; set; }


		public Entity()
		{
			//EntityNetworkID = this.GetHashCode();
		}

		public virtual void Update(IGameWorld world, GameTime gt) { }
		public virtual void ServerUpdate(IGameWorld world, GameTime gt) { }
		public virtual void ClientUpdate(IGameWorld world, GameTime gt) { }
	}

	public class Thinker : Entity
	{

	}
}
