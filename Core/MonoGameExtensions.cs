using DataManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaveGame.Core
{

	public static class MonoGameExtensions
	{
		public static Vector2 LookAt(this Vector2 origin, Vector2 goal)
        {
			Vector2 UnitVector = (goal - origin);
			UnitVector.Normalize();
			return UnitVector;
		}

		public static float Distance(this Vector2 a, Vector2 b) => (a - b).Length();

		public static Color Inverse(this Color color)
		{
			int r = 0xFFFFFF - color.R;
			int g = 0xFFFFFF - color.G;
			int b = 0xFFFFFF - color.B;
			return new Color(r, g, b);
		}

		public static float GetTextWidth(this string text, SpriteFont font)
		{
			return font.MeasureString(text).X;
		}


		public static float GetDelta(this GameTime gt)
		{
			return (float)gt.ElapsedGameTime.TotalSeconds;
		}

		public static Vector2 Lerp(this Vector2 a, Vector2 b, float alpha)
		{
			return new Vector2(
				MathematicsExtensions.Lerp(a.X, b.X, alpha),
				MathematicsExtensions.Lerp(a.Y, b.Y, alpha)
			);
		}

		public static Color Sub(this Color a, Color b)
		{
			return new Color(a.R - b.R, a.G - b.G, a.B - b.B);
		}

		public static Color Add(this Color a, Color b)
		{
			return new Color(a.R + b.R, a.G + b.G, a.B + b.B);
		}
	}
}
