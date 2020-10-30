using CaveGame.Client.UI;
using CaveGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client.Menu
{
	public class Credits : IGameContext
	{
		private static List<string> credits = new List<string>
		{
			">>CAVE GAME",
			"",
			">Lead Developer",
			"Josh O'Leary 'MadScientist'",
			"",
			">Contributing Developers",
			"dodeadam - Programming",
			"ConcurrentSquared - Programming & Design",
			"Mescalyne - Music",
			"WheezyBackports - Art",
			"Bumpylegoman02 - Security Testing & Design",
			"",
			">Testing",
			"Andrew J.",
			"squidthonkv2",
			"Billy J.",
			"",
			">Biz",
			"Tyler Stewart",
			"",
			"Copyright Conarium Software 2020",
		};

		Game IGameContext.Game => Game;
		public CaveGameGL Game { get; set; }
		public bool Active { get; set; }

		static UIRoot CreditsPage;

		

		private void ConstructUIElements()
		{
			CreditsPage = new UIRoot(Game.GraphicsDevice);

			UIRect creditslist = new UIRect
			{
				Size = new UICoords(0, 0, 1.0f, 1.0f),
				Position = new UICoords(0, 0, 0, 0),
				Parent = CreditsPage,
				BGColor = Color.Black*0.5f,
			};

			TextButton backButton = new TextButton
			{
				TextColor = Color.White,
				Text = "BACK",
				Font = GameFonts.Arial16,
				Size = new UICoords(100, 30, 0, 0),
				Position = new UICoords(10, -30, 0, 1.0f),
				AnchorPoint = new Vector2(0, 1),
				TextWrap = true,
				TextYAlign = TextYAlignment.Center,
				TextXAlign = TextXAlignment.Center,
				Parent = creditslist,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			}; 
			backButton.OnLeftClick += (btn, mouse) => Game.CurrentGameContext = Game.HomePageContext;

			UIListContainer container = new UIListContainer
			{
				Padding = 0,
				Parent = creditslist,
			};

			foreach(string text in credits)
			{
				string displayedText = text;
				SpriteFont font = GameFonts.Arial14;
				int size = 16;
				if (text.StartsWith(">>"))
				{
					font = GameFonts.Arial20;
					size = 24;
					displayedText = text.Replace(">>", "");
				} else if (text.StartsWith(">"))
				{
					font = GameFonts.Arial16;
					size = 20;
					displayedText = text.Replace(">", "");
				}
				

				var label = new Label
				{
					TextColor = UITheme.SmallButtonTextColor,
					Text = displayedText,
					Font = font,
					Size = new UICoords(1, size, 1.0f, 0),
					BGColor = Color.Black * 0.0f,
					TextXAlign = TextXAlignment.Center,
					Parent = container,
				};
			}

		}

		public Credits(CaveGameGL _game)
		{
			Game = _game;
		}


		public void Draw(SpriteBatch sb)
		{
			sb.Begin();
			CreditsPage.Draw(sb);
			sb.End();
		}

		public void Load()
		{
			if (CreditsPage == null)
				ConstructUIElements();
		}

		public void Unload()
		{

		}

		public void Update(GameTime gt)
		{
			CreditsPage.Update(gt);
		}
	}
}
