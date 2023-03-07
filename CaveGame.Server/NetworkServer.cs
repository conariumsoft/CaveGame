//#define PACKETDEBUG

using CaveGame.Common;
using CaveGame.Common.Generic;
using CaveGame.Common.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CaveGame.Server
{

	public class NetworkServer : SharedNetworkSubsystem
	{
		public override string DeviceName => "NetworkServer";


		public NetworkServer(int port) : base()
		{
			IncomingMessages = new ConcurrentQueue<NetworkMessage>();
			OutgoingMessages = new ConcurrentQueue<OutgoingPayload>();
			running = new ThreadSafeValue<bool>(false);
			UdpSocket = new UdpClient(port, AddressFamily.InterNetwork);

		}

		public void SendPacket(Packet packet, IPEndPoint target) {
			Send(packet, target);
		}

	}
}
