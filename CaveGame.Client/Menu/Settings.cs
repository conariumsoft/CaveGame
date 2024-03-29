﻿using CaveGame.Client.UI;
using CaveGame.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using CaveGame.Client.DesktopGL;

namespace CaveGame.Client.Menu
{
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

		GraphicsEngine GFX => Game.GraphicsEngine;

		public Settings(CaveGameDesktopClient _game)
		{
			Game = _game;
		}

		public CaveGameDesktopClient Game { get; set; }
		Microsoft.Xna.Framework.Game IGameContext.Game => Game;


		public bool Active { get; set; }

		public void Draw(GraphicsEngine GFX)
		{
			GFX.Begin();
			SettingsUI.Draw(GFX);
			GFX.End();
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

			SettingsUI = new UIRoot();

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
				Font = GFX.Fonts.Arial20,
				BorderSize = 0,
				TextWrap = false,
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
			};


			// Left-Side options
			var LeftList = new UIRect
			{
				Parent = SettingsUI,
				Size = new UICoords(0, 0, 0.3f, 0.8f),
				Position = new UICoords(0, 0, 0.6f, 0.2f),
				AnchorPoint = new Vector2(0f, 0f),
				BGColor = Color.Transparent,
			};
			var LeftContainer = new UIListContainer{Padding = 2, Parent = LeftList};

			Label graphicsLbl = new Label
			{
				TextColor = Color.White,
				Parent = LeftContainer,
				Size = new UICoords(0, 25, 1, 0),
				Font = GFX.Fonts.Arial16,
				Text = "GRAPHICS",
				TextXAlign = TextXAlignment.Center,
			};

			TextButton fullscreenBtn = new TextButton
			{
				Parent = LeftContainer,
				TextColor = Color.White,
				Text = "Fullscreen: " + ReadableBoolean(Game.Settings.Fullscreen),
				Font = GFX.Fonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			void updateFullscreenBnt(TextButton b, MouseState m)
			{
				Game.Settings.Fullscreen = !Game.Settings.Fullscreen;
				fullscreenBtn.Text = "Fullscreen: " + ReadableBoolean(Game.Settings.Fullscreen);
			}
			fullscreenBtn.OnLeftClick += updateFullscreenBnt;

			// Particles toggle
			TextButton particlesBtn = new TextButton
			{
				Parent = LeftContainer,
				TextColor = Color.White,
				Text = "Particles: " + ReadableBoolean(Game.Settings.Particles),
				Font = GFX.Fonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			void updateParticles(TextButton b, MouseState m)
			{
				Game.Settings.Particles = !Game.Settings.Particles;
				particlesBtn.Text = "Particles: " + ReadableBoolean(Game.Settings.Particles);
			}
			particlesBtn.OnLeftClick += updateParticles;

			// Framerate Cap
			Label fpsCapText = new Label
			{
				TextColor = Color.White,
				Parent = LeftContainer,
				Size = new UICoords(0, 25, 1, 0),
				Font = GFX.Fonts.Arial14,
				Text = "FPS Cap: " + Game.Settings.FramerateLimit,
			};

			NumericSlider fpsCapSlider = new NumericSlider
			{
				Minimum = 30, Maximum = 241, Interval = 30,

				Parent = LeftContainer,
				Size = new UICoords(0, 25, 0.5f, 0),
				AnchorPoint = new Vector2(0.0f, 0.0f),
				UnselectedBGColor = new Color(0.6f, 0.6f, 0.6f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Scrubber = new Scrubber { Width = 20 },
				BGColor = new Color(0.25f, 0.25f, 0.25f),
			};
			void onFpsCapSliderChanged(NumericSlider slider, float value)
			{
				fpsCapText.Text = "FPS Cap:" + (int)value;
				Game.Settings.FramerateLimit = (int)value;
			}
			fpsCapSlider.OnValueChanged += onFpsCapSliderChanged;

			// Right-Side options
			UIRect RightList = new UIRect
			{
				Parent = SettingsUI,
				Size = new UICoords(0, 0, 0.3f, 0.8f),
				Position = new UICoords(0, 0, 0.1f, 0.2f),
				AnchorPoint = new Vector2(0f, 0f),
				BGColor = Color.Transparent,
			};

			UIListContainer RightContainer = new UIListContainer
			{
				Padding = 2,
				Parent = RightList,
			};
			

			// chat size slider
			Label chatSizeText = new Label
			{
				TextColor = Color.White,
				Parent = LeftContainer,
				Size = new UICoords(0, 25, 1, 0),
				Font = GFX.Fonts.Arial14,
				Text = "Chat Size: " + Game.Settings.ChatSize,
			};
			Slider<SliderIndex<GameChatSize>> chatSizeSlider = new UI.Slider<SliderIndex<GameChatSize>>
			{
				DataSet = GameSettings.ChatSizeSliderOptions,
				Parent = LeftContainer,
				Size = new UICoords(0, 25, 0.5f, 0),
				AnchorPoint = new Vector2(0.0f, 0.0f),
				UnselectedBGColor = new Color(0.6f, 0.6f, 0.6f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Scrubber = new Scrubber { Width = 20 },
				BGColor = new Color(0.25f, 0.25f, 0.25f),
			};
			void onChatSliderChanged(Slider<SliderIndex<GameChatSize>> sl, SliderIndex<GameChatSize> val, int index)
			{
				Game.Settings.ChatSize = val.Value;
				chatSizeText.Text = "Chat Size:" + val.Display;
				GameSounds.MenuBlip?.Play(0.8f, 1, 0.0f);
			}
			chatSizeSlider.OnValueChanged += onChatSliderChanged;


			Label soundLbl = new Label
			{
				TextColor = Color.White,
				Parent = RightContainer,
				Size = new UICoords(0, 25, 1, 0),
				Font = GFX.Fonts.Arial16,
				Text = "SOUND",
				TextXAlign = TextXAlignment.Center,
			};
			Label masterVolumeLabel = new Label
			{
				TextColor = Color.White,
				Parent = RightContainer,
				Size = new UICoords(0, 25, 1, 0),
				Font = GFX.Fonts.Arial14,
				Text = "Master Volume: " + GameSettings.CurrentSettings.MasterVolume + "%",
			};
			Slider<SliderIndex<int>> masterVolumeSlider = new UI.Slider<SliderIndex<int>>
			{
				DataSet = GameSettings.VolumeSliderOptions,
				Parent = RightContainer,
				Size = new UICoords(0, 20, 1f, 0),
				AnchorPoint = new Vector2(0.0f, 0.0f),
				UnselectedBGColor = new Color(0.6f, 0.6f, 0.6f),
				SelectedBGColor = new Color(0.0f, 0.0f, 0.0f),
				Scrubber = new Scrubber { Width = 20 },
				BGColor = new Color(0.25f, 0.25f, 0.25f),
			};
			void onMasterVolumeSliderChanged(Slider<SliderIndex<int>> sl, SliderIndex<int> val, int index)
			{
				GameSettings.CurrentSettings.MasterVolume = val.Value;
				masterVolumeLabel.Text = "Master Volume: " + val.Display + "%";
				
				GameSounds.MenuBlip?.Play(val.Value/100.0f, 1, 0.0f);
			}
			masterVolumeSlider.OnValueChanged += onMasterVolumeSliderChanged;



			Label musicVolumeLabel = new Label
			{
				TextColor = Color.White,
				Parent = RightContainer,
				Size = new UICoords(0, 25, 1, 0),
				Font = GFX.Fonts.Arial14,
				Text = "Music Volume: " + GameSettings.ChatSizeSliderOptions[(int)GameSettings.CurrentSettings.ChatSize].Display,
			};
			Slider<SliderIndex<int>> musicVolumeSlider = new UI.Slider<SliderIndex<int>>
			{
				DataSet = GameSettings.VolumeSliderOptions,
				Parent = RightContainer,
				Size = new UICoords(0, 25, 1f, 0),
				AnchorPoint = new Vector2(0.0f, 0.0f),
				UnselectedBGColor = new Color(0.6f, 0.6f, 0.6f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Scrubber = new Scrubber { Width = 20 },
				BGColor = new Color(0.25f, 0.25f, 0.25f),
			};
			void onMusicVolumeSliderChanged(Slider<SliderIndex<int>> sl, SliderIndex<int> val, int index)
			{
				GameSettings.CurrentSettings.MusicVolume = val.Value;
				musicVolumeLabel.Text = "Music Volume: " + val.Display + "%";
				Game.Settings.MusicVolume = val.Value;
				GameSounds.MenuBlip?.Play(val.Value / 100.0f, 1, 0.0f);
			}
			musicVolumeSlider.OnValueChanged += onMusicVolumeSliderChanged;



			Label sfxVolumeLabel = new Label
			{
				TextColor = Color.White,
				Parent = RightContainer,
				Size = new UICoords(0, 25, 1, 0),
				Font = GFX.Fonts.Arial14,
				Text = GetSFXLabelText(GameSettings.CurrentSettings.SfxVolume),
			};
			NumericSlider sfxVolumeSlider = new NumericSlider
			{
				Maximum = 100, Minimum = 0, Interval = 1,

				Parent = RightContainer,
				Size = new UICoords(0, 25, 1f, 0),
				AnchorPoint = new Vector2(0.0f, 0.0f),
				UnselectedBGColor = new Color(0.6f, 0.6f, 0.6f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Scrubber = new Scrubber { Width = 20 },
				BGColor = new Color(0.25f, 0.25f, 0.25f),
			};
			string GetSFXLabelText(float value) => "SFX Volume: " + Math.Floor(value) + "%";
			void OnSFXVolumeSliderChanged(NumericSlider slider, float value)
			{
				GameSettings.CurrentSettings.SfxVolume = (int)value;
				AudioManager.EffectVolume = value / 100.0f;
				sfxVolumeLabel.Text = GetSFXLabelText(GameSettings.CurrentSettings.SfxVolume);
			}
			sfxVolumeSlider.OnValueChanged += OnSFXVolumeSliderChanged;

			Label controlLbl = new Label
			{
				TextColor = Color.White,
				Parent = RightContainer,
				Size = new UICoords(0, 25, 1, 0),
				Font = GFX.Fonts.Arial16,
				Text = "CONTROLS",
				TextXAlign = TextXAlignment.Center,
			};

			void bindButtonClick(TextButton b, MouseState m)
			{
				rebinding = (BindButton)b;
			}


			void jumpRebind(Keys key)
			{
				GameSettings.CurrentSettings.JumpKey = key;
				jumpKeybindButton.Text = "Jump: " + GameSettings.CurrentSettings.JumpKey.ToString();
			}


			jumpKeybindButton = new BindButton
			{
				TextColor = Color.White,
				Text = "Jump: " + GameSettings.CurrentSettings.JumpKey.ToString(),
				Font = GFX.Fonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Parent = RightContainer,
			};
			jumpKeybindButton.OnLeftClick += bindButtonClick;
			jumpKeybindButton.OnRebind += jumpRebind;


			void upRebind(Keys key)
			{
				GameSettings.CurrentSettings.MoveUpKey = key;
				upKeybindButton.Text = "Climb/Up: " + GameSettings.CurrentSettings.MoveUpKey.ToString();
			}

			upKeybindButton = new BindButton
			{
				TextColor = Color.White,
				Text = "Climb/Up: " + GameSettings.CurrentSettings.MoveUpKey.ToString(),
				Font = GFX.Fonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Parent = RightContainer,
			};
			upKeybindButton.OnLeftClick += bindButtonClick;
			upKeybindButton.OnRebind += upRebind;

			

			BindButton downKeybindButton = new BindButton
			{
				TextColor = Color.White,
				Text = "Descend/Down: " + GameSettings.CurrentSettings.MoveDownKey.ToString(),
				Font = GFX.Fonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Parent = RightContainer,
			};
			void downRebind(Keys key)
			{
				GameSettings.CurrentSettings.MoveDownKey = key;
				downKeybindButton.Text = "Descend/Down: " + GameSettings.CurrentSettings.MoveDownKey.ToString();
			}
			downKeybindButton.OnLeftClick += bindButtonClick;
			downKeybindButton.OnRebind += downRebind;


			BindButton leftKeybindButton = new BindButton
			{
				TextColor = Color.White,
				Text = "Walk Left: " + GameSettings.CurrentSettings.MoveLeftKey.ToString(),
				Font = GFX.Fonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Parent = RightContainer,
			};
			void leftRebind(Keys key)
			{
				GameSettings.CurrentSettings.MoveLeftKey = key;
				leftKeybindButton.Text = "Walk Left: " + GameSettings.CurrentSettings.MoveLeftKey.ToString();
			}
			leftKeybindButton.OnLeftClick += bindButtonClick;
			leftKeybindButton.OnRebind += leftRebind;

			BindButton rightKeybindButton = new BindButton
			{
				TextColor = Color.White,
				Text = "Walk Right: " + GameSettings.CurrentSettings.MoveRightKey.ToString(),
				Font = GFX.Fonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Parent = RightContainer,
			};
			void rightRebind(Keys key)
			{
				GameSettings.CurrentSettings.MoveRightKey = key;
				rightKeybindButton.Text = "Walk Right: " + GameSettings.CurrentSettings.MoveRightKey.ToString();
			}
			rightKeybindButton.OnLeftClick += bindButtonClick;
			rightKeybindButton.OnRebind += rightRebind;

			TextButton backButton = new TextButton
			{
				TextColor = Color.White,
				Text = "BACK",
				Font = GFX.Fonts.Arial16,
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

			backButton.OnLeftClick += (btn, mouse) => Game.CurrentGameContext = Game.MenuContext;
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
