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

		public User()
		{
			UserNetworkID = this.GetHashCode();
		}
	}
}
