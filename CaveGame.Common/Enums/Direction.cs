using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common
{
	public enum Direction : byte
	{
		Left,
		Right,
		Up,
		Down,
	}

	/// <summary>
	/// Cardinal Directions. Generally used for surface face of a tile.
	/// </summary>
	public enum Face
	{
		Top,
		Bottom,
		Left,
		Right
	}
	public enum Compass
	{
		North = 0,
		Northeast = 45,
		East = 90,
		Southeast = 135,
		South = 180,
		Southwest = 225,
		West = 270,
		Northwest = 315
	}

	public static class CardinalDirectionExtension
	{
		public static Rotation ToRotation(this Compass dir) => Rotation.FromDeg((int)dir);
		public static Vector2 ToSurfaceNormal(this Face face)
		{
			Vector2 normal = Vector2.Zero;
			if (face == Face.Top)
				normal = new Vector2(0, -1);
			if (face == Face.Bottom)
				normal = new Vector2(0, 1);
			if (face == Face.Left)
				normal = new Vector2(-1, 0);
			if (face == Face.Right)
				normal = new Vector2(1, 0);
			return normal;
		}
	}
}
