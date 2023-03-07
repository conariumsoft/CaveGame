using CaveGame.Common;
using CaveGame.Common.Extensions;
using Microsoft.Xna.Framework;

namespace CaveGame.Client.UI
{
	public struct UICoords
	{
		public float PixelX;
		public float PixelY;
		public float ScaleX;
		public float ScaleY;


		public static UICoords operator  +(UICoords a, UICoords b)
        {
			return new UICoords(
				a.PixelX + b.PixelX,
				a.PixelY + b.PixelY,
				a.ScaleX + b.ScaleX,
				a.ScaleY + b.ScaleY
			);
        }

		public Vector2 Scale
		{
			get
			{
				return new Vector2(ScaleX, ScaleY);
			}
			set
			{
				ScaleX = value.X;
				ScaleY = value.Y;
			}
		}

		public Vector2 Pixels
		{
			get { return new Vector2(PixelX, PixelY); }
			set
			{
				ScaleX = value.X;
				ScaleY = value.Y;
			}
		}


		public UICoords(float pixelX, float pixelY, float scaleX = 0, float scaleY = 0)
		{
			PixelX = pixelX;
			PixelY = pixelY;
			ScaleX = scaleX;
			ScaleY = scaleY;
		}

		public static UICoords FromScale(float x, float y)
		{
			return new UICoords(0, 0, x, y);
		}

		public static UICoords FromPixels(int x, int y)
		{
			return new UICoords(x, y, 0, 0);
		}

		public UICoords Lerp(UICoords b, float t)
		{
			return UICoords.Lerp(this, b, t);
		}

		public static UICoords Lerp(UICoords a, UICoords b, float t)
		{
			return new UICoords(
				MathematicsExtensions.Lerp(a.Pixels.X, b.Pixels.X, t),
				MathematicsExtensions.Lerp(a.Pixels.Y, b.Pixels.Y, t),
				MathematicsExtensions.Lerp(a.Scale.X, b.Scale.X, t),
				MathematicsExtensions.Lerp(a.Scale.Y, b.Scale.Y, t)
			);
		}

	}
}
