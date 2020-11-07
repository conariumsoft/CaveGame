using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaveGame.Core
{
	public class FrameCounter: GameComponent
	{
		static int framecounting = 120;

		double framerate;
		double[] framesamples;
		int frames;
		double averageFramerate;

		public FrameCounter(Microsoft.Xna.Framework.Game game): base(game)
		{
			frames = 0;
			framerate = 0;
			averageFramerate = 0.1;
			framesamples = new double[framecounting];
		}

		public override void Update(GameTime gameTime)
		{
			double dt = gameTime.ElapsedGameTime.TotalSeconds;

			// frame counter
			frames++;

			if (frames >= framecounting)
			{
				frames = 0;
			}

			framerate = 1 / dt;

			averageFramerate -= framesamples[frames];
			framesamples[frames] = dt;
			averageFramerate += framesamples[frames];
		}

		public double GetExactFramerate()
		{
			return framerate;
		}

		public double GetAverageFramerate()
		{
			return framecounting / averageFramerate;
		}
	}
}
