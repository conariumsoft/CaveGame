import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame', 'CaveGame.Client')
import ('CaveGame', 'CaveGame.Client.UI')

local MainMenu = require("assets.scripts.MenuScripts.MainMenu")
local MultiplayerMenu = require("assets.scripts.MenuScripts.MultiplayerMenu")
local TimeoutMenu = require("assets.scripts.MenuScripts.TimeoutMenu")
local SingleplayerMenu = require("assets.scripts.MenuScripts.SingleplayerMenu")
local CreditsMenu = require("assets.scripts.MenuScripts.CreditsMenu")

menumanager.Pages:Add("mainmenu", MainMenu);
menumanager.Pages:Add("multiplayermenu", MultiplayerMenu);
menumanager.Pages:Add("timeoutmenu", TimeoutMenu);
menumanager.Pages:Add("singleplayermenu", SingleplayerMenu);
menumanager.Pages:Add("creditsmenu", CreditsMenu);

menumanager.CurrentPage = MainMenu;