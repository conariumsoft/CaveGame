using CaveGame.Core.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CaveGame.Client
{

	public interface ISteamManager
	{


		bool SteamEnabled { get; set; }
		bool SteamInitialized { get; }

		//void OnUserStatsReceived(UserStatsReceived_t pCallback);
		void Shutdown();
		void Update(GameTime gameTime);
	}
}
