using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaveGame.Core
{

	public static class Extensions
	{
		public static Vector2 Lerp(this Vector2 a, Vector2 b, float alpha)
		{
			return new Vector2(
				Lerp(a.X, b.X, alpha),
				Lerp(a.Y, b.Y, alpha)
			);
		}

		public static float Lerp(this float a, float b, float alpha)
		{
			return a + (b - a) * alpha;
		}
	}
}
