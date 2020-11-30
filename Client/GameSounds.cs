using CaveGame.Core;
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
			MenuBlip = Content.Load<SoundEffect>("Sound/click1");
			MenuBlip2 = Content.Load<SoundEffect>("Sound/menu1");
			/*AmbientLava = Content.Load<Song>("Sound/ambient/lava");
			AmbientBirds1 = Content.Load<Song>("Sound/ambient/birds1");
			AmbientBirds2 = Content.Load<Song>("Sound/ambient/birds2");
			AmbientBirds3 = Content.Load<Song>("Sound/ambient/birds3");
			AmbientBirds4 = Content.Load<Song>("Sound/ambient/birds4");
			AmbientBirds5 = Content.Load<Song>("Sound/ambient/birds5");
			AmbientBirds6 = Content.Load<Song>("Sound/ambient/birds6");
			AmbientBirds7 = Content.Load<Song>("Sound/ambient/birds7");
			AmbientCreepy1 = Content.Load<Song>("Sound/ambient/birds1");
			AmbientBirds7 = Content.Load<Song>("Sound/ambient/birds1");*/
		}
	}
}
