using System;
using System.Collections.Generic;
using CaveGame.Core;
using CaveGame.Core.Entities;
using CaveGame.Core.FileUtil;
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

	public class PluginManager
	{
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

		#endregion
		public Plugin(PluginDefinition def, IPluginAPIServer server, string contents)
		{
			pluginDef = def;
			Lua state = new Lua();
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



-- plugin callback defaults;
function OnPluginLoaded() end
function OnPlayerJoined(player) end
function OnChatMessage(chatEvent) end
function OnPlayerPlaceTile(player, tile, x, y) end
function OnServerShutdown() end

server.Output:Out(plugin.PluginName..' Loaded');
");
			state.DoString(contents);
			//state.DoFile("Plugins/" + PluginFolder + "/main.lua");
			OnPluginLoaded = state["OnPluginLoaded"] as LuaFunction;
			OnPlayerJoined = state["OnPlayerJoined"] as LuaFunction;
			OnChatMessage = state["OnChatMessage"] as LuaFunction;
			OnPlayerPlaceTile = state["OnPlayerPlaceTile"] as LuaFunction;
			OnServerShutdown = state["OnServerShutdown"] as LuaFunction;
		}
	}
}
