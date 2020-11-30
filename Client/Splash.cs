using CaveGame.Core;
using CaveGame.Core.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client
{
    public class Splash
    {
		public bool SplashActive => (SplashTimer > 0);
		public float SplashTimer { get; set; }


		public Splash()
        {
			SplashTimer = 5;
        }

        public void Update(GameTime gt)
        {
			SplashTimer -= gt.GetDelta();
        }


        public void Draw(GraphicsEngine GraphicsEngine)
        {
			GraphicsEngine.Clear(Color.Black);
			//GraphicsEngine.Begin();


			GraphicsEngine.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
			Vector2 center = new Vector2(GraphicsEngine.WindowSize.X / 2.0f, GraphicsEngine.WindowSize.Y / 2.0f);
			Vector2 origin = new Vector2(GraphicsEngine.EyeOfHorus.Width / 2.0f, GraphicsEngine.EyeOfHorus.Height / 2.0f);
			float scale = 8;

			Vector2 bounds = GraphicsEngine.Fonts.Arial30.MeasureString("CONARIUM SOFTWARE");

			GraphicsEngine.Sprite(GraphicsEngine.EyeOfHorus, center - new Vector2(0, (float)Math.Sin(SplashTimer * 2) * 10), null, Color.White, Rotation.Zero, origin, scale, SpriteEffects.None, 0);

			GraphicsEngine.Text(
				font: GraphicsEngine.Fonts.Arial30,
				text: "CONARIUM SOFTWARE",
				position: center + new Vector2(0, 100), Color.White, TextXAlignment.Center, TextYAlignment.Center);
			GraphicsEngine.End();
		}
    }
}
