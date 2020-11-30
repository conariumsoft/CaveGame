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

			string hptext = Health + "/" + MaxHealth + " HP";
			Vector2 hpbounds = gfx.Fonts.Arial8.MeasureString(Health + "/" + MaxHealth + " HP");
			//sb.Print(GameFonts.Arial8, Color.Red, Position - new Vector2(hpbounds.X / 2, hpbounds.Y*2), );

			gfx.Text(gfx.Fonts.Arial8, hptext, Position-new Vector2(0, BoundingBox.Y/2), Color.White, TextXAlignment.Center, TextYAlignment.Bottom);

			gfx.Sprite(gfx.Player, TopLeft, spriteFrame, Color, Rotation.Zero, new Vector2(0, 0), 1, (SpriteEffects)flipSprite, 0);
		}
	}
}
