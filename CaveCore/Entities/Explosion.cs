using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Entities
{
	public class Explosion : Entity, IPositional, IServerUpdate, IClientUpdate
	{
		public Vector2 Position { get; set; }

		private bool detonated;

		public Explosion(Vector2 position) : base()
		{
			detonated = false;
		}

		public void Detonate(IGameWorld world)
		{
			// TODO: destroy nearby tiles & damage entities
			detonated = true;
			Dead = true;
		}

		protected void ExplosionClientEffects(IGameWorld world)
		{
			// TODO: spawn particles and play explosion sound
			detonated = true;
			Dead = true;
		}

		public void ServerUpdate(IGameWorld world, GameTime gt)
		{
			if (!detonated)
				Detonate(world);
		}

		public void ClientUpdate(IGameWorld world, GameTime gt)
		{
			if (!detonated)
				ExplosionClientEffects(world);
		}

		public override void Update(IGameWorld world, GameTime gt)
		{
			base.Update(world, gt);
		}
	}
}
