#if CLIENT
using CaveGame.Client;
#endif
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CaveGame.Core.Game.Entities
{
	public class Player : PhysicsEntity, IPhysicsObject, IPositional, IVelocity, INextPosition, IHorizontalDirectionState
	{

		public override Vector2 BoundingBox => new Vector2(6, 12);
		public override float Mass => 1.5f;
		public override Vector2 Friction => new Vector2(5.0f, 1f);
		public override int MaxHealth => 100;

		public bool God { get; set; }
		public bool OnRope { get; set; }
		public User User { get; set; }
		public string DisplayName { get; set; }
		public HorizontalDirection Facing { get; set; }
		public Color Color { get; set; }
		public bool Walking { get; set; }


		protected float walkingAnimationTimer;

		public Player()
		{
			Health = MaxHealth;
			God = false;
			Position = new Vector2(0, -200);
			Velocity = Vector2.Zero;
			OnGround = false;
			Color = Color.White;
		}

		public override void ClientUpdate(IClientWorld world, GameTime gt)
		{
			walkingAnimationTimer += (float)gt.ElapsedGameTime.TotalSeconds * 5;
			base.ClientUpdate(world, gt);
		}
	}
}
