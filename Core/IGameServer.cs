using CaveGame.Core.Game.Entities;
using CaveGame.Core.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CaveGame.Core
{
	public interface IGameServer
	{
		void SendTo(Packet p, User user);
		void SendToAll(Packet p);
		void SendToAllExcept(Packet p, User exclusion);
		User GetConnectedUser(IPEndPoint ep);
		void OutputAndChat(string text);

		void SpawnEntity(IEntity entity);
	}
}
