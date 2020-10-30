using CaveGame.Core.Generic;
using Microsoft.Xna.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		public bool SteamEnabled { get; set; }
		public bool SteamInitialized { get; private set; }
		

		Game game;
		bool receivedUserStats;

		DelayedTask callbackRun;

		public SteamManager(Game _game) : base(_game)
		{
			game = _game;

			SteamInitialized = true;
			Steam_EnsureDLLExists();
			Steam_InitAPI();

			

			if (SteamInitialized)
			{
				callbackRun = new DelayedTask(() => Steamworks.SteamAPI.RunCallbacks(), 1 / 20.0f);
				m_OverlayActivated = Steamworks.Callback<Steamworks.GameOverlayActivated_t>.Create(Steam_OnOverlayActivated);
				Steamworks.Callback<Steamworks.SteamShutdown_t>.Create(Steam_OnShutdown);
				Steamworks.Callback<Steamworks.ScreenshotRequested_t>.Create(Steam_OnScreenshotRequested);
				Steamworks.Callback<Steamworks.UserStatsReceived_t>.Create(Steam_OnUserStatsReceived);
				Steamworks.Callback<Steamworks.UserStatsStored_t>.Create(Steam_OnUserStatsStored);
				Steamworks.Callback<Steamworks.UserAchievementStored_t>.Create(Steam_OnAchievementsStored);
			}
				
		}


		#region Steam Callbacks
		private void Steam_OnOverlayActivated(Steamworks.GameOverlayActivated_t pCallback)
		{

		}
		private void Steam_OnShutdown(Steamworks.SteamShutdown_t callback) { }
		private void Steam_OnScreenshotRequested(Steamworks.ScreenshotRequested_t callback) { }
		private void Steam_OnUserStatsReceived(Steamworks.UserStatsReceived_t callback) { }
		private void Steam_OnUserStatsStored(UserStatsStored_t param)
		{

		}
		private void Steam_OnAchievementsStored(UserAchievementStored_t param)
		{
		}
		protected Steamworks.Callback<Steamworks.GameOverlayActivated_t> m_OverlayActivated;
		#endregion

		private void Steam_LoadAchievements()
		{
			foreach (Achievement ach in Achievements.List)
			{
				bool ret = SteamUserStats.GetAchievement(ach.SteamAcheivementID, out ach.Achieved);

				if (ret)
				{
					ach.Name = SteamUserStats.GetAchievementDisplayAttribute(ach.SteamAcheivementID, "name");
					ach.Description = SteamUserStats.GetAchievementDisplayAttribute(ach.SteamAcheivementID, "desc");
				}
			}
		}

		private void Steam_EnsureDLLExists()
		{
			try
			{
				if (Steamworks.SteamAPI.RestartAppIfNecessary((Steamworks.AppId_t)1238250))
				{
					Debug.WriteLine("Steam restarting?");
					game.Exit();
				}
			}
			catch (System.DllNotFoundException e)
			{
				Debug.WriteLine("Missing steam_api64.dll");
				//throw new Exception("Missing steam_api64.dll");
				SteamInitialized = false;
			}
		}

		private void Steam_InitAPI()
		{
			try
			{
				if (!Steamworks.SteamAPI.Init())
				{
					Debug.WriteLine("Steam API Failed to initialize!");
					//throw new Exception("Steam API Failed to initialize!");
					SteamInitialized = false;
				}
			} catch(Exception e)
			{
				Debug.WriteLine("Steam API Failed to initialize!");
				//throw new Exception("Steam API Failed to initialize!");
				SteamInitialized = false;
			}
			
		}

		public void OnUserStatsReceived(UserStatsReceived_t pCallback)
		{

		}

		public void Shutdown()
		{
			if (SteamInitialized)
				SteamAPI.Shutdown();
		}

		private void PollStats()
		{
			if (!receivedUserStats)
			{
			}
		}

		public override void Update(GameTime gameTime)
		{
			callbackRun?.Update(gameTime);
		}
	}
}
