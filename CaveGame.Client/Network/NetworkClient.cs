using CaveGame.Common;
using CaveGame.Common.Generic;
using CaveGame.Common.Network;
using CaveGame.Common.Network.Packets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CaveGame.Client
{
	public class NetworkClient: SharedNetworkSubsystem
	{


		public override string DeviceName => "NetworkClient";

        public string ServerIPAddress { get; private set; }
		public int ServerPort { get; private set; }


		// parses shit like "localhost"
		public static string ParseHostnameShortcuts(string hostname)
        {
			hostname = hostname.Trim();
			if (hostname.Contains("localhost"))
				hostname = hostname.Replace("localhost", "127.0.0.1");

			if (!hostname.Contains(":"))
				hostname += ":"+Globals.DEFAULT_PORT.ToString();

			return hostname;
		}

		public static IPEndPoint GetEndPointFromAddress(string hostname)
        {
			bool success = IPEndPoint.TryParse(ParseHostnameShortcuts(hostname), out IPEndPoint output);
			if (success)
				return output;
			throw new Exception($"Invalid IP Endpoint! {hostname}, {ParseHostnameShortcuts(hostname)}");
		}

		public IPEndPoint ServerAddress { get; private set; }

		public NetworkClient(string hostname) : base()
		{
			running = new ThreadSafeValue<bool>(false);
			var endpoint = GetEndPointFromAddress(hostname);// = ParseHostnameShortcuts(hostname);

			ServerAddress = endpoint;

			IncomingMessages = new ConcurrentQueue<NetworkMessage>();
            OutgoingMessages = new ConcurrentQueue<OutgoingPayload>();
            ServerPort = endpoint.Port;
			ServerIPAddress = endpoint.Address.ToString();

			UdpSocket = new UdpClient(ServerIPAddress, ServerPort);

			
			//UdpSocket.Connect(endpoint);
			//IOControlFixICMPBullshit();
		}

		public void Logout(int userID, UserDisconnectReason reason)
        {
			SendPacket(new DisconnectPacket(userID, reason));
			Close();
		}

		public void SendPacket(Packet packet) => Send(packet);
    }
}
