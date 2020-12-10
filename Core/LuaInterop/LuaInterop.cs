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
_G.list = list
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



	}
}
