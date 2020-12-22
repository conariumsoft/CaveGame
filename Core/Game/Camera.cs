using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CaveGame.Core
{

    public class Camera2D
    {
        public float Zoom { get; set; }
        public Vector2 OutputPosition { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }

        public Vector2 WindowSize => GraphicsEngine.Instance.WindowSize;

        public Vector2 _screenSize { get; set; }

        public Vector2 TopLeft
        {
            get { return Position; }
        }

        public Vector2 ShakeVector { get; set; }

        public void Shake(float x, float y)
        {
            ShakeVector += new Vector2(x, y);
        }

        public Vector2 WorldToScreenCoordinates(Vector2 worldCoords)
        {
            return Vector2.Transform(worldCoords, View);
        }

        public Vector2 ScreenToWorldCoordinates(Vector2 screenCoords)
        {
            return Vector2.Transform(screenCoords, Matrix.Invert(View));
        }

        public Vector2 ScreenCenterToWorldSpace
        {
            get { return Position; }
        }
        public Matrix View
        {
            get
            {
                return
                    Matrix.CreateTranslation(new Vector3(-OutputPosition.X, -OutputPosition.Y, 0)) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(Zoom) *
                    Matrix.CreateTranslation(new Vector3(WindowSize.X * 0.5f, WindowSize.Y * 0.5f, 0));
            }
        }

        public Rectangle VisibleArea
        {
            get
            {
                var inverseViewMatrix = Matrix.Invert(View);
                var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
                var tr = Vector2.Transform(new Vector2(WindowSize.X, 0), inverseViewMatrix);
                var bl = Vector2.Transform(new Vector2(0, WindowSize.Y), inverseViewMatrix);
                var br = Vector2.Transform(_screenSize, inverseViewMatrix);
                var min = new Vector2(
                    MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
                    MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
                var max = new Vector2(
                    MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
                    MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
                return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
            }
        }

        public Camera2D()
        {
            Rotation = 0;
            Zoom = 1;
            Position = new Vector2(0, 0);
        }
        Random rng = new Random();
        public void Update(GameTime gt)
        {
            ShakeVector *= 1.0f - (gt.GetDelta() * 2.0f);

            if (ShakeVector.Length() > 1f)
            {
                float cap = 100;
                float x = (float)(rng.NextDouble() - 0.5) * Math.Min(ShakeVector.X, cap);
                float y = (float)(rng.NextDouble() - 0.5) * Math.Min(ShakeVector.Y, cap);

                OutputPosition = Position + new Vector2(x, y);
            }
            else
            {
                OutputPosition = Position;
            }
        }
    }
}
