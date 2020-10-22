using CaveGame.Core.FileUtil;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client
{

	public class GameSettings : Configuration
	{
		public override void FillDefaults()
		{

		}

		public Keys MoveLeftKey { get; set; }
		public Keys MoveRightKey { get; set; }
		public Keys MoveDownKey { get; set; }
		public Keys MoveUpKey { get; set; }
		public Keys JumpKey { get; set; }
	}



	public class InputManager: GameComponent
	{
		public InputManager(Game game) : base(game)
		{

		}


		public override void Update(GameTime gt)
		{

		}
	}
}
