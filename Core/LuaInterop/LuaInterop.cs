using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CaveGame.Core.LuaInterop
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

";
    }
	public static class LuaExtensions
	{

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


		public static LuaTable ArrayToTable<T>(this Lua state, T[] list)
		{
			LuaTable table = state.GetEmptyTable(); ;
			for (int i = 0; i < list.Length; i++)
				table[i + 1] = list[i];
			return table;
		}

		public static LuaTable ListToTable<T>(this Lua state, List<T> list)
        {
			LuaTable table = state.GetEmptyTable();
			for (int i = 0; i < list.Count; i++)
				table[i + 1] = list[i];
			return table;
		}
	}
}
