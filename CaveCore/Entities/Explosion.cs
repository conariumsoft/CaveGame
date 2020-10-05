using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Entities
{
	public class Explosion : Entity, IPositional
	{
		public Vector2 Position { get; set; }

		private bool detonated;

		public Explosion(Vector2 position) : base()
		{
			detonated = false;
		}

		public override void ClientUpdate(IGameWorld world, GameTime gt)
		{
			detonated = true;
			base.Update(world, gt);
		}

		public override void ServerUpdate(IGameWorld world, GameTime gt)
		{
			detonated = true;
			base.Update(world, gt);
		}
	}
}
