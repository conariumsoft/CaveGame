using CaveGame.Core;
using CaveGame.Core.Game.Tiles;
using CaveGame.Core.Game.Walls;
using Microsoft.Xna.Framework;

namespace CaveGame.Core
{
    public interface ILightingEngine
    {
        Light3 GetLight(int x, int y);
        void SetLight(int x, int y, Light3 val);
    }
}