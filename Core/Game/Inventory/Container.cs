using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Inventory
{
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

}
