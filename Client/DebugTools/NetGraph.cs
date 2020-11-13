using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client
{


/*
	public class NetGraph: GameComponent
	{
		public List<GraphEntry> SamplePoints;
		public int PollingRate { get; set; }
		public int SampleCount { get; set; }

		public NetGraph(CaveGameGL game, int pollrate, int samplecount) : base(game)
		{
			SamplePoints = new List<GraphEntry>();
			PollingRate = pollrate;
			SampleCount = samplecount;
			Enabled = false;

			for (int x = 0; x < SampleCount + 1; x++)
			{
				SamplePoints.Add(new GraphEntry() { FrameTime = 0 });
			}
		}

		float timer;
		KeyboardState prevKeyboard = Keyboard.GetState();


		public override void Update(GameTime gameTime)
		{


			KeyboardState keyboard = Keyboard.GetState();

			if (keyboard.IsKeyDown(Keys.F2) && !prevKeyboard.IsKeyDown(Keys.F2))
			{
				Enabled = !Enabled;
			}

			prevKeyboard = keyboard;

			if (!Enabled)
				return;

			//base.Update(gameTime);

			float frametime = (float)gameTime.ElapsedGameTime.TotalSeconds;


			timer += frametime;

			if (timer > (PollingRate / 1000.0f))
			{
				timer = 0;

				var entry = new GraphEntry();

				entry.FrameTime = frametime;

				SamplePoints.Insert(0, entry);
				if (SamplePoints.Count > SampleCount)
				{
					SamplePoints.RemoveAt(SampleCount);
				}
			}
		}


		public void Draw(SpriteBatch sb)
		{

		}
	}
*/
}
