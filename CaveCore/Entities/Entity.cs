using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Entities
{
	public interface IPositional
	{
		public Vector2 Position { get; set; }
	}

	public interface IVelocity
	{
		public Vector2 Velocity { get; set; }
	}

	public interface INextPosition
	{
		public Vector2 NextPosition { get;set; }
	}

	public interface IPhysicsObject
	{
		public void PhysicsStep(IGameWorld world, float step);
	}

	public interface IEntity
	{
		public void Update(IGameWorld world, GameTime gt);
		public void ServerUpdate(IGameWorld world, GameTime gt);
		public void ClientUpdate(IGameWorld world, GameTime gt);
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
