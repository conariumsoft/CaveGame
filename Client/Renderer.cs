using Cave;
using CaveGame.Core.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaveGame.Client
{

	//public class RenderComponent
	//{
	//	Texture2D pixel;
	//	public RenderComponent(Game game)
	//	{

	//	}
	//	public void Draw(Player player)
		//{
			//sb.Print(Color.Black, player.TopLeft - new Vector2(0, 10), "YOU");
		//	sb.Rect(Color.Red, player.TopLeft, player.BoundingBox * 2);
		//	base.Draw(sb);
		//}
	//}

	using Circle = List<Vector2>;
	using Arc = List<Vector2>;

	static class ShapeCache
	{

		private static readonly Dictionary<String, Circle> circleCache = new Dictionary<string, Circle>();

		public static Arc GetArc(float radius, int sides, float startingAngle, float radians)
		{
			Arc points = new Arc();

			points.AddRange(GetCircle(radius, sides));
			points.RemoveAt(points.Count - 1);

			double curAngle = 0.0;
			double anglePerSide = MathHelper.TwoPi / sides;

			while ((curAngle + (anglePerSide / 2.0)) < startingAngle)
			{
				curAngle += anglePerSide;

				points.Add(points[0]);
				points.RemoveAt(0);
			}

			points.Add(points[0]);
			int sidesInArc = (int)((radians / anglePerSide) + 0.5);

			points.RemoveRange(sidesInArc + 1, points.Count - sidesInArc - 1);

			return points;
		}

		public static Circle GetCircle(double radius, int sides)
		{
			String circleKey = radius + "x" + sides;

			if (circleCache.ContainsKey(circleKey))
				return circleCache[circleKey];

			Circle circleDef = new Circle();

			const double max = 2.0 * Math.PI;

			double step = max / sides;

			for (double theta = 0.0; theta < max; theta += step)
			{
				circleDef.Add(new Vector2( (float) (radius * Math.Cos(theta)), (float)(radius * Math.Sin(theta))));
			}

			circleDef.Add(new Vector2((float)(radius * Math.Cos(0)), (float)(radius * Math.Sin(0))));

			circleCache.Add(circleKey, circleDef);

			return circleDef;
		}
	}

	public static class Renderer
	{



		static Texture2D _pixel;
		static Game _game;
		static BasicEffect _effect3D;


		public static void Initialize(Game game)
		{
			_game = game;

			


			_pixel = new Texture2D(game.GraphicsDevice, 1, 1);
			_pixel.SetData<Color>(new Color[] { Color.White });
			_effect3D = new BasicEffect(game.GraphicsDevice);
		}



		public static void OutlineRect(this SpriteBatch sb, Color color, Vector2 position, Vector2 size, float thickness = 2.0f)
		{
			Line(sb, color, position, position + new Vector2(0, size.Y), thickness);
			Line(sb, color, position, position + new Vector2(size.X, 0), thickness);
			Line(sb, color, position + new Vector2(size.X, 0), position + size, thickness);
			Line(sb, color, position + new Vector2(0, size.Y), position + new Vector2(size.X, size.Y), thickness);
		}
		public static void Rect(this SpriteBatch sb, Color color, Vector2 position, Vector2 size, float rotation = 0)
		{
			Rect(sb, color, (int)position.X, (int)position.Y, (int)size.X, (int)size.Y, rotation);
		}
		public static void Rect(this SpriteBatch sb, Color color, int x, int y, int width, int height, float rotation = 0)
		{
			sb.Draw(
				_pixel,
				new Rectangle(x, y, width, height),
				null,
				color, rotation, new Vector2(0, 0), SpriteEffects.None, 0
			);
			// retardretardretardretardretardretard
		}
		public static void Line(this SpriteBatch spriteBatch, Color color, Vector2 point1, Vector2 point2, float thickness = 1f)
		{
			float distance = Vector2.Distance(point1, point2);
			float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

			float expanded = (float)Math.Floor(angle * Math.PI);
			float backDown = expanded / (float)Math.PI;

			Line(spriteBatch, color, point1, distance, angle, thickness);
		}
		public static void Line(this SpriteBatch sb, Color color, Vector2 point, float length, float angle, float thickness = 1f)
		{
			Vector2 origin = new Vector2(0f, 0.5f);
			Vector2 scale = new Vector2(length, thickness);
			sb.Draw(_pixel, point, null, color, angle, origin, scale, SpriteEffects.None, 0);
		}
		public static void Points(this SpriteBatch sb, Color color, List<Vector2> points, float thickness = 1f)
		{
			if (points.Count < 2)
				return;

			for (int i = 1; i < points.Count; i++)
			{
				Line(sb, color, points[i - 1], points[i], thickness);
			}
		}
		public static void Points(this SpriteBatch sb, Color color, Vector2 position, List<Vector2> points, float thickness = 1f)
		{
			if (points.Count < 2)
				return;

			for (int i = 1; i < points.Count; i++)
			{
				Line(sb, color, points[i - 1]+position, points[i]+position, thickness);
			}
		}
		public static void Circle(this SpriteBatch sb, Color color, Vector2 position, double radius, int sides = 12, float thickness = 1f)
		{
			Circle c = ShapeCache.GetCircle(radius, sides);
			Points(sb, color, position, c, thickness);
		}
		public static void Arc(this SpriteBatch sb, Color color, Vector2 center, float radius, int sides, float startingAngle, float radians, float thickness = 1f)
		{
			Arc arc = ShapeCache.GetArc(radius, sides, startingAngle, radians);
			Points(sb, color, center, arc, thickness);
		}
		public static void Print(this SpriteBatch sb, Color color, Vector2 position, string text)
		{
			Print(sb, GameFonts.Arial10, color, position, text);
		}
		public static void Print(this SpriteBatch sb, SpriteFont font, Color color, Vector2 position, string text)
		{
			sb.DrawString(font, text, position, color);
		}
		/*public static void Line3D(this SpriteBatch sb, Camera camera, Vector3 pointA, Vector3 pointB, Color color)
		{
			Line3D(camera, pointA, pointB, color, color);
		}
		public static void Line3D(this SpriteBatch sb, Camera camera, Vector3 pointA, Vector3 pointB, Color colorA, Color colorB)
		{
			Line3D(camera, pointA, pointB, colorA, colorB);
		}
		public static void Line3D(Camera camera, Vector3 pointA, Vector3 pointB, Color color)
		{
			Line3D(camera, pointA, pointB, color, color);
		}
		public static void Line3D(Camera camera, Vector3 pointA, Vector3 pointB, Color colorA, Color colorB)
		{
			_effect3D.View = camera.View;
			_effect3D.Projection = camera.Projection;

			_effect3D.CurrentTechnique.Passes[0].Apply();

			var vertices = new[] { new VertexPositionColor(pointA, colorA), new VertexPositionColor(pointB, colorB)};
			_game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
		}*/
	}
}
