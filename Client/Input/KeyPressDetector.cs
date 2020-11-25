using CaveGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client.Input
{
	// clipboard support provided by: https://github.com/CopyText/TextCopy
	// Repeat Key press check
	public class KeyPressDetector
	{

		KeyboardState previousKB = Keyboard.GetState();
		KeyboardState currentKB = Keyboard.GetState();
		Keys listenFor;

		const float initialPressRepeatDelay = 0.4f;
		const float repeatDelay = 0.05f;

		float initialPressTimer;
		float repeatTimer;

		bool startRepeating;

		public KeyPressDetector(Keys key)
		{
			listenFor = key;
		}
		//private bool JustPressed(Keys key) => currentKB.IsKeyDown(key) && !previousKB.IsKeyDown(key);
		public bool KeySignal { get; private set; }

		public void Update(GameTime gt)
		{
			currentKB = Keyboard.GetState();

			KeySignal = false;

			if (currentKB.IsKeyDown(listenFor))
			{
				if (initialPressTimer == initialPressRepeatDelay)
				{
					KeySignal = true;
				}

				if (initialPressTimer > 0)
				{
					initialPressTimer -= gt.GetDelta();
				}
				else
				{
					repeatTimer -= gt.GetDelta();

					if (repeatTimer <= 0)
					{
						KeySignal = true;
						repeatTimer = repeatDelay;
					}
				}
			}
			else
			{
				initialPressTimer = initialPressRepeatDelay;
				repeatTimer = repeatDelay;
				KeySignal = false;
			}


			previousKB = currentKB;
		}
	}
}
