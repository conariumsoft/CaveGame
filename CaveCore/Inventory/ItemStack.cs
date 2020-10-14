using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Inventory
{
	public class ItemContainer
	{
		private ItemStack[,] slots;
		public ItemContainer(int x, int y)
		{
			slots = new ItemStack[x, y];
		}

	}


	public class Hotbar
	{

	}


	public struct ItemStack
	{
		public Item Item;
		public int Quantity { get; set; }
		public int MaxQuantity => Item.MaxStack;
		public int Room => MaxQuantity - Quantity;

		public bool AreCombinable(ItemStack other)
		{
			if (Item.ID == other.Item.ID)
			{
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
	}
}
