using Microsoft.Xna.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client
{
	public static class Achievements
	{
		public static Achievement[] List =
		{
			new Achievement("FIRST_TREE"),
			new Achievement("BRUG"),
			new Achievement("DUMP"),
		};
	}
	public class Achievement
	{
		public string SteamAcheivementID { get; private set; }
		public string Name;
		public string Description;
		public bool Achieved;

		public Achievement(string _id)
		{
			SteamAcheivementID = _id;
			Achieved = false;
		}


		public void Unlock()
		{
			if (!Achieved)
				Achieved = true;

			SteamUserStats.SetAchievement(SteamAcheivementID);

		}

	}

	public class SteamManager: GameComponent
	{
		Game game;
		bool receivedUserStats;

		public SteamManager(Game _game) : base(_game)
		{
			game = _game;
		}

		public void OnUserStatsReceived(UserStatsReceived_t pCallback)
		{

		}

		private void PollStats()
		{
			if (!receivedUserStats)
			{
			}
		}

		public override void Update(GameTime gameTime)
		{
			
		}
	}
}
