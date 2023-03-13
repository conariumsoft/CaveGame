print("Start initialization of lua menu script")

import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Common')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Client')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Client.UI')


local MainMenu = require("Assets.Scripts.MenuScripts.MainMenu")

print("require MainMenu Succeeded")

local SingleplayerMenu = require("Assets.Scripts.MenuScripts.SingleplayerMenu")

print("require SingleplayerMenu Succeeded")

local MultiplayerMenu = require("Assets.Scripts.MenuScripts.MultiplayerMenu")

print("require MultiplayerMenu Succeeded")

local TimeoutMenu = require("Assets.Scripts.MenuScripts.TimeoutMenu")

print("require TimeoutMenu Succeeded")

local CreditsMenu = require("Assets.Scripts.MenuScripts.CreditsMenu")

print("require CreditsMenu Succeeded")

local NewWorldMenu = require("Assets.Scripts.MenuScripts.NewWorldMenu")

print("require NewWorldMenu Succeeded")

menumanager.Pages:Add("mainmenu", MainMenu);
menumanager.Pages:Add("singleplayermenu", SingleplayerMenu);
menumanager.Pages:Add("multiplayermenu", MultiplayerMenu);
menumanager.Pages:Add("timeoutmenu", TimeoutMenu);
menumanager.Pages:Add("creditsmenu", CreditsMenu);
menumanager.Pages:Add("newworldmenu", NewWorldMenu);

menumanager.CurrentPage = MainMenu;