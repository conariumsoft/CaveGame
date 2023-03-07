using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common
{
    public interface ISteamManager
    {
        bool HasAchievement(GameSteamAchievement ach);
        void AwardAchievement(GameSteamAchievement ach);
    }
}
