using System;
using System.Collections.Generic;
using System.Text;
using CaveGame.Client.DesktopGL;
using CaveGame.Common;
using CaveGame.Common.LuaInterop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CaveGame.Client.UI
{
    public class ContextButton : UIRect, IButtonWidget
    {
        public ContextButton(NLua.Lua state, NLua.LuaTable table) : this()
        {
            this.InitFromLuaPropertyTable(state, table);
        }
        public ContextButton() : base()
        {
            ContextNodes = new List<UINode>();
        }

        public Color UnselectedBGColor { get; set; }
        public Color ActivatedBGColor { get; set; }
        public Color SelectedBGColor { get; set; }
        public bool Selected { get; set; }
        public bool MouseDown { get; private set; }

        public LuaEvent<LuaEventArgs> OnLMBClick = new LuaEvent<LuaEventArgs>();
        public LuaEvent<LuaEventArgs> OnRMBClick = new LuaEvent<LuaEventArgs>();
        public LuaEvent<LuaEventArgs> OnSelected = new LuaEvent<LuaEventArgs>();
        public LuaEvent<LuaEventArgs> OnUnselected = new LuaEvent<LuaEventArgs>();

        public List<UINode> ContextNodes { get; set; }

        protected MouseState prevMouse = Mouse.GetState();

        public override void Update(GameTime gt)
        {

            MouseState mouse = Mouse.GetState();

            if (MouseOver && !IsMouseInside(prevMouse))
                GameSounds.MenuBlip?.Play(1.0f, 1, 0.0f);


            if (!MouseOver && IsMouseInside(prevMouse))
                GameSounds.MenuBlip?.Play(0.8f, 1, 0.0f);


            // selecting
            if (MouseOver && !Selected && mouse.LeftButton == ButtonState.Pressed && (prevMouse.LeftButton != ButtonState.Pressed))
            {
                GameSounds.MenuBlip?.Play(1.0f, 0.9f, 0.0f);
                OnSelected.Invoke(new LuaEventArgs());
                Selected = true;
            }

            // unselecting
            if (Selected && mouse.LeftButton == ButtonState.Pressed && (prevMouse.LeftButton != ButtonState.Pressed))
            {
                if (MouseOver)
                    return;

                foreach(var node in ContextNodes)
                {
                    if (node.MouseOver)
                        return;
                }

                Selected = false;
                GameSounds.MenuBlip?.Play(1.0f, 0.9f, 0.0f);
                OnUnselected.Invoke(new LuaEventArgs());
            }

            
            // FIXME: Reference to DesktopClient in Client code...
            // Keep this shit platform independent, AND self-contained
            if (Selected && CaveGameDesktopClient.ClickTimer > (1 / 60.0f))
            {
                if (mouse.LeftButton == ButtonState.Pressed && (prevMouse.LeftButton != ButtonState.Pressed))
                {
                    GameSounds.MenuBlip?.Play(1.0f, 0.9f, 0.0f);
                    OnLMBClick.Invoke(new LuaEventArgs());
                    CaveGameDesktopClient.ClickTimer = 0;
                }


                if (mouse.RightButton == ButtonState.Pressed && (prevMouse.RightButton != ButtonState.Pressed))
                {

                    GameSounds.MenuBlip?.Play(1.0f, 0.9f, 0.0f);
                    OnRMBClick.Invoke(new LuaEventArgs());
                    // Ditto Here
                    CaveGameDesktopClient.ClickTimer = 0;
                }
            }

            prevMouse = mouse;
            if (Selected)
            {
                BGColor = ActivatedBGColor;
            } else
                BGColor = MouseOver ? SelectedBGColor : UnselectedBGColor;

            base.Update(gt);
        }

        public override void Draw(GraphicsEngine GFX) => base.Draw(GFX);
    }
}
