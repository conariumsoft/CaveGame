using DataManagement;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;

namespace CaveGame.Core
{
	/// <summary>
	/// A structure representing lighting value in-game
	/// Roughly equatable to a color struct, but with specialized
	/// methods for dealing with light calculation
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct Light3 : IEquatable<Light3>
	{
		public static Light3 Dark = new Light3(0, 0, 0);
		public static Light3 Moonlight = new Light3(128, 128, 128);
		public static Light3 Dawn = new Light3(96, 96, 40);
		public static Light3 Ambience = new Light3(128, 128, 128);
		public static Light3 Daylight = new Light3(128, 128, 128);
		public static Light3 Dusk = new Light3(96, 60, 40);

		[FieldOffset(0)] public byte Red;
		[FieldOffset(1)] public byte Blue;
		[FieldOffset(2)] public byte Green;



		public Light3(byte r, byte g, byte b)
		{
			Red = r;
			Green = g;
			Blue = b;
		}

		/*public Light3(float r, float g, float b)
        {
			Red =   (byte)(r * 255);
			Green = (byte)(g * 255);
			Blue =  (byte)(b * 255);
        }*/

		public bool Equals(Light3 other)=>(other.Red == Red && other.Blue == Blue && other.Green == Green);


		public Color ToColor() => new Color(Red, Green, Blue);


		public Color MultiplyAgainst(Color col)
		{
			return new Color(
				(col.R / 255.0f) * (Red / 127.0f),
				 (col.G / 255.0f) * (Green / 127.0f),
				(col.B / 255.0f) * (Blue / 127.0f)
			);
		}

		public static Color operator *(Light3 l, Color c) => l.MultiplyAgainst(c);
		public static Color operator *(Color c, Light3 l) => l.MultiplyAgainst(c);

		public static Light3 operator +(Light3 a, Light3 b) => new Light3(a.Red.AddByte(b.Red), a.Green.AddByte(b.Green), a.Blue.AddByte(b.Blue));

		public static Light3 operator -(Light3 a, Light3 b) => new Light3(a.Red.SubtractByte(b.Red), a.Green.SubtractByte(b.Green), a.Blue.SubtractByte(b.Blue));

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
