using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Inventory
{
	public class Item
	{
		public Rectangle quad;


		public Item()
		{

		}

#if CLIENT
		public virtual void Draw(SpriteBatch sb)
		{

		}
#endif
	}



	public class TileItem : Item
	{

		public TileItem()
		{

		}



#if CLIENT
		public override void Draw(SpriteBatch sb)
		{

		}
#endif
	}

	public class EquipmentItem : Item
	{

	}

	public class ArmourItem : Item
	{

	}

	public struct ItemStack
	{
		Item Item;
		int Quantity { get; set; }

	}
}
