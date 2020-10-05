using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CaveGame.Client
{
	public interface IGameContext
	{
		public Game Game { get; }
		public bool Active { get; set; }


		void Load();
		void Unload();
		void Update(GameTime gt);
		void Draw(SpriteBatch sb);
	}
	/*	public interface IGameState
	{
		public Game Game { get; }
		public bool Active { get; set; }

		void Initialize();
		void LoadContent();
		void UnloadContent();
		void Update(GameTime gt);
		void Draw(SpriteBatch sb);
	}*/
}
