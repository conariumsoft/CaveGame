using CaveGame.Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client
{
	public class StatusEffectDisplay
	{

		GameClient Client { get; set; }
		public StatusEffectDisplay(GameClient client)
		{
			Client = client;
		}

		public void Update(GameTime gt)
		{
			var player = Client.MyPlayer;
			if (player == null)
				return;


		}

		public void Draw(GraphicsEngine GFX)
		{
			var player = Client.MyPlayer;
			if (player == null)
				return;

			int idx = 0;
			foreach(var effect in player.ActiveEffects)
			{
				var TopRight = new Vector2(GFX.WindowSize.X-100, idx*20);
				GFX.Rect(Color.Gray, TopRight, new Vector2(90, 20));
				GFX.Text(effect.EffectName+": "+effect.Duration, TopRight);
				idx++;
			}
		}
	}
}
