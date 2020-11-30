using CaveGame.Client.UI;
using CaveGame.Core.FileUtil;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CaveGame.Client.Menu
{
	[Serializable]
	public class MultiplayerInputHistory : Configuration
	{

		public static MultiplayerInputHistory Load()
        {
			return Configuration.Load<MultiplayerInputHistory>("mphistory.xml");
		}

		public override void FillDefaults()
		{
			IPAddress = "";
			Username = "";
		}

		public string IPAddress;
		public string Username;
	}

	[Serializable]
	public class ServerHistoryPersistence : Configuration
	{
		public override void FillDefaults()
		{
			IPAddress = new List<string>();
		}

		public List<string> IPAddress;

	}
}
