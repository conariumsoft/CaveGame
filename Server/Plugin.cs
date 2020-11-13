using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CaveGame.Core;
using CaveGame.Core.FileUtil;
using CaveGame.Core.Game.Entities;
using CaveGame.Core.LuaInterop;
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using NLua;
using NLua.Exceptions;

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
		public void LoadPlugins(IPluginAPIServer server)
        {
			if (!System.IO.Directory.Exists("Plugins"))
				System.IO.Directory.CreateDirectory("Plugins");


			foreach (string folder in System.IO.Directory.EnumerateDirectories("Plugins"))
			{
				var pluginDefinition = Configuration.Load<PluginDefinition>(Path.Combine(folder, "plugin.xml"));

				var plug = new Plugin(pluginDefinition, server, System.IO.File.ReadAllText(Path.Combine(folder, "main.lua")));
				Plugins.Add(plug);
				plug.OnPluginLoad.Invoke(null);
			}
		}

		public void UnloadPlugins()
        {
			foreach (Plugin plugin in Plugins) {
				plugin.OnPluginUnload.Invoke(null);
				plugin.Dispose();
			}
        }
	}


	public class Plugin : IDisposable
	{

		public LuaEvent<LuaEventArgs> OnPluginLoad = new LuaEvent<LuaEventArgs>();
		public LuaEvent<LuaEventArgs> OnPluginUnload = new LuaEvent<LuaEventArgs>();

		protected Lua PluginLuaEnvironment;

		private PluginDefinition pluginDef;

		public bool FailedToLoad { get; private set; }

		public string PluginName => pluginDef.Name;
		public string PluginAuthor => pluginDef.Author;
		public string PluginDescription => pluginDef.Description;
		public string PluginFolder => pluginDef.Folder;
		public string PluginVersion => pluginDef.Version;


		public void Dispose()
        {
			Dispose(true);
			GC.SuppressFinalize(this);
        }

		~Plugin() => Dispose(false);

		protected virtual void Dispose(bool disposing)
        {
			if (disposing)
            {
				PluginLuaEnvironment.Dispose();
			}
        }

		public Plugin(PluginDefinition def, IPluginAPIServer server, string contents)
		{
			pluginDef = def;
			PluginLuaEnvironment = new Lua();
			PluginLuaEnvironment.LoadCLRPackage();

			#region Defined Properties

			PluginLuaEnvironment["plugin"] = this;
			PluginLuaEnvironment["server"] = server;

			#endregion

			PluginLuaEnvironment.DoString(
@"import ('MonoGame.Framework', 'Microsoft.Xna.Framework')


-- functions

function print(v)
	local date = server.Time;
	server.Output:Out('['..plugin.PluginName..' '..date:ToString('HH:mm:ss.ff')..'] '..v);
end
"+LuaSnippets.UtilityFunctions + @"

print(plugin.PluginName..' Loaded');

");

			try {
				PluginLuaEnvironment.DoString(contents);
			} catch(LuaScriptException e)
            {
				server.Output.Out(String.Format("Plugin Error! {0} Failed to load:", this.PluginName), Color.Red);
				server.Output.Out(String.Format("Error Message: {0} ", e.Message), Color.Red);
				server.Output.Out(String.Format("Stack trace: {0} ", e.StackTrace), Color.Red);
				this.FailedToLoad = true;
            }
			
		}
	}
}
