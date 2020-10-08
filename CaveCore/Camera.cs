using CaveGame.Core;
using CaveGame.Core.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
    public class Camera2D
    {
        public float Zoom { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }

        public Rectangle Bounds { get; set; }

        public Vector2 _screenSize;

        public Vector2 TopLeft
		{
            get { return Position; }
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
            get {
                return
                    Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(Zoom) *
                    Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0));
            }
        }

        public Rectangle VisibleArea
        {
            get
            {
                var inverseViewMatrix = Matrix.Invert(View);
                var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
                var tr = Vector2.Transform(new Vector2(_screenSize.X, 0), inverseViewMatrix);
                var bl = Vector2.Transform(new Vector2(0, _screenSize.Y), inverseViewMatrix);
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

        public Camera2D(Viewport viewport) 
        {
            Bounds = viewport.Bounds;
            _screenSize = new Vector2(Bounds.Width, Bounds.Height);
            Rotation = 0;
            Zoom = 1;
            Position = new Vector2(0, 0);
        }
    }
}
