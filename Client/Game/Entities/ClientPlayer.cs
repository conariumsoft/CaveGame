using CaveGame.Core;
using CaveGame.Core.Game.Entities;
using CaveGame.Core.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client.Game.Entities
{
	public abstract class ClientPlayer : Core.Game.Entities.Player
	{
		public override void Draw(GraphicsEngine gfx)
		{

			Rectangle spriteFrame = new Rectangle(0, 0, 16, 24);

			int flipSprite = 0;
			if (Facing == Direction.Left)
				flipSprite = 0;
			if (Facing == Direction.Right)
				flipSprite = 1;


			if (Walking)
			{
				spriteFrame = new Rectangle(16, 0, 16, 24);
				if (walkingAnimationTimer % 2 >= 1)
					spriteFrame = new Rectangle(32, 0, 16, 24);
			}

			if (!OnGround)
				spriteFrame = new Rectangle(48, 0, 16, 24);


			DrawHealth(gfx);
			gfx.Sprite(gfx.Player, TopLeft, spriteFrame, Color, Rotation.Zero, new Vector2(0, 0), 1, (SpriteEffects)flipSprite, 0);
		}
	}
}
