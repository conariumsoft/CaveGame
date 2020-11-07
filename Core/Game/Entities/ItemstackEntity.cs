using CaveGame.Core.Inventory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Entities
{
	public class ItemstackEntity : PhysicsEntity
	{
		public override float Mass => 1;

		public override Vector2 BoundingBox => new Vector2(4, 4);

		public override Vector2 Friction => new Vector2(0.95f, 0.95f);


		public ItemStack ItemStack;


		public override void Draw(SpriteBatch sb)
		{
			base.Draw(sb);
		}
	}
}
