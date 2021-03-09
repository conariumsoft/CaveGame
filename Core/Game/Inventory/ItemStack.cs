using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using CaveGame.Core.Game.Items;

namespace CaveGame.Core.Inventory
{
	
	

	public class Hotbar
	{

	}


	public class ItemStack: IEquatable<ItemStack>
	{
		public ItemStack()
		{

		}
		public ItemStack(Item item, int quantity = 1)
		{
			this.Item = item;
			this.quantity = quantity;
		}

		public static ItemStack Of<T>(int quantity = 1) where T: Item, new()
		{
			ItemStack stack = new ItemStack { Quantity = quantity, Item = new T() };
			return stack;
		}

		public static ItemStack Empty = new ItemStack { Quantity = 0 };

		public Item Item;

		private int quantity;

		public int Quantity {
			get => quantity;
			set
            {
				quantity = value;
				if (quantity < 1)
                {
					Item = null;
                }
            }
		}
		public int MaxQuantity => Item.MaxStack;
		public int Room => MaxQuantity - Quantity;

		public bool AreCombinable(ItemStack other)
		{
			if (Item == null && other.Item == null)
				return true;
			if (Item == null || other.Item == null)
				return false;

			if (Item.GetType() == other.Item.GetType())
			{
				if (Item is TileItem tile && other.Item is TileItem otherTile && tile.Tile.ID!= otherTile.Tile.ID)
					return false;
				if (Item is WallItem wall && other.Item is WallItem otherwall && wall.Wall.ID != otherwall.Wall.ID)
					return false;


				return true;
			}
			return false;
		}

		public void TryCombine(ItemStack other)
		{
			int got = other.TryTake(Room);
			Add(got);
		}

		private void Add(int amount)
		{
			Quantity += amount;
		} 

		private void Take(int amount)
		{
			Quantity -= amount;
		}

		public int TryAdd(int amount)
		{
			int added = Math.Min(Room, amount);
			Quantity += added;
			return added;
		}
		public int TryTake(int amount)
		{
			int taken = Math.Max(amount, Quantity);
			Quantity -= taken;
			return taken;
		}

		public bool Equals(ItemStack o) {
			if (o == null)
				return (this == ItemStack.Empty);


			if (o.Item == null && Item == null)
				return true;
			if (o.Item == null || Item == null)
				return false;

			return (o.Item.Name == Item.Name && o.Quantity == Quantity);
				
		}

		//public static bool operator ==(ItemStack a, ItemStack b) => a.Equals(b);
		//public static bool operator !=(ItemStack a, ItemStack b) => !a.Equals(b);


        public override string ToString()
        {
            return String.Format("{0}(Item:{1}, Quantity:{2})",base.ToString(), Item, Quantity);
        }
    }
}
