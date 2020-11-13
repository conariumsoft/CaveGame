using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.CraftingRecipes
{
	public class RecipeResult<T> where T : Item, new()
	{
		public int Count;

		public ItemStack Retrieve()
		{
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
}
