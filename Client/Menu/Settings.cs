using CaveGame.Client.UI;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CaveGame.Client.Menu
{
	// settings:
	// Video
	// fullscreen (boolean toggle)
	// screen resolution (a selection list)
	// max particles (particles off)
	// Max FPS (Unlimited, 240, 144, 120, 90, 60, 30)
	// Audio
	// Music Volume
	// Menu Volume
	// Game Master Volume
	// Player Volume
	// World Volume
	// Entity Volume
	public class BindButton : TextButton
	{
		public event KeysHandler OnRebind;
	
		public delegate void KeysHandler(Keys key);

		public void Rebind(Keys key)
		{
			OnRebind?.Invoke(key);
		}

	}

	public class Settings: IGameContext
	{

		UIRoot SettingsUI;

		public Settings(CaveGameGL _game)
		{
			Game = _game;
		}

		public CaveGameGL Game { get; set; }
		Game IGameContext.Game => Game;


		public bool Active { get; set; }

		public void Draw(SpriteBatch sb)
		{
			sb.Begin();
			SettingsUI.Draw(sb);
			sb.End();
		}


		BindButton jumpKeybindButton;
		BindButton upKeybindButton;

		private void ConstructUIElements()
		{
			string ReadableBoolean(bool b)
			{
				if (b)
					return "On";
				else
					return "Off";
			}

			SettingsUI = new UIRoot(Game.GraphicsDevice);

			Label title = new Label
			{
				BGColor = Color.Transparent,
				BorderColor = Color.Transparent,
				Size = new UICoords(0, 0, 0.3f, 0.1f),
				AnchorPoint = new Vector2(0.5f, 0.5f),
				Position = new UICoords(0, 0, 0.5f, 0.05f),
				Parent = SettingsUI,
				TextColor = Color.White,
				Text = "Settings",
				Font = GameFonts.Arial20,
				BorderSize = 0,
				TextWrap = false,
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
			};

			UIRect optionList = new UIRect
			{
				Parent = SettingsUI,
				Size = new UICoords(0, 0, 0.6f, 0.7f),
				Position = new UICoords(0, 0, 0.5f, 0.5f),
				AnchorPoint = new Vector2(0.5f, 0.5f),
				BGColor = Color.Transparent,
			};

			UIListContainer container = new UIListContainer
			{
				Padding = 2,
				Parent = optionList,
			};

			TextButton fullscreenBtn = new TextButton
			{
				Parent = container,
				TextColor = Color.White,
				Text = "Fullscreen: "+ReadableBoolean(CaveGameGL.GameSettings.Fullscreen),
				Font = GameFonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			void updateFullscreenBnt(TextButton b, MouseState m)
			{
				CaveGameGL.GameSettings.Fullscreen = !CaveGameGL.GameSettings.Fullscreen;
				fullscreenBtn.Text = "Fullscreen: " + ReadableBoolean(CaveGameGL.GameSettings.Fullscreen);
				CaveGameGL.GameSettings.Save();
				Game.OnSetFullscreen(CaveGameGL.GameSettings.Fullscreen);
				
			}
			fullscreenBtn.OnLeftClick += updateFullscreenBnt;

			TextButton particlesBtn = new TextButton
			{
				Parent = container,
				TextColor = Color.White,
				Text = "Particles: " + ReadableBoolean(CaveGameGL.GameSettings.Particles),
				Font = GameFonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			void updateParticles(TextButton b, MouseState m)
			{
				particlesBtn.Text = "Particles: " + ReadableBoolean(CaveGameGL.GameSettings.Particles);
				CaveGameGL.GameSettings.Particles = !CaveGameGL.GameSettings.Particles;
				CaveGameGL.GameSettings.Save();
			}
			particlesBtn.OnLeftClick += updateParticles;




			Label fpsCapText = new Label
			{
				TextColor = Color.White,
				Parent = container,
				Size = new UICoords(0, 25, 1, 0),
				Font = GameFonts.Arial14,
				Text = "Framerate Cap: " + GameSettingsData.FPSSlider[CaveGameGL.GameSettings.FPSCapIndex].Display,
			};

			Slider<SliderIndex<int>> fpsCapSlider = new UI.Slider<SliderIndex<int>>
			{
				DataSet = GameSettingsData.FPSSlider,
				Parent = container,
				Size = new UICoords(0, 25, 0.5f, 0),
				AnchorPoint = new Vector2(0.0f, 0.0f),
				UnselectedBGColor = new Color(0.6f, 0.6f, 0.6f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Scrubber = new Scrubber { Width=20},
				BGColor = new Color(0.25f, 0.25f, 0.25f),
			};
			void onFpsCapSliderChanged(Slider<SliderIndex<int>> sl, SliderIndex<int> val, int index)
			{
				CaveGameGL.GameSettings.FPSCapIndex = index;
				fpsCapText.Text = "FPS Cap:" + val.Display;
				Game.OnSetFPSLimit(val.Value);
				GameSounds.MenuBlip?.Play(0.8f, 1, 0.0f);
			}
			fpsCapSlider.OnValueChanged += onFpsCapSliderChanged;
			fpsCapSlider.SetIndex(CaveGameGL.GameSettings.FPSCapIndex);

			void bindButtonClick(TextButton b, MouseState m)
			{
				rebinding = (BindButton)b;
			}


			void jumpRebind(Keys key)
			{
				CaveGameGL.GameSettings.JumpKey = key;
				jumpKeybindButton.Text = "Jump: " + CaveGameGL.GameSettings.JumpKey.ToString();
			}


			jumpKeybindButton = new BindButton
			{
				TextColor = Color.White,
				Text = "Jump: " + CaveGameGL.GameSettings.JumpKey.ToString(),
				Font = GameFonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Parent = container,
			};
			jumpKeybindButton.OnLeftClick += bindButtonClick;
			jumpKeybindButton.OnRebind += jumpRebind;


			void upRebind(Keys key)
			{
				CaveGameGL.GameSettings.MoveUpKey = key;
				upKeybindButton.Text = "Climb/Up: " + CaveGameGL.GameSettings.MoveUpKey.ToString();
			}

			upKeybindButton = new BindButton
			{
				TextColor = Color.White,
				Text = "Climb/Up: " + CaveGameGL.GameSettings.MoveUpKey.ToString(),
				Font = GameFonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Parent = container,
			};
			upKeybindButton.OnLeftClick += bindButtonClick;
			upKeybindButton.OnRebind += upRebind;

			

			BindButton downKeybindButton = new BindButton
			{
				TextColor = Color.White,
				Text = "Descend/Down: " + CaveGameGL.GameSettings.MoveDownKey.ToString(),
				Font = GameFonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Parent = container,
			};
			void downRebind(Keys key)
			{
				CaveGameGL.GameSettings.MoveDownKey = key;
				downKeybindButton.Text = "Descend/Down: " + CaveGameGL.GameSettings.MoveDownKey.ToString();
			}
			downKeybindButton.OnLeftClick += bindButtonClick;
			downKeybindButton.OnRebind += downRebind;


			BindButton leftKeybindButton = new BindButton
			{
				TextColor = Color.White,
				Text = "Walk Left: " + CaveGameGL.GameSettings.MoveLeftKey.ToString(),
				Font = GameFonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Parent = container,
			};
			void leftRebind(Keys key)
			{
				CaveGameGL.GameSettings.MoveLeftKey = key;
				leftKeybindButton.Text = "Walk Left: " + CaveGameGL.GameSettings.MoveLeftKey.ToString();
			}
			leftKeybindButton.OnLeftClick += bindButtonClick;
			leftKeybindButton.OnRebind += leftRebind;

			BindButton rightKeybindButton = new BindButton
			{
				TextColor = Color.White,
				Text = "Walk Right: " + CaveGameGL.GameSettings.MoveRightKey.ToString(),
				Font = GameFonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Parent = container,
			};
			void rightRebind(Keys key)
			{
				CaveGameGL.GameSettings.MoveRightKey = key;
				rightKeybindButton.Text = "Walk Right: " + CaveGameGL.GameSettings.MoveRightKey.ToString();
			}
			rightKeybindButton.OnLeftClick += bindButtonClick;
			rightKeybindButton.OnRebind += rightRebind;

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
				Parent = SettingsUI,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};

			backButton.OnLeftClick += (btn, mouse) => Game.CurrentGameContext = Game.HomePageContext;
		}
		BindButton rebinding;

		public void Load()
		{
			if (SettingsUI == null)
				ConstructUIElements();
		}

		public void Unload()
		{
			
		}

		Keys[] alsoListenFor =
		{
			Keys.LeftShift,
			Keys.LeftControl,
			Keys.RightShift,
			Keys.RightControl,
			Keys.Up, Keys.Left, Keys.Down, Keys.Right
		};

		KeyboardState previousKB = Keyboard.GetState();


		protected void Rebind(Keys newKey)
		{
			if (rebinding != null)
			{
				rebinding.Rebind(newKey);
			}

			rebinding = null;
		}

		public void Update(GameTime gt)
		{
			KeyboardState currentKB = Keyboard.GetState();


			foreach( var key in alsoListenFor)
			{
				if (currentKB.IsKeyDown(key) && !previousKB.IsKeyDown(key))
				{
					Rebind(key);
				}
			}
			

			previousKB = currentKB;

			SettingsUI.Update(gt);
		}

		public void OnTextInput(object sender, TextInputEventArgs args)
		{
			Rebind(args.Key);
		}

	}



}
