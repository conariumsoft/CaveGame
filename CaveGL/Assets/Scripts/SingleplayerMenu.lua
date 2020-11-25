import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame', 'CaveGame.Client')
import ('CaveGame', 'CaveGame.Client.UI')
import ('CaveGame', 'CaveGame.Core')


local MainMenu = UIRoot();

MainMenu.OnLoad:Bind(function(ev)


end)

MainMenu.OnUnload:Bind(function(ev)


end)

local title = Label(script, {
	Parent = menu,
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
	Parent = menu,
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
        Parent = buttonlist,
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

spb.OnLMBClick:Bind(function(ev, mousedata)
	GameSounds.MenuBlip:Play(1, 0.25, 0)
	spb.Text = 'LUA CLICKED!'
	spb.UnselectedBGColor = Color(0.05, 0.05, 0.05)
	spb.SelectedBGColor = Color(0.2, 0.2, 0.2)
	spb.Size = spb.Size + UICoords(0, 10, 0, 0)
end)

local multiplayerButton = GetButton("MULTIPLAYER");
local statisticsButton = GetButton("STATISTICS");
local steamPageButton = GetButton("STEAM WORKSHOP");
local discordButton = GetButton("DISCORD COMMUNITY");
discordButton.OnLMBClick:Bind(function (ev, md)
    OperatingSystem.OpenUrl("https://discord.gg/6mDmYqs");
end)

local creditsButton = GetButton("CREDITS");
creditsButton.OnLMBClick:Bind(function(ev, md)
    --menumanager.CurrentPage = 
end)


return MainMenu;