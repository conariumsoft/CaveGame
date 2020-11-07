using CaveGame.Core.Game.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CaveGame.Core.Network
{
	public class User
	{
		public IPEndPoint EndPoint { get; set; }
		public string Username { get; set; }
		public int UserNetworkID { get; private set; }
		public float KeepAlive { get; set; }
		public Player PlayerEntity { get; set; }


		public bool Kicked { get; private set; }
		public string DisconnectReason { get; private set; }
		
		public User()
		{
			UserNetworkID = this.GetHashCode();
		}

		public void Kick(string reason)
		{
			DisconnectReason = reason;

			Kicked = true;
		}
	}
}
