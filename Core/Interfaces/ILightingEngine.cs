using CaveGame.Core;
using CaveGame.Core.Game.Tiles;
using CaveGame.Core.Game.Walls;
using Microsoft.Xna.Framework;

namespace CaveGame.Core
{
    public interface ILightingEngine
    {
        Light3 GetLight(int x, int y);
        Light3 GetLight(Point coords);
        //void SetLight(int x, int y, Light3 val);
       // void SetLight(Point coords, Light3 val);
        void InvokeLight(Point coords, Light3 val);
    }
}