using CaveGame.Core;
using CaveGame.Core.Game.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client.Game.Entities
{
	public abstract class ClientPlayer : Core.Game.Entities.Player
	{
		public override void Draw(SpriteBatch sb)
		{

			Rectangle spriteFrame = new Rectangle(0, 0, 16, 24);

			int flipSprite = 0;
			if (Facing == HorizontalDirection.Left)
				flipSprite = 0;
			if (Facing == HorizontalDirection.Right)
				flipSprite = 1;


			if (Walking)
			{
				spriteFrame = new Rectangle(16, 0, 16, 24);
				if (walkingAnimationTimer % 2 >= 1)
					spriteFrame = new Rectangle(32, 0, 16, 24);
			}

			if (!OnGround)
				spriteFrame = new Rectangle(48, 0, 16, 24);

			Vector2 hpbounds = GameFonts.Arial8.MeasureString(Health + "/" + MaxHealth + " HP");
			sb.Print(GameFonts.Arial8, Color.Red, Position - new Vector2(hpbounds.X / 2, hpbounds.Y*2), Health + "/" + MaxHealth + " HP");

			sb.Draw(GameTextures.Player, TopLeft, spriteFrame, Color, 0, new Vector2(0, 0), 1, (SpriteEffects)flipSprite, 0);
		}
	}
}
