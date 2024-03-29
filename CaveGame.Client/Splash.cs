﻿using CaveGame.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CaveGame.Common.Extensions;

namespace CaveGame.Client
{
    public class Splash
    {
		public bool SplashActive => (SplashTimer > 0);
		public float SplashTimer { get; set; }

		public Splash()
        {
			SplashTimer = 4.5f;
        }

        public void Update(GameTime gt)
        {
			SplashTimer -= gt.GetDelta();
        }

        public void Draw(GraphicsEngine GraphicsEngine)
        {
			GraphicsEngine.Clear(Color.Black);

			var splash = GraphicsEngine.CSoftWP;
			GraphicsEngine.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
			Vector2 center = new Vector2(GraphicsEngine.WindowSize.X / 2.0f, GraphicsEngine.WindowSize.Y / 2.0f);
			Vector2 origin = new Vector2(splash.Width / 2.0f, splash.Height / 2.0f);
			var scale = center/origin;

			Vector2 bounds = GraphicsEngine.Fonts.Arial30.MeasureString("CONARIUM SOFTWARE");

			Color drawColor = Color.White;

			GraphicsEngine.Sprite(splash, center, null, drawColor, Rotation.Zero, origin, scale, SpriteEffects.None, 0);

			GraphicsEngine.End();
		}
    }
}
