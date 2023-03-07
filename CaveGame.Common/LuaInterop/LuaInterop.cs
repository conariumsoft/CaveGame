using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace CaveGame.Common.LuaInterop
{

	public static class LuaSnippets
    {
		public const string UtilityFunctions = @"
-- TODO: Document this
function list(clrlist)
	local it = clrlist:GetEnumerator()
    return function ()
		local has = it:MoveNext()
		if has then
			return it.Current
        end
    end
end
_G.list = list
";
    }
	public static class LuaExtensions
	{

		public static void InitFromLuaPropertyTable(this object thing, Lua environment, LuaTable table)
		{
			foreach (KeyValuePair<object, object> kvp in environment.GetTableDict(table))
			{
				if (kvp.Key is string keyString)
				{
					var prop = thing.GetType().GetProperty(keyString);
					if (prop != null)
					{
#if AUTOCASTING_DEBUG
						Debug.WriteLine("PropertySet {0} to {1} on {2}", keyString, kvp.Value.ToString(), thing.ToString());
#endif
						prop.SetValue(thing, Cast(prop.PropertyType, kvp.Value));
					}
				}
			}
		}


		public static object Cast(this Type Type, object data)
		{
			var DataParam = Expression.Parameter(typeof(object), "data");
			var Body = Expression.Block(Expression.Convert(Expression.Convert(DataParam, data.GetType()), Type));

			var Run = Expression.Lambda(Body, DataParam).Compile();
			var ret = Run.DynamicInvoke(data);
			return ret;
		}

		public static bool LoadCoreScript(string fname, out string data)
        {
			data = "";
			if (File.Exists(Path.Combine("Scripts", fname))) {
				data = File.ReadAllText(Path.Combine("Scripts", fname));
				return true;
			}
			return false;
        }

		public static LuaTable GetEmptyTable(this Lua state) => (LuaTable)state.DoString("return {}")[0];



	}
}
