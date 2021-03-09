using CaveGame.Core;
using DataManagement;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client
{
    public class Sky
    {
		public LocalWorld World { get; private set; }


		public Color SkyColor
		{
			get
			{
				int wrapped = ((int)Math.Floor(World.TimeOfDay)).Mod(24);
				int last = ((int)Math.Floor(World.TimeOfDay) - 1).Mod(24);
				float diff = World.TimeOfDay % 1;
				return Color.Lerp(SkyColors[last], SkyColors[wrapped], diff);
			}
		}

		public static Color[] SkyColors =
		{
			new Color(0, 2, 6), new Color(5, 5, 30), //0 or 24
			new Color(2, 2, 10), new Color(16, 16, 40), //2
			new Color(2, 2, 10), new Color(20, 20, 45), //4
			new Color(8, 9, 50), new Color(85, 85, 40),  //6
			new Color(40, 60, 90), new Color(90, 90, 190), //8
			new Color(70, 90, 130), new Color(110, 110, 230), //10
			new Color(70, 80, 170), new Color(170, 170, 255), //12
			new Color(80, 100, 140), new Color(140, 140, 250), //14
			new Color(35, 41, 60), new Color(60, 80, 140), //14
			new Color(50, 32, 50), new Color(170, 100, 70), // 18
			new Color(25, 25, 55), new Color(92, 52, 23), //20
			new Color(5, 7, 14),  new Color(9, 23, 45), //22
		};

		public Sky(LocalWorld world)
        {
			World = world;
		}


		private void DrawSkyColorGradient(GraphicsEngine GFX)
		{

			if (World.Client.Camera.Position.Y > 600)
			{
				GFX.Rect(Color.Black, Vector2.Zero, World.Client.Camera.WindowSize);
				return;
			}

			for (int y = 0; y < 10; y++)
			{
				int hourTime = (int)Math.Floor(((World.TimeOfDay + 1) % 24) / 2);
				int bottom = hourTime * 2;
				int top = (hourTime * 2) + 1;
				//float diff = World.TimeOfDay % 1;
				var thisSection = Color.Lerp(SkyColors[bottom], SkyColors[top], y / 10.0f);

				int prevhourTime = (int)Math.Floor((World.TimeOfDay % 24) / 2);
				int prevbottom = prevhourTime * 2;
				int prevtop = (prevhourTime * 2) + 1;
				//float diff = World.TimeOfDay % 1;
				var prevSection = Color.Lerp(SkyColors[prevbottom], SkyColors[prevtop], y / 10.0f);

				var finalColor = Color.Lerp(prevSection, thisSection, (World.TimeOfDay % 2.0f) / 2.0f);
				float sliceHeight = World.Client.Camera.WindowSize.Y / 10.0f;
				GFX.Rect(finalColor, new Vector2(0, (sliceHeight * y)), new Vector2(World.Client.Camera.WindowSize.X, sliceHeight + 1));
			}

		}
		Random rng = new Random();
		float rotation = 0;
		
		private void DrawBackgroundParallax(GraphicsEngine GFX)
		{
			if (World.Client.Camera.Position.Y > 600)
			{
				return;
			}

			Vector2 lill = Rotation.FromDeg(World.TimeOfDay * 360.0f).ToUnitVector() * 0.1f;

			float starfieldPar = 0.85f;
			float scale = 0.25f;
			float textureWidth = GFX.Starfield.Width * scale;
			float textureHeight = GFX.Starfield.Height * scale;

			var pos = World.Client.Camera.Position;
			var gridPos = new Vector2(
				(float)pos.X / textureWidth, 
				(float)pos.Y / textureHeight
			) * starfieldPar;

			var textureCenter = new Vector2(GFX.Starfield.Width / 2.0f, GFX.Starfield.Height / 2.0f);


			for (int tx = -3; tx < 3; tx++)
			{
				for (int ty = -3; ty < 3; ty++)
				{
					float xPos = tx + gridPos.X;
					float yPos = ty + gridPos.Y;
					GFX.Sprite(GFX.Starfield, new Vector2(xPos * textureWidth, yPos * textureHeight)+lill, null, Color.White, Rotation.FromDeg((tx+ty).Mod(4)*90), textureCenter, scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
				}
			}


		}

		public void DrawSkyColors(GraphicsEngine GFX) => DrawSkyColorGradient(GFX);
		public void DrawBackground(GraphicsEngine GFX) => DrawBackgroundParallax(GFX);


	}
}
