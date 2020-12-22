using CaveGame.Core.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
	public interface IGameClient
	{
		Camera2D Camera { get; }
		void Send(Packet p);
		IClientWorld World { get; }

	}
}
