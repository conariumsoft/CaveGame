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
			">Programming",
			"JoshMadScientist",
			"ConcurrentSquared",
			"dodeadam",
			">Game Design",
			"JoshMadScientist",
			"dodeadam",
			">Art and Music",
			"Mescalyne",
			"JoshMadScientist",
			"WheezyBackports",
			"Bumpylegoman02",
			">Testing",
			"Bumpylegoman02",
			"WheezyBackports",
			"AndrewJ",
			"squidthonkv2",
			">Community Management",
			"N/A",
			">Biz",
			"Tyler S.",
			"Copyright Conarium Software 2020",
		};

		public Game Game { get; set; }

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
				BGColor = Color.DarkBlue,
			};

			UIListContainer container = new UIListContainer
			{
				Padding = 0,
				Parent = creditslist,
			};

			foreach(string text in credits)
			{
				string displayedText = text;
				SpriteFont font = GameFonts.Arial10;
				int size = 16;
				if (text.StartsWith(">>"))
				{
					font = GameFonts.Arial20;
					size = 24;
					displayedText = text.Replace(">>", "");
				} else if (text.StartsWith(">"))
				{
					font = GameFonts.Arial14;
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

		public Credits(Game _game)
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
