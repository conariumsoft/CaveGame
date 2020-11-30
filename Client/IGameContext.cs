using CaveGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CaveGame.Client
{
	public interface IGameContext
	{
		Microsoft.Xna.Framework.Game Game { get; }
		bool Active { get; set; }


		void Load();
		void Unload();
		void Update(GameTime gt);
		void Draw(GraphicsEngine gfx);
	}
}
