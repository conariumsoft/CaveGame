using CaveGame.Common;
using KeraLua;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CaveGame.Client
{
	public static class GameSounds
	{
		public static SoundEffect MenuBlip { get; private set; }
		public static SoundEffect MenuBlip2 { get; private set; }

		public static Song Mu_Big_Brother { get; private set; }
		public static Song Mu_Cliff { get; private set; }
		public static Song Mu_Hey_Bella { get; private set; }
		public static Song Mu_Mithril_Ocean { get; private set; }

		public static Song AmbientLava { get; private set; }
		public static Song AmbientBirds1 { get; private set; }
		public static Song AmbientBirds2 { get; private set; }
		public static Song AmbientBirds3 { get; private set; }
		public static Song AmbientBirds4 { get; private set; }
		public static Song AmbientBirds5 { get; private set; }
		public static Song AmbientBirds6 { get; private set; }
		public static Song AmbientBirds7 { get; private set; }
		public static Song AmbientCreepy1 { get; private set; }
		public static Song AmbientCreepy2 { get; private set; }
		public static Song AmbientCreepy3 { get; private set; }
		public static Song AmbientCreepy4 { get; private set; }

		public static void LoadAssets(ContentManager Content)
		{
			Content.RootDirectory = Path.Combine("Assets", "Sound");
			MenuBlip = Content.Load<SoundEffect>("click1");

			Mu_Big_Brother = Content.Load<Song>("mu/big_brother");
			Mu_Cliff = Content.Load<Song>("mu/cliff");
			Mu_Hey_Bella = Content.Load<Song>("mu/hey_bella");
			Mu_Mithril_Ocean = Content.Load<Song>("mu/mithril_ocean");
			AmbientLava = Content.Load<Song>("ambient/lava");
			AmbientBirds1 = Content.Load<Song>("ambient/birds1");
			AmbientBirds2 = Content.Load<Song>("ambient/birds2");
			AmbientBirds3 = Content.Load<Song>("ambient/birds3");
			AmbientBirds4 = Content.Load<Song>("ambient/birds4");
			AmbientBirds5 = Content.Load<Song>("ambient/birds5");
			AmbientBirds6 = Content.Load<Song>("ambient/birds6");
			AmbientBirds7 = Content.Load<Song>("ambient/birds7");
			AmbientCreepy1 = Content.Load<Song>("ambient/birds1");
			AmbientBirds7 = Content.Load<Song>("ambient/birds1");
		}
	}
}
