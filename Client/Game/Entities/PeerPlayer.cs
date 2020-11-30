using CaveGame.Core;
using CaveGame.Core.Game.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client.Game.Entities
{
	// Player entities that are controlled by other clients
	public class PeerPlayer : ClientPlayer, IClientPhysicsObserver
	{
		//public void ClientPhysicsTick(IClientWorld world, float step) => PhysicsStep(world, step);
		public override void Draw(GraphicsEngine GFX)
		{

			Vector2 namebounds = GFX.Fonts.Arial8.MeasureString(DisplayName);
			GFX.Text(GFX.Fonts.Arial8, DisplayName, Position - new Vector2(namebounds.X/2, namebounds.Y), Color.White);
			base.Draw(GFX);
		}

		public void ClientPhysicsTick(IClientWorld world, float step) => Position = Position.Lerp(NextPosition, 0.5f);
    }
}
