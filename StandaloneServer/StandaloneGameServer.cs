using CaveGame.Core.Game.Entities;
using CaveGame.Core.LuaInterop;
using CaveGame.Core.Network;
using CaveGame.Server;
using Microsoft.Xna.Framework;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CaveGame.Core;
using CaveGame.Core.Network.Packets;

namespace StandaloneServer
{

	public class ServerCommand
	{
		public delegate void SCommandHandler(ServerCommand command, params string[] args);
		public string Keyword;
		public string Description;
		public List<string> Args;

		public ServerCommand(string cmd, string desc, List<string> args)
		{
			Keyword = cmd;
			Description = desc;
			Args = args;
		}

	}

	public class CommandEventArgs : LuaEventArgs
	{

		public Player Sender { get; set; }
		public string Command { get; private set; }
		public List<string> Arguments { get; private set; }

		public CommandEventArgs(string cmd, List<string> args)
		{
			Command = cmd;
			Arguments = args;
		}
		public CommandEventArgs(string cmd, List<string> args, Player player)
		{
			Command = cmd;
			Arguments = args;
			Sender = player;
		}
	}

	public class StandaloneGameServer: GameServer, IPluginAPIServer
	{
        #region Lua Events
        public LuaEvent<PlayerEventArgs> OnPlayerJoinedServer = new LuaEvent<PlayerEventArgs>();
		public LuaEvent<PlayerEventArgs> OnPlayerLeftServer = new LuaEvent<PlayerEventArgs>();
		public LuaEvent<PlayerChatMessageEventArgs> OnChatMessageFromClient = new LuaEvent<PlayerChatMessageEventArgs>();
		public LuaEvent<CommandEventArgs> OnServerCommand = new LuaEvent<CommandEventArgs>();

		#endregion

		public List<ServerCommand> Commands { get; set; }
		public void BindCommand(ServerCommand command)
		{
			Commands.Add(command);
		}
		public void BindCommand(string cmd, string descr, LuaTable args)
		{
			var argsList = new List<string>();

			foreach (var arg in args.Values)
			{
				argsList.Add((string)arg);
			}

			ServerCommand command = new ServerCommand(cmd, descr, argsList);
			Commands.Add(command);
		}

		public PluginManager PluginManager;
		public string Information = "";

		public void OnCommand(string msg)
		{
			Output.Out("> " + msg);

			string cleaned = msg.Trim();

			string[] keywords = cleaned.Split(' ');

			foreach (ServerCommand cmdDef in Commands)
			{
				if (keywords[0] == cmdDef.Keyword)
				{
					//cmdDef.InvokeCommand(keywords.Skip(1).ToArray());
					OnServerCommand.Invoke(new CommandEventArgs(cmdDef.Keyword, keywords.Skip(1).ToList()));
					return;
				}
			}
			Output.Out("No command " + keywords[0] + " found!", new Color(1.0f, 0, 0));
		}

		public StandaloneGameServer(ServerConfig config, WorldMetadata worldMDT) : base(config, worldMDT) 
		{
			Commands = new List<ServerCommand>();
			PluginManager = new PluginManager();
		}


        public override void Update(GameTime gt)
        {
			Information = String.Format("{0} Players", ConnectedUsers.Count);
			base.Update(gt);
        }

        public void LoadPlugins() => PluginManager.LoadPlugins(this);

        public override void Shutdown()
        {
			PluginManager.UnloadPlugins();
			base.Shutdown();
        }

        protected override void OnClientQuit(NetworkMessage msg, User user)
        {
			DisconnectPacket packet = new DisconnectPacket(msg.Packet.GetBytes());

			if (World.FindEntityOfID(packet.LeavingEntityID, out Player player))
				if (OnPlayerLeftServer.Invoke(new PlayerEventArgs(player)))
					OutputAndChat(String.Format("{0} has left the server.", user.Username));
			base.OnClientQuit(msg, user);
        }

        protected override void OnClientChat(NetworkMessage msg, User user)
        {
			ClientChatMessagePacket chatMessagePacket = new ClientChatMessagePacket(msg.Packet.GetBytes());
			if (OnChatMessageFromClient.Invoke(new PlayerChatMessageEventArgs(user.PlayerEntity, chatMessagePacket.Message)))
			{
				base.OnClientChat(msg, user);
			}
        }

        protected override void OnPlayerConnects(User newuser, Player player)
        {
			if (OnPlayerJoinedServer.Invoke(new PlayerEventArgs(player)))
				base.OnPlayerConnects(newuser, player);
		}
    }
}
