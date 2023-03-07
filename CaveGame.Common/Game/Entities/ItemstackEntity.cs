// The fuck?
#if CLIENT
using CaveGame.Client;
#endif
using CaveGame.Common.Game.Inventory;
using CaveGame.Common.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CaveGame.Common.Extensions;
using CaveGame.Common.Game.Inventory;

namespace CaveGame.Common.Game.Entities
{
	// client-relevant code
	[Summonable]
	public partial class ItemstackEntity : PhysicsEntity, IClientPhysicsObserver
	{
		public override float Mass => 0.3f;
		public override Vector2 BoundingBox => new Vector2(4, 4);
		public override Vector2 Friction => new Vector2(5.0f, 0.5f);

		public ItemStack ItemStack;

		private float hover = 0;

		public void ClientPhysicsTick(IClientWorld world, float step) => PhysicsStep(world, step);


		public override void ClientUpdate(IGameClient client, GameTime gt)
        {
			hover += gt.GetDelta();
			base.ClientUpdate(client, gt);
        }

        public override void Draw(GraphicsEngine GFX)
		{

			var drawPos = TopLeft + new Vector2(0, (float)Math.Sin((double)hover * 1.5) * 1.0f);

			if (ItemStack.Quantity > 0 && !Dead)
            {

				if (ItemStack.Quantity > 10)
				{
					ItemStack.Item.Draw(GFX, drawPos + new Vector2(-2f, -2f), 0.5f);
				}
				if (ItemStack.Quantity > 1)
				{
					ItemStack.Item.Draw(GFX, drawPos + new Vector2(-1f, -1f), 0.5f);
				}
				
				ItemStack.Item.Draw(GFX, drawPos, 0.5f);
			
				if (ItemStack.Quantity > 100)
				{
					ItemStack.Item.Draw(GFX, drawPos + new Vector2(1f, 1f), 0.5f);
				}
			}

			GFX.Text(ItemStack.Quantity.ToString(), drawPos - new Vector2(0, 10));
		}
	}




	// server-relevant code
	public partial class ItemstackEntity : PhysicsEntity, IServerPhysicsObserver
	{
		public void ServerPhysicsTick(IServerWorld world, float step)
		{

			foreach (var entity in world.Entities.ToArray())
			{

				if (entity is Player player && player.Position.Distance(Position) < 50)
				{
					float distance = player.Position.Distance(Position);
					Vector2 unitVec = player.Position - Position;
					unitVec.Normalize();
					Velocity += (unitVec * Math.Max(0.75f - (distance / 6), 0.05f));


					if (this.Dead == false && player.User != null && ItemStack.Quantity > 0 && player.Position.Distance(Position) < 10)
					{
						
						player.User.Send(new GivePlayerItemPacket(this.ItemStack));
						// TODO: Add Item to player Inventory
						this.Dead = true;
						return;
					}
				}


				if (entity is ItemstackEntity partner && entity != this)
				{
					if (partner.ItemStack.Quantity > 0 && ItemStack.Quantity > 0 && partner.ItemStack.AreCombinable(ItemStack) && partner.Position.Distance(Position) < 10)
					{
						int spaceLeft = Math.Min(ItemStack.Item.MaxStack - ItemStack.Quantity, partner.ItemStack.Quantity);

						if (spaceLeft > 0)
						{
							partner.ItemStack.Quantity -= spaceLeft;
							partner.Dead = true;
							ItemStack.Quantity += spaceLeft;
							return;
						}
					}
				}
			}

			base.PhysicsStep(world, step);
		}

		public override void ServerUpdate(IGameServer server, GameTime gt)
		{

			if (ItemStack.Quantity <= 0)
			{
				this.Dead = true;
				return;
			}

			base.ServerUpdate(server, gt);
		}
	}
}
