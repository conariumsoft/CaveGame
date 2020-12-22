using CaveGame.Core.Game.Entities;
using CaveGame.Core.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CaveGame.Core
{
	public interface IGameServer
	{
		Entity GetEntity(int networkID);
		void SendTo(Packet p, User user);
		void SendToAll(Packet p);
		void SendToAllExcept(Packet p, User exclusion);
		User GetConnectedUser(IPEndPoint ep);
		void OutputAndChat(string text);
		void Chat(string text);
		void Chat(string text, Color color);
		IServerWorld World { get; }
		void SpawnEntity(IEntity entity);
		void Update(GameTime gt);
		int TickRate { get; }
		int MaxPlayers { get; }
		IEntityManager EntityManager { get; }

	}
}
