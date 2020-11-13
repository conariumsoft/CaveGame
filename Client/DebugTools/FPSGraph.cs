using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client
{

	public struct GraphEntry
	{
		public float FrameTime;

	}

	public class FPSGraph: GameComponent
	{
		public bool Enabled { get; set; }

		public int PollingRate { get; set; }
		public int SampleCount { get; set; }

		public Vector2 Position;

		public FPSGraph(Microsoft.Xna.Framework.Game game, int pollrate, int samplecount) : base(game)
		{
			Position = new Vector2(0, 0);
			PollingRate = pollrate;
			SampleCount = samplecount;
			SamplePoints = new List<GraphEntry>();
			Enabled = false;

			for (int x = 0; x < SampleCount+1; x++)
			{
				SamplePoints.Add(new GraphEntry() { FrameTime = 0 });
			}
		}

		float timer;


		public List<GraphEntry> SamplePoints;

		KeyboardState prevKeyboard;

		public override void Update(GameTime gt)
		{

			KeyboardState keyboard = Keyboard.GetState();

			if (prevKeyboard != null)
			{
				if (keyboard.IsKeyDown(Keys.F1) && !prevKeyboard.IsKeyDown(Keys.F1)) {
					Enabled = !Enabled;
				}
			}
			prevKeyboard = keyboard;

			if (!Enabled)
				return;

			float frametime = (float)gt.ElapsedGameTime.TotalSeconds;


			timer += frametime;

			if (timer > (PollingRate/1000.0f))
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

		int sampleWidth = 3;
		float heightScale = 5000f;

		public void Draw(SpriteBatch sb)
		{
			if (!Enabled)
				return;

			sb.Begin(SpriteSortMode.BackToFront, null, SamplerState.PointClamp);
			for (int idx = 0; idx < SampleCount; idx++)
			{
				var entry = SamplePoints[idx];
				sb.Rect(Color.Red, Position + new Vector2(sampleWidth * idx, 0), new Vector2(sampleWidth, 2+(entry.FrameTime * heightScale)));
			}

			sb.End();
		}
	}
}
