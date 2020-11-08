using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CaveGame.Core.Inventory
{
	public class RecipeResult<T> where T: Item, new()
	{
		public int Count;

		public ItemStack Retrieve() {
			return new ItemStack
			{
				Item = new T(),
				Quantity = Count
			};
		}
	}

	public class Recipe
	{

	}


	public class Container
	{
		private ItemStack[,] slots;

		public int Width { get; private set; }
		public int Height { get; private set; }

		public Container(int width, int height)
		{
			Width = width;
			Height = height;
			slots = new ItemStack[Width, Height];
		}

		public bool HasItem(Item search, int quantity = 1)
		{
			for (int x = 0; x < Width; x++)
				for (int y = 0; y < Height; y++)
					if (slots[x, y].Item.GetType().Equals(search.GetType()))
						if (slots[x, y].Quantity >= quantity)
							return true;
			return false;
		}


		public bool AddItem()
		{
			return false;
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
