import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame', 'CaveGame.Client')
import ('CaveGame', 'CaveGame.Client.UI')
import ('CaveGame', 'CaveGame.Client.Menu')
import ('CaveGame', 'CaveGame.Core')

local MultiplayerMenu = UIRoot();


local inputHistory = MultiplayerInputHistory.Load();

local title = Label(script, {
    BGColor = Color.Transparent,
    BorderColor = Color.Transparent,
    Size = UICoords(0, 0, 0.3, 0.1),
    AnchorPoint = Vector2(0.5, 0.5),
    Position = UICoords(0, 0, 0.5, 0.05),
    Parent = MultiplayerPage,
    TextColor = Color.White,
    Text = "MULTIPLAYER",
    Font = GameFonts.Arial20,
    BorderSize = 0,
    TextWrap = false,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
});

local buttonList = UIRect(script,
{
    Size = UICoords(220, -20, 0, 0.8),
    Position = UICoords(10, 0, 0, 0.1),
    Parent = MultiplayerPage,
    BGColor = Color.DarkBlue,
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
    Font = GameFonts.Arial12,
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
    Font = GameFonts.Arial12,
    BGColor = new Color(0.2, 0.2, 0.3),
    BorderColor = Color.DarkBlue,
    BackgroundText = "Nickname",
    BackgroundTextColor = Color.Gray,
    TextColor = Color.White,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
});
usernameInputBox.Input.InputBuffer = inputHistory.Username;
usernameInputBox.Input.BlacklistedCharacters.Add(' ');
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

    game.CurrentGameContext = game.InWorldContext;
    game.InWorldContext.NetworkUsername = username;
    game.InWorldContext.ConnectAddress = address;

    inputHistory.IPAddress = address;
    inputHistory.Username = username;

end

local connect = TextButton(script, {
    Size = UICoords(0, 35, 1, 0),
    Parent = buttons,
    Text = "CONNECT",
    Font = GameFonts.Arial14,
    BorderSize = 0,
    TextColor = Color.White,
    TextWrap = false,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
    UnselectedBGColor = Color(0.2, 0.2, 0.2),
    SelectedBGColor = Color(0.1, 0.1, 0.1),
});
connect.OnLMBClick:Bind(function(ev, mb)
    OnJoinServer(serverInputBox.Input.InternalText, usernameInputBox.Input.InternalText);
end)


local back = TextButton(script, {
    Size = UICoords(0, 30, 1, 0),
    Parent = buttons,
    Text = "BACK",
    Font = GameFonts.Arial14,
    BorderSize = 0,
    TextColor = Color.White,
    TextWrap = false,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
    UnselectedBGColor = Color(0.2, 0.2, 0.2),
    SelectedBGColor = Color(0.1, 0.1, 0.1),
});

back.OnLMBClick:Bind(function(ev, mb)
    menumanager.CurrentPage = menumanager.Pages[""];
end)

return MultiplayerMenu;