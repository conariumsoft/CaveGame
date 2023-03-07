using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common
{
	public struct Rotation : IEquatable<Rotation>
	{
		public static Rotation Zero = FromDeg(0);
		public static Rotation Pi = FromRad((float)Math.PI);
		public static Rotation TwoPi = FromRad((float)Math.PI * 2);
		public static Rotation RightAngle = FromDeg(90);


		public const float PI = MathHelper.Pi;
		private float _radians;

		public float Radians
		{
			get { return _radians; }
			set { _radians = value; }
		}
		public float Degrees
		{
			get { return _radians * (180.0f / PI); }
			set { _radians = value * (PI / 180.0f); }
		}


		public Vector2 ToUnitVector()
		{
			return new Vector2((float)Math.Cos(Radians), (float)Math.Sin(Radians));
		}

		public static Rotation FromUnitVector(Vector2 vec)
		{
			vec.Normalize();
			return Rotation.FromRad((float)Math.Atan2(vec.Y, vec.X));
		}

		public static Rotation FromRad(float radians)
		{
			return new Rotation { Radians = radians };
		}

		public static Rotation FromDeg(float degree)
		{
			return new Rotation { Degrees = degree };
		}

		public Rotation RotateDeg(float degree) => FromDeg(this.Degrees + degree);

		public Rotation RotateRad(float radians) => FromRad(this.Radians + radians);

		public bool EqualsRadians(float rad, float toleranceDegrees = 1.0f)
		{
			return Math.Abs(Radians - rad) < (toleranceDegrees * (PI / 180.0f));
		}
		public bool EqualsDegrees(float deg, float toleranceDegrees = 1.0f)
		{
			return Math.Abs(Degrees - deg) < toleranceDegrees;
		}

		public bool Equals(Rotation other)
		{
			return EqualsRadians(other.Radians);
		}
	}
}
