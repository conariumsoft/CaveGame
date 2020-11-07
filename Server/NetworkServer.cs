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

	public class NetworkServer
	{

		public readonly int Port;

		public int ProtocolVersion;


		public IMessageOutlet Output { get; set; }

		private UdpClient udpClient;
		

		private ConcurrentQueue<NetworkMessage> incomingMessages;
		private ConcurrentQueue<Tuple<Packet, IPEndPoint>> outgoingMessages;

		private ThreadSafeValue<bool> listening = new ThreadSafeValue<bool>(false);

		public NetworkServer(int port)
		{

			udpClient = new UdpClient(port, AddressFamily.InterNetwork);

			Port = port;
			incomingMessages = new ConcurrentQueue<NetworkMessage>();
			outgoingMessages = new ConcurrentQueue<Tuple<Packet, IPEndPoint>>();

		}

		private string DumpHex(byte[] data)
		{
			StringBuilder bob = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				bob.Append(String.Format("{0:X2} ", data[i]));
			}
			return bob.ToString();
		}

		private void NetworkThread()
		{
			if (!listening.Value)
				return;

			while (listening.Value)
			{
				
				bool canRead = udpClient.Available > 0;

				// Read in message if we have any data
				if (canRead)
				{

					IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
					try
					{
						byte[] data = udpClient.Receive(ref ep);

						NetworkMessage nm = new NetworkMessage();
						nm.Sender = ep;
						nm.Packet = new Packet(data);
						nm.ReceiveTime = DateTime.Now;


#if PACKETDEBUG
				Output?.Out(String.Format("PK_IN: [{0}] at [{1} {2}] from [{3}]", nm.Packet.Type.ToString(), nm.ReceiveTime.ToLongTimeString(), nm.ReceiveTime.Millisecond, nm.Sender.ToString()));
				Output?.Out(String.Format("DATUM: [{0}]", DumpHex(nm.Packet.Payload)));
				Output?.Out("");
#endif

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
					#if PACKETDEBUG
						Output?.Out(String.Format("PK_OUT: [{0}] at [{1} {2}] to [{3}]", msg.Item1.Type.ToString(), DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond, msg.Item2.ToString()));
						Output?.Out(String.Format("DATUM: [{0}]", DumpHex(msg.Item1.Payload)));
						Output?.Out("");
					#endif
						msg.Item1.Send(udpClient, msg.Item2);
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
		}
		public void Run() {
			Task.Factory.StartNew(NetworkThread);
			Output?.Out("server: Network listener thread created", Color.Coral);
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

		public void SendPacket(Packet packet, IPEndPoint target)
		{
			//Output?.Out("server: Sending packet " + packet.Type.ToString(), Color.Coral);
			outgoingMessages.Enqueue(new Tuple<Packet, IPEndPoint>(packet, target));
		}

		public void Cleanup() { }
		public void Close()
		{
			listening.Value = false;	
		}

	}
}
