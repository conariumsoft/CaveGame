//#define PACKETDEBUG

using CaveGame.Core;
using CaveGame.Core.Generic;
using CaveGame.Core.Network;
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

		private ConcurrentQueue<NetworkMessage> incomingMessages;
		private ConcurrentQueue<Tuple<Packet, IPEndPoint>> outgoingMessages;

		private ThreadSafeValue<bool> listening = new ThreadSafeValue<bool>(false);

		public NetworkServer(int port)
		{

			UdpSocket = new UdpClient(port, AddressFamily.InterNetwork);
			IOControlFixICMPBullshit();

			Port = port;
			incomingMessages = new ConcurrentQueue<NetworkMessage>();
			outgoingMessages = new ConcurrentQueue<Tuple<Packet, IPEndPoint>>();

		}

		private void NetworkThread()
		{
			if (!listening.Value)
				return;

			while (listening.Value)
			{
				
				bool canRead = UdpSocket.Available > 0;

				// Read in message if we have any data
				if (canRead)
				{

					IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
					try
					{
						byte[] data = UdpSocket.Receive(ref ep);

						NetworkMessage nm = new NetworkMessage();
						nm.Sender = ep;
						nm.Packet = new Packet(data);
						nm.ReceiveTime = DateTime.Now;

						PacketsReceived++;
						InternalReceiveCount += nm.Packet.Payload.Length;
						TotalBytesReceived += nm.Packet.Payload.Length;
						incomingMessages.Enqueue(nm);
					} catch(SocketException ex)
					{
						Output?.Out(ex.Message);

					}

				//	Output?.Out("server: "+nm.ToString());
				}

				int numToWrite = outgoingMessages.Count;
				// write out queued
				for (int i = 0; i < numToWrite; i++)
				{
					Tuple<Packet, IPEndPoint> msg;
					bool have = outgoingMessages.TryDequeue(out msg);
					if (have)
					{
						Packet packet = msg.Item1;
						IPEndPoint target = msg.Item2;
						packet.Send(UdpSocket, target);
						PacketsSent++;
						TotalBytesSent += packet.Payload.Length;
						InternalSendCount += packet.Payload.Length;
					}
					
				}

				// If Nothing happened, take a nap
				if (!canRead && (numToWrite == 0))
					Thread.Sleep(1);
			}
		}

		public void Start()
		{
			listening.Value = true;
			Output?.Out(String.Format("server: listening on port {0}",  Port), Color.Coral);
			Task.Factory.StartNew(NetworkThread);
		}

		public bool HaveIncomingMessage()
		{
			return incomingMessages.Count > 0;
		}

		public NetworkMessage GetLatestMessage()
		{
			NetworkMessage msg;
			bool success = incomingMessages.TryDequeue(out msg);

			if (success)
			{
				return msg;
			}
			throw new Exception("No Message Queued! Use HaveIncomingMessage() to check!");
		}

		public void SendPacket(Packet packet, IPEndPoint target)
		{
			outgoingMessages.Enqueue(new Tuple<Packet, IPEndPoint>(packet, target));
		}


		public override void Update(GameTime gt)
		{
			base.Update(gt);
				
		}

		public void Cleanup() { }
		public void Close()
		{
			listening.Value = false;	
		}

	}
}
