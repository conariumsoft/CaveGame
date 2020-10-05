using CaveGame.Core.FileUtil;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;

namespace CaveGame.Server
{

	[Serializable]
	public class ServerConfig : Configuration
	{
		public override void FillDefaults()
		{
			Port = 40269;
			World = "World1";
			MaxPlayers = 40;
			ServerName = "Standalone";
			TickRate = 20;
		}

		public int TickRate;
		public string ServerName;
		public int Port;
		public int MaxPlayers;
		public string World;
		public string IPAddress;

		[NonSerialized]
		public int Based;
	}
}
