import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame', 'CaveGame.Client')
import ('CaveGame', 'CaveGame.Client.UI')

local MainMenu = require("Assets.Scripts.MainMenu")
local MultiplayerMenu = require("Assets.Scripts.MultiplayerMenu")

menumanager.Pages:Add("mainmenu", MainMenu);
menumanager.Pages:Add("multiplayermenu", MultiplayerMenu);

menumanager.CurrentPage = MainMenu;