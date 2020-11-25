using CaveGame.Client.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client.Menu
{
	public class TimeoutMenu : IGameContext
	{
		public CaveGameGL Game { get; set; }
		Microsoft.Xna.Framework.Game IGameContext.Game => Game;

		UIRoot TimeoutPage;

		public bool Active { get; set; }

		public string Message { get; set; }

		public TimeoutMenu(CaveGameGL _game)
		{
			Game = _game;
		}

		private void ConstructUIElements()
		{
			TimeoutPage = new UIRoot();

			Label message = new Label
			{
				BGColor = Color.Transparent,
				BorderColor = Color.Transparent,
				Size = new UICoords(0, 0, 1.0f, 0.1f),
				AnchorPoint = new Vector2(0.5f, 0.5f),
				Position = new UICoords(0, 0, 0.5f, 0.3f),
				Parent = TimeoutPage,
				Text = Message,
				TextColor = Color.White,
				Font = GameFonts.Arial16,
				BorderSize = 0,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
			};


			TextButton back = new TextButton
			{
				Parent = TimeoutPage,
				Size = new UICoords(200, 30, 0, 0),
				AnchorPoint = new Vector2(0.5f, 0.5f),
				Position = new UICoords(0, 0, 0.5f, 0.7f),
				Text = "Back",
				Font = GameFonts.Arial10,
				BorderSize = 0,
				TextColor = Color.White,
				TextWrap = false,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,

				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			back.OnMouseEnter += WhenMouseOverButton;
			back.OnMouseExit += WhenMouseOffButton;
			back.OnLeftClick += (b, m) => Game.CurrentGameContext = Game.HomePageContext;
		}

		public void Load()
		{
			if (TimeoutPage == null)
				ConstructUIElements();
		}

		private void WhenMouseOverButton(TextButton b, MouseState m) {
			GameSounds.MenuBlip?.Play(1.0f, 1, 0.0f);
		}
		private void WhenMouseOffButton(TextButton b, MouseState m)
		{
			GameSounds.MenuBlip?.Play(0.8f, 1, 0.0f);
		}

		public void Unload()
		{
		}

		public void Update(GameTime gt)
		{
			TimeoutPage.Update(gt);
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Begin();
			TimeoutPage.Draw(sb);
			sb.End();
		}
	}
}
