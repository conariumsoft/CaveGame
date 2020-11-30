import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame', 'CaveGame.Client')
import ('CaveGame', 'CaveGame.Client.UI')

local MainMenu = require("Assets.Scripts.MenuScripts.MainMenu")
local SingleplayerMenu = require("Assets.Scripts.MenuScripts.SingleplayerMenu")
local MultiplayerMenu = require("Assets.Scripts.MenuScripts.MultiplayerMenu")
local TimeoutMenu = require("Assets.Scripts.MenuScripts.TimeoutMenu")
local CreditsMenu = require("Assets.Scripts.MenuScripts.CreditsMenu")

menumanager.Pages:Add("mainmenu", MainMenu);
menumanager.Pages:Add("multiplayermenu", MultiplayerMenu);
menumanager.Pages:Add("timeoutmenu", TimeoutMenu);
menumanager.Pages:Add("singleplayermenu", SingleplayerMenu);
menumanager.Pages:Add("creditsmenu", CreditsMenu);

menumanager.CurrentPage = MainMenu;