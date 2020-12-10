using CaveGame.Core.Game.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CaveGame.Core.Network
{
	public class User
	{
		public IPEndPoint EndPoint { get; set; }
		public string Username { get; set; }
		public int UserNetworkID { get; set; }
		public float KeepAlive { get; set; }
		public Player PlayerEntity { get; set; }



		private ConcurrentQueue<Packet> dispatchedPackets;


		public bool Kicked { get; private set; }
		public string DisconnectReason { get; private set; }
		
		public User()
		{
			dispatchedPackets = new ConcurrentQueue<Packet>();
		}

		public bool DispatcherHasMessage()
        {
			return dispatchedPackets.Count > 0;
        }

		public Packet PopDispatcherQueue()
        {
			if (!DispatcherHasMessage())
				throw new Exception("Check the dispatcher you dipshit!");


			bool success = dispatchedPackets.TryDequeue(out Packet payload);
			return payload;
        }

		public void Send(Packet p) => dispatchedPackets.Enqueue(p);

		public void Kick(string reason)
		{
			DisconnectReason = reason;

			Kicked = true;
		}
	}
}
