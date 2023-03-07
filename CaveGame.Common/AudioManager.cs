using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common
{
	public class AudioManager
	{
		public static Dictionary<string, Song> Songs = new Dictionary<string, Song>();
		public static Dictionary<string, SoundEffect> Effects = new Dictionary<string, SoundEffect>();

		public static void RegisterEffect(string effectname, SoundEffect effect)
		{
			Effects.Add(effectname, effect);
		}

		public static void RegisterSong(string songname, Song song)
		{
			Songs.Add(songname, song);
		}

		public static void PlaySong(string songname)
		{
			MediaPlayer.Volume = (MasterVolume * MusicVolume);
			MediaPlayer.Play(Songs[songname]);
		}

		public static void PlayEffect(string effectname, float volume = 1, float pitch = 1, float pan = 0)
		{
			var effect = Effects[effectname].CreateInstance();
			effect.Volume = (MasterVolume*EffectVolume*volume);
			effect.Pitch = pitch;
			effect.Pan = pan;
			effect.Play();
		}

		public static float MasterVolume { get; set; } = 1;
		public static float MusicVolume { get; set; } = 0.1f;
		public static float EffectVolume { get; set; } = 1;
	}
}
