using CaveGame.Client.UI;
using CaveGame.Core;
using CaveGame.Core.Game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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

		public Settings(CaveGameGL _game)
		{
			Game = _game;
		}

		public CaveGameGL Game { get; set; }
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
				Text = "Fullscreen: "+ReadableBoolean(GameSettings.CurrentSettings.Fullscreen),
				Font = GFX.Fonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			void updateFullscreenBnt(TextButton b, MouseState m)
			{
				GameSettings.CurrentSettings.Fullscreen = !GameSettings.CurrentSettings.Fullscreen;
				fullscreenBtn.Text = "Fullscreen: " + ReadableBoolean(GameSettings.CurrentSettings.Fullscreen);
				GameSettings.CurrentSettings.Save();
				Game.OnSetFullscreen(GameSettings.CurrentSettings.Fullscreen);
				
			}
			fullscreenBtn.OnLeftClick += updateFullscreenBnt;

			TextButton particlesBtn = new TextButton
			{
				Parent = container,
				TextColor = Color.White,
				Text = "Particles: " + ReadableBoolean(GameSettings.CurrentSettings.Particles),
				Font = GFX.Fonts.Arial14,
				Size = new UICoords(0, 25, 1, 0),
				TextXAlign = TextXAlignment.Center,
				TextYAlign = TextYAlignment.Center,
				UnselectedBGColor = new Color(0.2f, 0.2f, 0.2f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
			};
			void updateParticles(TextButton b, MouseState m)
			{
				particlesBtn.Text = "Particles: " + ReadableBoolean(GameSettings.CurrentSettings.Particles);
				GameSettings.CurrentSettings.Particles = !GameSettings.CurrentSettings.Particles;
				GameSettings.CurrentSettings.Save();
			}
			particlesBtn.OnLeftClick += updateParticles;


			Label fpsCapText = new Label
			{
				TextColor = Color.White,
				Parent = container,
				Size = new UICoords(0, 25, 1, 0),
				Font = GFX.Fonts.Arial14,
				Text = "Framerate Cap: " + GameSettings.CurrentSettings.FPSLimit,
			};

			Slider<SliderIndex<int>> fpsCapSlider = new UI.Slider<SliderIndex<int>>
			{
				DataSet = GameSettings.FramerateCapSliderOptions,
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
				GameSettings.CurrentSettings.FPSLimitIndex = index;
				GameSettings.CurrentSettings.FPSLimit = val.Value;
				fpsCapText.Text = "FPS Cap:" + val.Display;
				Game.OnSetFPSLimit(val.Value);
				GameSounds.MenuBlip?.Play(0.8f, 1, 0.0f);
			}
			fpsCapSlider.OnValueChanged += onFpsCapSliderChanged;
			//fpsCapSlider.SetIndex(GameSettings.CurrentSettings.FPSLimitIndex);

			Label chatSizeText = new Label
			{
				TextColor = Color.White,
				Parent = container,
				Size = new UICoords(0, 25, 1, 0),
				Font = GFX.Fonts.Arial14,
				Text = "Chat Size: " + GameSettings.ChatSizeSliderOptions[(int)GameSettings.CurrentSettings.ChatSize].Display,
			};
			Slider<SliderIndex<GameChatSize>> chatSizeSlider = new UI.Slider<SliderIndex<GameChatSize>>
			{
				DataSet = GameSettings.ChatSizeSliderOptions,
				Parent = container,
				Size = new UICoords(0, 25, 0.5f, 0),
				AnchorPoint = new Vector2(0.0f, 0.0f),
				UnselectedBGColor = new Color(0.6f, 0.6f, 0.6f),
				SelectedBGColor = new Color(0.1f, 0.1f, 0.1f),
				Scrubber = new Scrubber { Width = 20 },
				BGColor = new Color(0.25f, 0.25f, 0.25f),
			};
			void onChatSliderChanged(Slider<SliderIndex<GameChatSize>> sl, SliderIndex<GameChatSize> val, int index)
			{
				GameSettings.CurrentSettings.ChatSize = val.Value;
				chatSizeText.Text = "Chat Size:" + val.Display;
				Game.OnSetChatSize(val.Value);
				GameSounds.MenuBlip?.Play(0.8f, 1, 0.0f);
			}
			chatSizeSlider.OnValueChanged += onChatSliderChanged;
			//chatSizeSlider.SetIndex((int)GameSettings.CurrentSettings.ChatSize);

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
				Parent = container,
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
				Parent = container,
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
				Parent = container,
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
				Parent = container,
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
				Parent = container,
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
