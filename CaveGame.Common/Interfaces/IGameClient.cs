using CaveGame.Common.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common
{
	public interface IGameClient
	{
		Camera2D Camera { get; }
		void Send(Packet p);
		IClientWorld World { get; }

	}
}
