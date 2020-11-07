using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;

namespace CaveGame.Core
{
	[StructLayout(LayoutKind.Explicit)]
	public struct Light3 : IEquatable<Light3>
	{
		public static Light3 Dark = new Light3(0, 0, 0);
		public static Light3 Ambience = new Light3(16, 16, 16);

		[FieldOffset(0)] public byte Red;
		[FieldOffset(1)] public byte Blue;
		[FieldOffset(2)] public byte Green;

		public Light3(byte r, byte g, byte b)
		{
			Red = r;
			Green = g;
			Blue = b;
		}

		public bool Equals(Light3 other)
		{
			return (other.Red == Red && other.Blue == Blue && other.Green == Green);
		}
		private static double Squirt = Math.Sqrt(15.0);
		public Color MultiplyAgainst(Color col)
		{
			return new Color(
				(col.R / 255.0f) * (float)(Math.Sqrt(Red / 30.0f)),
				 (col.G / 255.0f) * (float)(Math.Sqrt(Green / 30.0f)),
				(col.B / 255.0f) * (float)(Math.Sqrt(Blue / 30.0f)),
				col.A
			);
		}

		public Light3 Absorb(byte opacity)
		{
			byte red = (byte)Math.Max(0, Red - opacity);
			byte green = (byte)Math.Max(0, Green - opacity);
			byte blue = (byte)Math.Max(0, Blue - opacity);

			return new Light3(red, green, blue);
		}

		public override string ToString()
		{
			return string.Format("[{0} {1} {2}]", Red, Green, Blue);
		}

	}
}
