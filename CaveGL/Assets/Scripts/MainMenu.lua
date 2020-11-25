import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame', 'CaveGame.Client')
import ('CaveGame', 'CaveGame.Client.UI')
import ('CaveGame', 'CaveGame.Client.Menu')
import ('CaveGame', 'CaveGame.Core')


local MainMenu = UIRoot();

MainMenu.OnLoad:Bind(function(ev)

end)

MainMenu.OnUnload:Bind(function(ev)
    
end)

local title = Label(script, {
	Parent = MainMenu,
	Text = 'CAVE GAME',
	BGColor = Color.Transparent,
	Size = UICoords(220, -20, 0, 0.1),
	AnchorPoint = Vector2(0,0),
	Position = UICoords(10,10, 0, 0),
	TextColor = Color.White,
	Font = GameFonts.Arial30,
	BorderSize = 0,
	TextWrap = false,
	TextXAlign = TextXAlignment.Center,
	TextYAlign = TextYAlignment.Center,
})


local copyright_label = Label(script, {
	BGColor = Color.Transparent,
	BorderColor = Color.Transparent,
	Size = UICoords(200, 10, 0, 0),
	AnchorPoint = Vector2(0, 1),
	Position = UICoords(10, 0, 0, 1),
	Parent = MainMenu,
	TextColor = Color.White,
	Text = "Copyright Conarium Software 2019-2020",
	BorderSize = 0,
	Font = GameFonts.Arial10,
	TextWrap = false,
	TextYAlign = TextYAlignment.Bottom,
	TextXAlign = TextXAlignment.Left,
})

local versionTag = Label(script, {
    BGColor = Color.Transparent,
    BorderColor = Color.Transparent,
    Size = UICoords(200, 10, 0, 0),
    AnchorPoint = Vector2(1, 1),
    Position = UICoords(-10, 0, 1, 1),
    Parent = MainMenu,
    TextColor = Color.White,
    Text = "v"..Globals.CurrentVersionString,
    BorderSize = 0,
    Font = GameFonts.Arial10,
    TextWrap = false,
    TextXAlign = TextXAlignment.Right,
    TextYAlign = TextYAlignment.Bottom,
});

local buttonList = UIRect(script, {
    Size = UICoords(220, 350, 0, 0),
    AnchorPoint = Vector2(0, 1),
    Position = UICoords(10, 10, 0, 1),
    Parent = MainMenu,
    BGColor = Color.Transparent;
});


local buttonContainer = UIListContainer();
buttonContainer.Padding = 5;
buttonContainer.Parent = buttonList;

-- Singleplayerbutton

local function GetButton(text) 
    return TextButton(script, {
        Parent = buttonContainer,
        Text = text,
        TextColor = Color.White,
        Size = UICoords(0, -10, 1, 0.125),
        Font = GameFonts.Arial14,
        TextWrap = false,
        TextXAlign = TextXAlignment.Center,
        TextYAlign = TextYAlignment.Center,
        UnselectedBGColor = Color(0.2, 0.2, 0.2),
		SelectedBGColor = Color(0.1, 0.1, 0.1)
    })
end
local function GetInactiveButton(text)
    return TextButton(script, {
        Parent = buttonContainer,
        Text = text,
        TextColor = Color.White,
        Size = UICoords(0, -10, 1, 0.125),
        Font = GameFonts.Arial14,
        TextWrap = false,
        TextXAlign = TextXAlignment.Center,
        TextYAlign = TextYAlignment.Center,
        UnselectedBGColor = Color(0.05, 0.05, 0.05),
        SelectedBGColor = Color(0.05, 0.05, 0.05),
    })
end

local spb = GetButton("SINGLEPLAYER");
spb.OnLMBClick:Bind(function(ev, mousedata)
	menumanager.CurrentPage = menumanager.Pages["creditsmenu"];
end)

-- example of 9slice
--[[local slicer = NineSlice(script, {
	Parent = spb,
	Color = Color.White,
	TextureScale = 2,
	Texture = GameTextures.Border,
	TopLeftCornerQuad = Rectangle(0, 0, 8, 8),
	BottomLeftCornerQuad = Rectangle(0, 16, 8, 8),
	LeftSideQuad = Rectangle(0, 8, 8, 8),
	TopSideQuad = Rectangle(8, 0, 8, 8),
	TopRightCornerQuad = Rectangle(16, 0, 8, 8),
	RightSideQuad = Rectangle(16, 8, 8, 8),
	BottomRightCornerQuad = Rectangle(16, 16, 8, 8),
	BottomSideQuad = Rectangle(8, 16, 8, 8)
})]]



local multiplayerButton = GetButton("MULTIPLAYER");
multiplayerButton.OnLMBClick:Bind(function(ev, mousedata)
    menumanager.CurrentPage = menumanager.Pages["multiplayermenu"]
end)
local statisticsButton = GetInactiveButton("STATISTICS");

statisticsButton.OnLMBClick:Bind(function(ev, md)

end)

local steamPageButton = GetButton("STEAM WORKSHOP");
steamPageButton.OnLMBClick:Bind(function(ev, md)
    OperatingSystem.OpenUrl("https://steamcommunity.com/app/1238250");
end)

local discordButton = GetButton("DISCORD COMMUNITY");
discordButton.OnLMBClick:Bind(function (ev, md)
    OperatingSystem.OpenUrl("https://discord.gg/6mDmYqs");
end)

local creditsButton = GetButton("CREDITS");
creditsButton.OnLMBClick:Bind(function(ev, md)
    menumanager.CurrentPage = menumanager.Pages["creditsmenu"];
end)

local quitButton = GetButton("QUIT");
quitButton.OnLMBClick:Bind(function(ev, md)
    game:Exit();
end)


local changeLogContentWindow = UIRect(script, {
    Size = UICoords(350, -70, 0, 1),
    Position = UICoords(-20, 20, 1, 0),
    AnchorPoint = Vector2(1, 0),
    Parent = MainMenu,
    BGColor = Color(0.1, 0.1, 0.1)*0.8,
    BorderSize = 2,
    BorderColor = Color(0.2, 0.2, 0.2),
    BorderEnabled = true,
});

local updateLog = UIListContainer();
updateLog.Padding = 2;
updateLog.Parent = changeLogContentWindow;

local changeLogTextEntries = require("Assets.Scripts.ChangeLogGenerator")


for _, text in pairs(changeLogTextEntries) do
    local displayedText = text;
    local font = GameFonts.Arial10;
    local size = 18;

    if (text:find(">>>")) then
        font = GameFonts.Arial16;
        size = 24;
        displayedText = text:gsub(">>", "");
    elseif text:find(">") then
        font = GameFonts.Arial14;
        size = 16;
        displayedText = text:gsub(">", "")
    elseif text:find("-") then
        font = GameFonts.Arial10;
        size = 12;
    end

    local label = Label(script, {
        TextColor = UITheme.SmallButtonTextColor,
        Text = displayedText,
        Font = font,
        Size = UICoords(1, size, 1, 0),
        BGColor = Color.Black * 0,
        TextXAlign = TextXAlignment.Left,
        Parent = updateLog,
        TextWrap = true,
    });
end



return MainMenu;