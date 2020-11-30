#if CLIENT
using CaveGame.Client;
using CaveGame.Core.Game.Inventory;
using CaveGame.Core.Inventory;
#endif
using CaveGame.Core.Game.Inventory;
using CaveGame.Core.Inventory;
using CaveGame.Core.Network;
using CaveGame.Core.Game.Tiles;
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
        //public override float Buoyancy => 4.0f; 
        public override Vector2 Friction => new Vector2(5.0f, 1f);
		public override int MaxHealth => 100;

		public bool God { get; set; }
		public bool OnRope { get; set; }
		public User User { get; set; }
		public string DisplayName { get; set; }
		public HorizontalDirection Facing { get; set; }
		public Color Color { get; set; }
		public bool Walking { get; set; }

		public Container Inventory { get; set; }
		public Container ArmorSlots { get; set; }

		protected float walkingAnimationTimer;

		public Player()
		{

			Inventory = new Container(10, 4);
			ArmorSlots = new Container(1, 3);
			ArmorSlots.ItemWhitelistEnabled = true;
			ArmorSlots.ItemTagWhitelist = new List<ItemTag>{ ItemTag.Armor };
			Health = MaxHealth;
			God = false;
			Position = new Vector2(0, -200);
			Velocity = Vector2.Zero;
			OnGround = false;
			Color = Color.White;
		}

		public override void ClientUpdate(IGameClient client, GameTime gt)
		{
			walkingAnimationTimer += (float)gt.ElapsedGameTime.TotalSeconds * 5;
			base.ClientUpdate(client, gt);
		}
	}
}
