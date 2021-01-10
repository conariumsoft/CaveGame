using CaveGame.Core.Game.Items;
using CaveGame.Core.Inventory;
using NLua;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.CraftingRecipes
{

	public class RecipeComponent
    {
		public string ItemTag { get; set; }
		public int Quantity { get; set; }
		public RecipeComponent(string tag, int quantity)
        {
			ItemTag = tag;
			Quantity = quantity;
        }
    }

	public class Recipe
	{
		public static List<Recipe> RecipeList { get; private set; }

		public static Recipe ParseLuaRecipeData(Lua VM, LuaTable RecipeData)
		{
			var dict = VM.GetTableDict(RecipeData);
			var stationString = dict["station"] as string;
			var ingredients = dict["ingredients"] as LuaTable;
			var products = dict["products"] as LuaTable;

			Recipe recipe = new Recipe();

			foreach(KeyValuePair<object, object> kvp in VM.GetTableDict(ingredients))
            {
				var token = kvp.Value;
				if (token is LuaTable tokentable)
                {
					string itemtag = tokentable[1] as string;
					int quantity = 1;
					if (tokentable[2] != null)
						quantity = (int)tokentable[2];

					recipe.Ingredients.Add(new RecipeComponent(itemtag, quantity));
                }
            }
			foreach (KeyValuePair<object, object> kvp in VM.GetTableDict(products))
			{
				var token = kvp.Value;
				if (token is LuaTable tokentable)
				{
					string itemtag = tokentable[1] as string;
					int quantity = 1;
					if (tokentable[2] != null)
						quantity = (int)tokentable[2];

					recipe.Ingredients.Add(new RecipeComponent(itemtag, quantity));
				}
			}
			return recipe;

		}

		public List<RecipeComponent> Ingredients { get; set; }
		public List<RecipeComponent> Products { get; set; }



		public Recipe()
        {

        }

	}
}
