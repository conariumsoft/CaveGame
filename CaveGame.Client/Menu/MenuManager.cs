
using CaveGame.Client.UI;
using CaveGame.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using NLua;
using System.IO;
using CaveGame.Client.DesktopGL;
using CaveGame.Common.Extensions;
using CaveGame.Common.LuaInterop;

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

		public CaveGameDesktopClient Game { get; private set; }

		public bool Active { get; set; }


		public UIRoot TimeoutPage => Pages["timeoutmenu"];

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

		public string TimeoutMessage
		{
			get => TimeoutPage.FindNode<Label>("MessageLabel").Text;
			set => TimeoutPage.FindNode<Label>("MessageLabel").Text = value;
            
		}

		RasterizerState rastering;
		public MenuManager(CaveGameDesktopClient _game)
		{
			Pages = new Dictionary<string, UIRoot>();
			Game = _game;
			rastering = new RasterizerState() { ScissorTestEnable = false };
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
            //luastate.DoString(@"_G.oldprint = print; _G.print = function(str)  --game.Console:LuaPrint(str) end");
			luastate.DoString(LuaSnippets.UtilityFunctions);
			Console.WriteLine("PreLoad");
			try
			{
				luastate.DoFile(Path.Combine("Assets", "Scripts", "menu.lua"));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			Console.WriteLine("PostLoad");
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
