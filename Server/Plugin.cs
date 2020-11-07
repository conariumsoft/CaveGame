using System;
using System.Collections.Generic;
using System.Linq;
using CaveGame.Core;
using CaveGame.Core.FileUtil;
using CaveGame.Core.Game.Entities;
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using NLua;
namespace CaveGame.Server
{

	public class ChatMessageEvent
	{
		public bool Cancelled { get; set; }
		public string Message { get; set; }
		public Player Sender { get; set; }
	}

	[Serializable]
	public class PluginDefinition : Configuration {

		public string Name;
		public string Author;
		public string Description;
		public string Folder;
		public string Version;
	}

	public static class LuaExtensions
	{

		public static LuaTable ListToTable<T>(this Lua state, T[] list)
		{
			LuaTable table = (LuaTable)state.DoString("return {}")[0];
			for (int i = 0; i < list.Length; i++)
			{

				table[i+1] = list[i];
			}

			return table;
		}
	}

	public class PluginManager
	{

		private LuaTable MarshalDictionaryToTable<A, B>(Dictionary<A, B> dict)
		{
			using (Lua state = new Lua())
			{
				LuaTable table = (LuaTable)state.DoString("return{}")[0];
				foreach (KeyValuePair<A, B> kv in dict)
					table[kv.Key] = kv.Value;
				return table;
			}
				
		}

		public List<Plugin> Plugins;

		public PluginManager()
		{

			Plugins = new List<Plugin>();
		}

		public void CallOnPluginLoaded()
		{
			foreach (Plugin plugin in Plugins)
				plugin.OnPluginLoaded?.Call();
		}

		public void CallOnServerShutdown()
		{
			foreach(Plugin plugin in Plugins)
				plugin.OnServerShutdown?.Call();
		}

		public void CallOnPlayerJoined(Player player)
		{
			foreach (Plugin plugin in Plugins)
				plugin.OnPlayerJoined?.Call(player);
		}

		public void CallOnPlayerPlaceTile(Player player, Tile tile, int x, int y)
		{
			foreach (Plugin plugin in Plugins)
				plugin.OnPlayerPlaceTile?.Call();
		}

		public bool CallOnChatMessage(Player player, string message)
		{
			ChatMessageEvent ev = new ChatMessageEvent
			{
				Cancelled = false,
				Sender = player,
				Message = message,
			};
			foreach (Plugin plugin in Plugins)
			{
				plugin.OnChatMessage?.Call(ev);
				if (ev.Cancelled)
				{
					return false;
				}
			}
			return true;
		}

		public void CallOnCommand(string command, List<string> args)
		{
			foreach (Plugin plugin in Plugins)
			{
				plugin.OnCommand?.Call(command, plugin.LuaState.ListToTable(args.ToArray()));
			}
				
		}
	}


	public class Plugin
	{
		private PluginDefinition pluginDef;

		public string PluginName => pluginDef.Name;
		public string PluginAuthor => pluginDef.Author;
		public string PluginDescription => pluginDef.Description;
		public string PluginFolder => pluginDef.Folder;
		public string PluginVersion => pluginDef.Version;

		#region LuaFunctions
		// these are defined as overwritten callbacks
		public LuaFunction OnPluginLoaded;
		public LuaFunction OnPlayerJoined;
		public LuaFunction OnChatMessage;
		public LuaFunction OnPlayerPlaceTile;
		public LuaFunction OnServerShutdown;
		public LuaFunction OnCommand;

		public Lua LuaState;

		#endregion
		public Plugin(PluginDefinition def, IPluginAPIServer server, string contents)
		{
			pluginDef = def;
			Lua state = new Lua();
			LuaState = state;
			state.LoadCLRPackage();

			#region Defined Properties

			state["plugin"] = this;
			state["server"] = server;

			#endregion

			state.DoString(
@"import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
-- functions

function print(v)
	local date = server.Time;
	server.Output:Out('['..plugin.PluginName..' '..date:ToString('HH:mm:ss.ff')..'] '..v);
end


function list(clrlist)
	local it = clrlist:GetEnumerator()
    return function ()
		local has = it:MoveNext()
		if has then
			return it.Current
        end
    end
end


-- plugin callback defaults;
function OnPluginLoaded() end
function OnPlayerJoined(player) end
function OnChatMessage(chatEvent) end
function OnPlayerPlaceTile(player, tile, x, y) end
function OnServerShutdown() end
function OnCommand(command, args) end

server.Output:Out(plugin.PluginName..' Loaded');


--server:BindCommand('tim', 'jim', {'a'});

");
			state.DoString(contents);
			//state.DoFile("Plugins/" + PluginFolder + "/main.lua");
			OnPluginLoaded = state["OnPluginLoaded"] as LuaFunction;
			OnPlayerJoined = state["OnPlayerJoined"] as LuaFunction;
			OnChatMessage = state["OnChatMessage"] as LuaFunction;
			OnPlayerPlaceTile = state["OnPlayerPlaceTile"] as LuaFunction;
			OnServerShutdown = state["OnServerShutdown"] as LuaFunction;
			OnCommand = state["OnCommand"] as LuaFunction;
		}
	}
}
