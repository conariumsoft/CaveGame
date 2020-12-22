using CaveGame.Core;
using CaveGame.Core.Generic;
using CaveGame.Core.Network;
using CaveGame.Core.Network.Packets;
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
	public class NetworkClient : SharedNetworkSubsystem
	{
		public IPEndPoint EndPoint
		{
			get {
				IPEndPoint output = default;
				bool success = IPEndPoint.TryParse(ServerHostname + ":"+ServerPort, out output);
				return output;
			}
		}



		private ThreadSafeValue<bool> running = new ThreadSafeValue<bool>(false);
		
		public readonly string ServerHostname;
		public readonly int ServerPort;

		private ConcurrentQueue<NetworkMessage> incomingMessages;
		private ConcurrentQueue<Packet> outgoingMessages;


		public static bool IsServerOnline(string hostname)
        {
            try
            {
                Ping pingSender = new Ping();
                PingOptions options = new PingOptions();
                options.DontFragment = true;
                string data = "CAVEGAME CAVEGAME CAVEGAME";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 120;

                PingReply reply = pingSender.Send(hostname, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                    return true;

            }
            catch (Exception ex)
            {


            }
			return false;
		}

		public static bool IsAddressValidIPv4(string hostname)=>IPEndPoint.TryParse(hostname, out _);

		public static HandshakeResponsePacket GetServerInformation(string hostname)
        {

			var tempClient = new NetworkClient(hostname);
			return null;

			// TODO: Finish
        }

		public NetworkClient(string ipaddress)
		{
			IPEndPoint addr;

			bool success = IPEndPoint.TryParse(ipaddress, out addr);

			ServerHostname = addr.Address.ToString();
			ServerPort = addr.Port;

			UdpSocket = new UdpClient(ServerHostname, ServerPort);
			IOControlFixICMPBullshit();
		}

		private void FlushOutgoingPackets()
        {
			int outQCount = outgoingMessages.Count;
			// write out queued messages
			for (int i = 0; i < outQCount; i++)
			{
				Packet packet;
				bool have = outgoingMessages.TryDequeue(out packet);

				if (have)
				{
					packet.Send(UdpSocket);
					PacketsSent++;
					TotalBytesSent += packet.Payload.Length;
					InternalSendCount += packet.Payload.Length;
				}
			}
		}

		private void ReadIncomingPackets()
        {
			IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
			byte[] data = UdpSocket.Receive(ref ep);

			NetworkMessage nm = new NetworkMessage();
			nm.Sender = ep;
			nm.Packet = new Packet(data);
			nm.ReceiveTime = DateTime.Now;

			incomingMessages.Enqueue(nm);
			PacketsReceived++;
			TotalBytesReceived += nm.Packet.Payload.Length;
			InternalReceiveCount += nm.Packet.Payload.Length;
			LatestReceiveTimestamp = DateTime.Now;
		}

		
		private void NetworkThreadLoop()
		{
			GameConsole.Log("NetworkClientSubsystem thread started.");
			while (running.Value) {
				bool canRead = UdpSocket.Available > 0;
				int outgoingMessageQueueCount = outgoingMessages.Count;

				// get data if there's any
				if (canRead)
					ReadIncomingPackets();

				FlushOutgoingPackets();

				// if nothing happened, take a nap
				if (!canRead && (outgoingMessageQueueCount == 0))
					Thread.Sleep(NAP_TIME_MILLISECONDS);
			}

			
			UdpSocket.Close();
			UdpSocket.Dispose();
			GameConsole.Log("NetworkClientSubsystem thread stopped.");
		}

		public void SendPacket(Packet packet)
		{
			//Output?.Out("client: Sending packet " + packet.Type.ToString(), Color.Cyan);
			outgoingMessages.Enqueue(packet);
			LatestSendTimestamp = DateTime.Now;
		}

		public void Logout(int userID, UserDisconnectReason reason)
        {
			SendPacket(new DisconnectPacket(userID, reason));
			Stop();
		}

		public void Start()
		{
			running.Value = true;
			Task.Factory.StartNew(NetworkThreadLoop);
		}

		public void Stop()
		{
			running.Value = false;
		}

		
		public override void Update(GameTime gt)
        {
			base.Update(gt);
        }

		public bool HaveIncomingMessage() => incomingMessages.Count > 0;

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
