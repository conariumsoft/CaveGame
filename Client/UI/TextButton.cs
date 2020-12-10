using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using CaveGame.Client.Menu;
using CaveGame;
using CaveGame.Core.LuaInterop;
using NLua;
using System.Diagnostics;
using CaveGame.Core;

namespace CaveGame.Client.UI
{
	public interface IButtonWidget
	{
        bool MouseDown { get; }
		bool Selected { get; set; }
		Color UnselectedBGColor { get; set; }
		Color SelectedBGColor { get; set; }
    }

	public class TextButton: Label, IButtonWidget
	{

		public TextButton(NLua.Lua state, NLua.LuaTable table) : base(state, table) { }
		public TextButton() : base() {}

		public Color UnselectedBGColor { get; set; }
		public Color SelectedBGColor { get; set; }
		public bool Selected { get; set; }
        public bool MouseDown { get; private set; }

		public delegate void ClickHandler(TextButton sender, MouseState mouse);

		public event ClickHandler OnLeftClick;
		public event ClickHandler OnRightClick;

        public LuaEvent<LuaEventArgs> OnLMBClick = new LuaEvent<LuaEventArgs>();
		public LuaEvent<LuaEventArgs> OnRMBClick = new LuaEvent<LuaEventArgs>();

		protected MouseState prevMouse = Mouse.GetState();



		public override void Update(GameTime gt)
		{
			MouseState mouse = Mouse.GetState();

			Selected = IsMouseInside(mouse);


            if (Selected && !IsMouseInside(prevMouse))
                GameSounds.MenuBlip?.Play(1.0f, 1, 0.0f);


            if (!Selected && IsMouseInside(prevMouse))
                GameSounds.MenuBlip?.Play(0.8f, 1, 0.0f);

            if (Selected && CaveGameGL.ClickTimer > (1/60.0f)) 
			{ 
				if (mouse.LeftButton == ButtonState.Pressed && (prevMouse.LeftButton != ButtonState.Pressed)) 
				{ 
					GameSounds.MenuBlip?.Play(1.0f, 0.9f, 0.0f); 
					OnLeftClick?.Invoke(this, mouse); 
					OnLMBClick.Invoke(new LuaEventArgs()); 
					CaveGameGL.ClickTimer = 0; 
                }

                    
				if (mouse.RightButton == ButtonState.Pressed && (prevMouse.RightButton != ButtonState.Pressed)) 
				{
                    
					GameSounds.MenuBlip?.Play(1.0f, 0.9f, 0.0f);
                    OnRightClick?.Invoke(this, mouse);
                    OnRMBClick.Invoke(new LuaEventArgs()); 
					CaveGameGL.ClickTimer = 0; 
                }
						
            }



			prevMouse = mouse;


			BGColor = Selected ? SelectedBGColor : UnselectedBGColor;

            base.Update(gt);
		}

		public override void Draw(GraphicsEngine GFX)
		{
			base.Draw(GFX);
		}
	}
}
