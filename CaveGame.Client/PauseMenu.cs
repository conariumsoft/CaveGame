using CaveGame.Client.UI;
using CaveGame.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CaveGame.Client
{
    public class PauseMenu
    {
		public PauseMenu(GameClient client)
        {
			Client = client;

        }
		public GameClient Client { get; private set; }
		public UIRoot Menu { get; private set; }

		public bool Open { get; set; }

		Effect WaterpixelsShader { get; set; }

		public void DrawWaterPixelsFilter(GraphicsEngine gfx)
        {
			WaterpixelsShader.Parameters["xSize"].SetValue((float)256);
			WaterpixelsShader.Parameters["ySize"].SetValue((float)256);
			WaterpixelsShader.Parameters["xDraw"].SetValue((float)16);
			WaterpixelsShader.Parameters["yDraw"].SetValue((float)16);
			//effect.Parameters["filterColor"].SetValue(Color.White.ToVector4());
			WaterpixelsShader.CurrentTechnique.Passes[0].Apply();
		}

		public void LoadShader(ContentManager GameContent)
        {
			GameContent.RootDirectory = Path.Combine("Assets", "Shaders");
			WaterpixelsShader = GameContent.Load<Effect>("Waterpixels");
        }

		public void Update(GameTime gt) {

			if (Menu!=null && Open)
				Menu.Update(gt);
		}


		public void Draw(GraphicsEngine gfx)
        {
			if (Menu==null)
				ConstructPauseMenu(gfx);

			if (Open)
				Menu.Draw(gfx);
        }

		private void ConstructPauseMenu(GraphicsEngine gfx)
		{
			Menu = new UIRoot();

			UIRect bg = new UIRect
			{
				BGColor = Color.Transparent,
				Size = new UICoords(0, 0, 1, 1),
				Position = new UICoords(0, 0, 0, 0),
				Parent = Menu
			};

			TextButton resumeButton = new TextButton
			{
				Parent = bg,
				TextColor = Color.White,
				Text = "Resume",
				Font = gfx.Fonts.Arial14,
				Size = new UICoords(150, 25, 0, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Position = new UICoords(10, -100, 0, 1)
			};
			resumeButton.OnLeftClick += (x, y) => Open = false;

			TextButton exitButton = new TextButton
			{
				Parent = bg,
				TextColor = Color.White,
				Text = "Disconnect",
				Font = gfx.Fonts.Arial14,
				Size = new UICoords(150, 25, 0, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Position = new UICoords(10, -50, 0, 1)
			};
			exitButton.OnLeftClick += TryClientExit;
		}


		public void TryClientExit(TextButton tbtn, MouseState ms)
        {
			Client.Disconnect();
        }
	}
}
