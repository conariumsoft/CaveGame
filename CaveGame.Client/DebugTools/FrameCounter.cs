using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Again, Why is this in Common (Previously Core) Namespace?
namespace CaveGame.Common
{
	public class FrameCounter: GameComponent
	{
		const int FrameBufferSampleSize = 120;

		private double _framerate;
		double[] _framesamples;
		int frames;
		double averageFramerate;

		public FrameCounter(Microsoft.Xna.Framework.Game game): base(game)
		{
			frames = 0;
			_framerate = 0;
			averageFramerate = 0.1;
			_framesamples = new double[FrameBufferSampleSize];
		}

		public override void Update(GameTime gameTime)
		{
			double dt = gameTime.ElapsedGameTime.TotalSeconds;

			// frame counter
			frames++;

			if (frames >= FrameBufferSampleSize)
			{
				frames = 0;
			}

			_framerate = 1 / dt;

			averageFramerate -= _framesamples[frames];
			_framesamples[frames] = dt;
			averageFramerate += _framesamples[frames];
		}

		public double GetExactFramerate()
		{
			return _framerate;
		}

		public double GetAverageFramerate()
		{
			return FrameBufferSampleSize / averageFramerate;
		}
	}
}
