using CaveGame;
using CaveGame.Client.UI;
using CaveGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using NLua;
using System.IO;
using System.Diagnostics;
using DataManagement;
using CaveGame.Core.LuaInterop;

namespace CaveGame.Client.Menu
{
	public static class EnumExtensions
    {
		public static int ToInt(this GameChatSize e)
        {
			return (int)e;
        }
    }
	public class MenuManager : IGameContext
	{

		public CaveGameGL Game { get; private set; }

		public bool Active { get; set; }

		Microsoft.Xna.Framework.Game IGameContext.Game => Game;

		public Dictionary<string, UIRoot> Pages { get; set; }

		private UIRoot _currentPage;
		public UIRoot CurrentPage {
			get => _currentPage;
			set
            {
				_currentPage?.OnUnload.Invoke(null);
				_currentPage = value;
				_currentPage.OnLoad.Invoke(null);
            }
		}

		public string TimeoutMessage { get; set; }

		RasterizerState rastering;
		public MenuManager(CaveGameGL _game)
		{
			Pages = new Dictionary<string, UIRoot>();
			Game = _game;
			rastering = new RasterizerState() { ScissorTestEnable = true };
		}

		public void Draw(GraphicsEngine GFX)
		{
			GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, rastering);
			CurrentPage?.Draw(GFX);
			GFX.End();
		}

		private void LoadMenuScripts()
        {
			Lua luastate = new Lua();
			luastate.LoadCLRPackage();
			luastate["_G.game"] = Game;
			luastate["_G.menumanager"] = this;
			//luastate["buttonlist"] = buttons;
			luastate["_G.script"] = luastate;
            luastate.DoString(@"_G.oldprint = print; _G.print = function(str) game.Console:LuaPrint(str) end");
			luastate.DoString(LuaSnippets.UtilityFunctions);
			luastate.DoFile(Path.Combine("assets", "scripts", "menu.lua"));
		}

		public void Load()
		{

			LoadMenuScripts();

		}

		public void Unload()
		{

		}


		public void Update(GameTime gt)
		{

			CurrentPage?.Update(gt);
		}
	}
}
