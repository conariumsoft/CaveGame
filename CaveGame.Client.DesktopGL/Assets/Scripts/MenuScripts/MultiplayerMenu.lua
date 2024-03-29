import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Client')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Client.UI')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Client.Menu')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Common')

local MultiplayerMenu = UIRoot();


local inputHistory = MultiplayerInputHistory.Load();

local GetButton = require("Assets.Scripts.MenuScripts.MenuButton");
local GetInactiveButton = require("Assets.Scripts.MenuScripts.InactiveMenuButton");

local title = Label(script, {
    BGColor = Color.Transparent,
    BorderColor = Color.Transparent,
    Size = UICoords(0, 0, 0.3, 0.1),
    AnchorPoint = Vector2(0.5, 0.5),
    Position = UICoords(0, 0, 0.5, 0.05),
    Parent = MultiplayerMenu,
    TextColor = Color.White,
    Text = "MULTIPLAYER",
    Font = GraphicsEngine.Instance.Fonts.Arial20,
    BorderSize = 0,
    TextWrap = false,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
});

local buttonList = UIRect(script,
{
    Size = UICoords(220, -20, 0, 0.8),
    Position = UICoords(10, 0, 0, 0.1),
    Parent = MultiplayerMenu,
    BGColor = Color(0.1, 0.1, 0.1),
});

local buttons = UIListContainer();
buttons.Padding = 1;
buttons.Parent = buttonList;

local serverInputBox = TextInputLabel(script, {
    Size = UICoords(0, 30, 1, 0),
    AnchorPoint = Vector2(0, 0),
    Parent = buttons,
    BGColor = Color(0.2, 0.2, 0.3),
    BorderColor = Color.DarkBlue,
    Font = GraphicsEngine.Instance.Fonts.Arial12,
    BackgroundText = "Server Address",
    BackgroundTextColor = Color.Gray,
    TextColor = Color.White,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
});
serverInputBox.Input.InputBuffer = inputHistory.IPAddress;
serverInputBox.Input.Focused = false;
serverInputBox.Input.CursorPosition = inputHistory.IPAddress.Length;

local usernameInputBox = TextInputLabel(script, {
    Size = UICoords(0, 30, 1, 0),
    AnchorPoint = Vector2(0, 0),
    Parent = buttons,
    Font = GraphicsEngine.Instance.Fonts.Arial12,
    BGColor = Color(0.2, 0.2, 0.3),
    BorderColor = Color.DarkBlue,
    BackgroundText = "Nickname",
    BackgroundTextColor = Color.Gray,
    TextColor = Color.White,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
});
usernameInputBox.Input.InputBuffer = inputHistory.Username;
usernameInputBox.Input:BlacklistCharacter(' ');
usernameInputBox.Input.Focused = true;


local function Timeout(message)
    menumanager.CurrentPage = menumanager.Pages["timeout"];
    menumanager.Pages["timeout"].Message = message;
end

local function OnJoinServer(address, username) 
    if #address == 0 then
        Timeout("Server Address is empty! Please enter a valid IP Address!");
        return;
    end

    if #username == 0 then
        Timeout("Please enter a nickname!");
        return;
    end

    game:StartClient(username, address);
    --game.CurrentGameContext = game.GameClientContext;
    --game.GameClientContext.NetworkUsername = username;
    --game.GameClientContext.ConnectAddress = address;

    inputHistory.IPAddress = address;
    inputHistory.Username = username;
    inputHistory:Save();

end

local connect = GetButton("CONNECT", buttons)
connect.OnLMBClick:Bind(function(ev, mb)
    OnJoinServer(serverInputBox.Input.InternalText, usernameInputBox.Input.InternalText);
end)


local back = TextButton(script, {
    Size = UICoords(0, 30, 1, 0),
    Parent = buttons,
    Text = "BACK",
    Font = GraphicsEngine.Instance.Fonts.Arial14,
    BorderSize = 0,
    TextColor = Color.White,
    TextWrap = false,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
    UnselectedBGColor = Color(0.2, 0.2, 0.2),
    SelectedBGColor = Color(0.1, 0.1, 0.1),
});

back.OnLMBClick:Bind(function(ev, mb)
    menumanager.CurrentPage = menumanager.Pages["mainmenu"];
end)

return MultiplayerMenu;