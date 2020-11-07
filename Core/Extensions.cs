using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaveGame.Core
{

	public static class MathematicalExtensions
	{
		public static float Distance(this Vector2 a, Vector2 b) => (a - b).Length();
		public static int Max(this int num, int max) => Math.Max(num, max);
		public static float Max(this float num, float max) => Math.Max(num, max);
		public static double Max(this double num, double max) => Math.Max(num, max);
	}


	public static class Extensions
	{

		
	

		public static bool NextBool(this Random rng, double probability = 0.5)
		{
			return rng.NextDouble() <= probability;
		}

		public static float NextFloat(this Random rng) => (float)rng.NextDouble();

		public static T OneOf<T>(this Random rng, params T[] things)
		{
			return things[rng.Next(things.Length)];
		}

		public static Color Inverse(this Color color)
		{
			int r = 0xFFFFFF-color.R;
			int g = 0xFFFFFF - color.G;
			int b = 0xFFFFFF - color.B;
			return new Color(r, g, b);
		}

		// LINQ Extensions, borrowed from Jonathan Skeet
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (action == null) throw new ArgumentNullException("action");
			foreach (var element in source)
			{
				action(element);
			}
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
				Lerp(a.X, b.X, alpha),
				Lerp(a.Y, b.Y, alpha)
			);
		}

		public static float Lerp(this float a, float b, float alpha)
		{
			return a + (b - a) * alpha;
		}

		public static void Set(ref this byte a, int pos, bool value)
		{
			if (value)
			{
				a = (byte)(a | (1 << pos));
			} else
			{
				a = (byte)(a & ~(1 << pos));
			}
			//return a;
		}

		public static bool Get(this byte a, int pos)
		{
			return ((a & (1 << pos)) != 0);
		}

		// positive modulus
		public static int Mod(this int num, int remainder)
		{
			return (num % remainder + remainder) % remainder;
		}

		public static Color Sub(this Color a, Color b)
		{
			return new Color(a.R - b.R, a.G - b.G, a.B - b.B);
		}

		public static Color Add(this Color a, Color b)
		{
			return new Color(a.R + b.R, a.G + b.G, a.B + b.B);
		}


		public static string DumpHex(this byte[] data, int index = 0)
		{
			return DumpHex(data, index, data.Length);
		}

		public static string DumpHex(this byte[] data, int index, int length)
		{
			StringBuilder bob = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				bob.Append(String.Format("{0:X2}", data[i + index]));
			}
			return bob.ToString();
		}
	}
}
