import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Client')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Client.UI')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Client.Menu')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Common')

local TimeoutPage = UIRoot();

local message = Label(script,
{
    Name = "MessageLabel",
    BGColor = Color(0,0,0,0.5),
    BorderColor = Color.Transparent,
    Size = UICoords(0, 0, 1.0, 0.1),
    AnchorPoint = Vector2(0.5, 0.5),
    Position = UICoords(0, 0, 0.5, 0.3),
    Parent = TimeoutPage,
    Text = "blank",
    TextColor = Color.White,
    Font = GraphicsEngine.Instance.Fonts.Arial16,
    BorderSize = 0,
    TextWrap = false,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
});

local GetButton = require("Assets.Scripts.MenuScripts.MenuButton");
local GetInactiveButton = require("Assets.Scripts.MenuScripts.InactiveMenuButton");

local back = GetButton("BACK", TimeoutPage)
back.OnLMBClick:Bind(function(ev, mb)
    menumanager.CurrentPage = menumanager.Pages["mainmenu"];
end)

return TimeoutPage;