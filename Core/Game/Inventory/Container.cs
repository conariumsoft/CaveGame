using System;
using System.Collections.Generic;
using System.Text;
using CaveGame.Core.FileUtil;
using CaveGame.Core.Game.Items;
using CaveGame.Core.Inventory;
using DataManagement;

namespace CaveGame.Core.Game.Inventory
{
	public class Container
	{
		private ItemStack[,] slots;

		public ItemStack this[int x, int y]
        {
			get => slots[x, y];
			set => slots[x, y] = value;
        }

		public int Width { get; private set; }
		public int Height { get; private set; }

		public Container(int width, int height)
		{
			Width = width;
			Height = height;
			slots = new ItemStack[Width, Height];
			for (int x = 0; x < Width; x++)
            {
				for (int y = 0; y < Height; y++)
                {
					slots[x, y] = ItemStack.Empty;
                }
            }
		}


		public bool ItemWhitelistEnabled { get; set; }
		public List<ItemTag> ItemTagWhitelist { get; set; }

		public void ForceSetSlot(int x, int y, ItemStack stack)
        {
			slots[x, y] = stack;
        }



		public bool IsItemAllowed(Item search)
        {
			if (ItemWhitelistEnabled)
            {
				foreach (ItemTag tag in ItemTagWhitelist) {
					if (search.Tags.Contains(tag))
						return true;
				}
				return false;
            }
			return true;
        }

		public ItemStack GetItemStack(int x, int y)
        {
			//if (slots[x, y] == null)
				//return ItemStack.Empty;
			return slots[x, y];
        }

		public bool IsSlotEmpty(int x, int y)
        {
			return slots[x, y].Equals(ItemStack.Empty);
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

		public bool HasSpaceFor(ItemStack stack)
        {
			return false; // TODO: makle this work
        }

		public bool AddItem(ItemStack stack)
		{
			if (!IsItemAllowed(stack.Item))
				return false;

			for (int x = 0; x < Width; x++)
            {
				for (int y = 0; y < Height; y++)
                {
					if (IsSlotEmpty(x, y))
                    {
						ForceSetSlot(x, y, stack);
						return true;
                    }
					if (slots[x, y].AreCombinable(stack))
                    {
						slots[x, y].Quantity += stack.Quantity;
						return true;
                    }
                }
            }

			return false;
		}

		public List<ItemStack> GetItems() => slots.ToList();

		public Metabinary ToMetabinary()
        {
			Metabinary mb = new Metabinary { Name = "container" };
			mb.AddInt("width", Width);
			mb.AddInt("height", Height);
			ComplexTag ItemList = new ComplexTag { Name = "items" };
			int width = slots.GetLength(0);
			int height = slots.GetLength(1);
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					ItemStack stack = slots[i, j];
					var itemstacktag = new ComplexTag { Name = "itemstack" };
					itemstacktag.AddInt("quantity", stack.Quantity);
					itemstacktag.AddInt("slotx", i);
					itemstacktag.AddInt("sloty", j);
					itemstacktag.AddComplex((ComplexTag)stack.Item.GetMetadataComplex());
					ItemList.AddComplex(itemstacktag);
				}
			}
			mb.AddComplex(ItemList);
			return mb;
        }

		public static Container FromMetabinary(Metabinary data)
        {
			int width = data.GetValue<int>("width");
			int height = data.GetValue<int>("height");
			Container container = new Container(width, height);

			//container
			foreach(ComplexTag itemtag in data.GetValue<ComplexTag>("items").Payload)
            {
				ItemStack stack = new ItemStack();
				stack.Item = Item.FromMetadataComplex(itemtag.GetValue<ComplexTag>("item"));
				stack.Quantity = itemtag.GetValue<int>("quantity");
				int x = itemtag.GetValue<int>("slotx");
				int y = itemtag.GetValue<int>("sloty");
				container[x, y] = stack;
            }

			return container;
        }
	}
}
