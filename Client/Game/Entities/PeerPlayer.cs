﻿using CaveGame.Core;
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
		public override void Draw(SpriteBatch sb)
		{

			Vector2 namebounds = GameFonts.Arial8.MeasureString(DisplayName);
			sb.Print(GameFonts.Arial8, Color.White, Position - new Vector2(namebounds.X/2, namebounds.Y), DisplayName);
			base.Draw(sb);
		}

		public void ClientPhysicsTick(IClientWorld world, float step) => Position = Position.Lerp(NextPosition, 0.5f);
    }
}
