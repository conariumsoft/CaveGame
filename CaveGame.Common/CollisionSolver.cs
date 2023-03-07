using CaveGame.Common.Game.Entities;
using CaveGame.Common.Game.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using CaveGame.Common.Extensions;

namespace CaveGame.Common
{

	public struct TileRaycastResult
    {
		public bool Hit { get; set; }
		public Tile Target { get; set; }
		public Point TileCoordinates { get; set; }
		public Vector2 Intersection { get; set; }
		public Vector2 SurfaceNormal { get; set; }
		public Face Face { get; set; }
		
	}

	public struct EntityRaycastResult
    {
		public bool Hit { get; set; }
		public IEntity Target { get; set; }
		public Vector2 Intersection { get; set; }
		public Vector2 SurfaceNormal { get; set; }
		public Face Face { get; set; }
	}

	public struct LineSegment
	{
		[DataMember]
		public Vector2 A { get; set; }
		[DataMember]
		public Vector2 B { get; set; }

		public Vector2 Midpoint => new Vector2(A.X + B.X, A.Y + B.Y) / 2;

		public LineSegment(Vector2 a, Vector2 b)
        {
			A = a;
			B = b;
        }
	}

	public static class CollisionSolver
	{

		public static bool Intersects(LineSegment seg, Rectangle rect, out Vector2 intersection, out Face collidingface)
        {
			intersection = Vector2.Zero;
			collidingface = Face.Top;

			var rect_topleft     = new Vector2(rect.Left, rect.Top);
			var rect_bottomright = new Vector2(rect.Right, rect.Bottom);
			var rect_bottomleft  = new Vector2(rect.Left, rect.Bottom);
			var rect_topright    = new Vector2(rect.Right, rect.Top);

			var rect_top    = new LineSegment(rect_topleft, rect_topright);
			var rect_left   = new LineSegment(rect_topleft, rect_bottomleft);
			var rect_bottom = new LineSegment(rect_bottomleft, rect_bottomright);
			var rect_right  = new LineSegment(rect_topright, rect_bottomright);


			bool top_hits = Intersects(seg, rect_top, out Vector2 top_intersect);
			bool bottom_hits = Intersects(seg, rect_bottom, out Vector2 bottom_intersect);
			bool left_hits = Intersects(seg, rect_left, out Vector2 left_intersect);
			bool right_hits = Intersects(seg, rect_right, out Vector2 right_intersect);

			if (top_hits || bottom_hits || left_hits || right_hits)
            {
				intersection = seg.B;

				if (top_hits && seg.A.Distance(top_intersect) < seg.A.Distance(intersection))
                {
					intersection = top_intersect;
					collidingface = Face.Top;
				}
					
				if (bottom_hits && seg.A.Distance(bottom_intersect) < seg.A.Distance(intersection))
                {
					intersection = bottom_intersect;
					collidingface = Face.Bottom;
				}
					
				if (left_hits && seg.A.Distance(left_intersect) < seg.A.Distance(intersection))
                {
					intersection = left_intersect;
					collidingface = Face.Left;
				}
					
				if (right_hits && seg.A.Distance(right_intersect) < seg.A.Distance(intersection))
                {
					intersection = right_intersect;
					collidingface = Face.Right;
				}
				return true;
			}
			return false;
		}

		public static bool Intersects(LineSegment s1, LineSegment s2, out Vector2 intersection) => Intersects(s1.A, s1.B, s2.A, s2.B, out intersection);


		public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
		{
			intersection = Vector2.Zero;

			Vector2 b = a2 - a1;
			Vector2 d = b2 - b1;
			float bDotDPerp = b.X * d.Y - b.Y * d.X;

			
			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if (bDotDPerp == 0)
				return false;

			Vector2 c = b1 - a1;
			float t = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
			if (t < 0 || t > 1)
				return false;

			float u = (c.X * b.Y - c.Y * b.X) / bDotDPerp;
			if (u < 0 || u > 1)
				return false;

			intersection = a1 + t * b;

			return true;
		}


		public static bool CheckAABB(Vector2 posA, Vector2 sizeA, Vector2 posB, Vector2 sizeB)
		{
			float absDistanceX = Math.Abs(posA.X - posB.X);
			float absDistanceY = Math.Abs(posA.Y - posB.Y);

			float sumWidth = sizeA.X + sizeB.X;
			float sumHeight = sizeA.Y + sizeB.Y;

			if (absDistanceY >= sumHeight || absDistanceX >= sumWidth)
			{
				return false;
			}
			return true;
		}

		public static Vector2 GetSeparationAABB(Vector2 posA, Vector2 sizeA, Vector2 posB, Vector2 sizeB)
		{
			float distanceX = posA.X - posB.X;
			float distanceY = posA.Y - posB.Y;

			float absDistanceX = Math.Abs(distanceX);
			float absDistanceY = Math.Abs(distanceY);

			float sumWidth = sizeA.X + sizeB.X;
			float sumHeight = sizeA.Y + sizeB.Y;

			float sx = sumWidth - absDistanceX;
			float sy = sumHeight - absDistanceY;


			if (sx > sy)
			{
				if (sy > 0)
				{
					sx = 0;
				}
			}
			else
			{
				if (sx > 0)
				{
					sy = 0;
				}
			}

			if (distanceX < 0)
			{
				sx = -sx;
			}
			if (distanceY < 0)
			{
				sy = -sy;
			}

			return new Vector2(sx, sy);
		}

		public static Vector2 GetNormalAABB(Vector2 separation, Vector2 velocity)
		{
			float d = (float)Math.Sqrt(separation.X * separation.X + separation.Y * separation.Y);

			float nx = separation.X / d;
			float ny = separation.Y / d;

			float ps = velocity.X * nx + velocity.Y * ny;

			// maybe needed?
			if (ps <= 0)
			{
				return new Vector2(nx, ny);
			}
			return new Vector2(0, 0);

		}
	}
}
