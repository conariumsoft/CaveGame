using CaveGame.Core;
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

namespace CaveGame.Client
{

	//Graph

	public class NetworkClient
	{

		public IPEndPoint EndPoint
		{
			get {
				IPEndPoint output = default;
				bool success = IPEndPoint.TryParse(ServerHostname + ":"+ServerPort, out output);
				return output;
			}
		}

		public IMessageOutlet Output { get; set; }

		public int ReceivedCount { get; private set; }
		public int SentCount { get; private set; }
		private DateTime lastPacketReceivedTime = DateTime.MinValue;
		private DateTime lastPacketSentTime = DateTime.MinValue;
		private long lastPacketReceivedTimestamp = 0; // from server
		private TimeSpan heartbeatTimeout = TimeSpan.FromSeconds(20);

		private ThreadSafe<bool> running = new ThreadSafe<bool>(false);
		
		public readonly string ServerHostname;
		public readonly int ServerPort;

		private ConcurrentQueue<NetworkMessage> incomingMessages;
		private ConcurrentQueue<Packet> outgoingMessages;

		private UdpClient udpClient;

		public NetworkClient(string ipaddress)
		{
			IPEndPoint addr;
			
			bool success = IPEndPoint.TryParse(ipaddress, out addr);
			
			ServerHostname = addr.Address.ToString();
			ServerPort = addr.Port;
			System.Diagnostics.Debug.WriteLine(addr.ToString());
			incomingMessages = new ConcurrentQueue<NetworkMessage>();
			outgoingMessages = new ConcurrentQueue<Packet>();

			udpClient = new UdpClient(ServerHostname, ServerPort);
		}

		public NetworkClient(string hostname, int port)
		{

			incomingMessages = new ConcurrentQueue<NetworkMessage>();
			outgoingMessages = new ConcurrentQueue<Packet>();

			ServerHostname = hostname;
			ServerPort = port;
			udpClient = new UdpClient(ServerHostname, ServerPort);
		}

		private void NetworkRun()
		{
			while (running.Value) {
				bool canRead = udpClient.Available > 0;
				int outgoingMessageQueueCount = outgoingMessages.Count;

				// get data if there's any
				if (canRead)
				{
					IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = udpClient.Receive(ref ep);

					NetworkMessage nm = new NetworkMessage();
					nm.Sender = ep;
					nm.Packet = new Packet(data);
					nm.ReceiveTime = DateTime.Now;

					incomingMessages.Enqueue(nm);
					ReceivedCount++;
					lastPacketReceivedTime = DateTime.Now;
				}

				// write out queued messages
				for (int i = 0; i < outgoingMessageQueueCount; i++)
				{
					Packet packet;
					bool have = outgoingMessages.TryDequeue(out packet);

					if (have)
					{
						packet.Send(udpClient);
						SentCount++;
						//Output?.Out(string.Format("client: sending {0} at {1}", packet.Payload, packet.Timestamp), Color.Cyan);
					}
						
				}

				// if nothing happened, take a nap
				if (!canRead && (outgoingMessageQueueCount == 0))
					Thread.Sleep(1);
			}

			
			udpClient.Close();
			udpClient.Dispose();
		}

		public void SendPacket(Packet packet)
		{
			Output?.Out("client: Sending packet " + packet.Type.ToString(), Color.Cyan);
			outgoingMessages.Enqueue(packet);
			lastPacketSentTime = DateTime.Now;
		}

		public void Start()
		{
			running.Value = true;
			Task.Factory.StartNew(NetworkRun);
		}

		public void Stop()
		{
			running.Value = false;
			
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
			throw new Exception("No Message Queued! Used HaveIncomingMessage() to check!");
		}

	}
}
