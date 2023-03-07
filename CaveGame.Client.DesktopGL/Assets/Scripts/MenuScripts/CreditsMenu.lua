import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame', 'CaveGame.Client')
import ('CaveGame', 'CaveGame.Client.UI')
import ('CaveGame', 'CaveGame.Client.Menu')
import('CaveGame', 'CaveGame.Common')

local credits = {
    ">>CAVE GAME",
    "",
    ">Josh O'Leary",
    "Programming, Game Design",
    "",
    ">invinciblespeed",
    "Art",
    "",
    ">Tyler Stewart",
    "Biz",
    "",
    ">Contributing Developers",
    "dodeadam - Programming",
    "ConcurrentSquared - Programming & Design",
    "Mescalyne - Music",
    "Bumpylegoman02 - Security Testing & Design",
    "WheezyBackports - Community Multiplayer Servers",
    "",
    ">Testing",
    "Andrew J.",
    "squidthonkv2",
    "Billy J.",
    "WheezyBackports",
    "",
    "Copyright Conarium Software 2020",
}

local CreditsPage = UIRoot();

local creditslist = UIRect(script, 
{
    Size = UICoords(0, 0, 1.0, 1.0),
    Position = UICoords(0, 0, 0, 0),
    Parent = CreditsPage,
    BGColor = Color.Black*0.5,
});

local backButton = TextButton(script, 
{
    TextColor = Color.White,
    Text = "BACK",
    Font = GraphicsEngine.Instance.Fonts.Arial16,
    Size = UICoords(100, 30, 0, 0),
    Position = UICoords(10, -30, 0, 1.0),
    AnchorPoint = Vector2(0, 1),
    TextWrap = true,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
    Parent = creditslist,
    UnselectedBGColor = Color(0.2, 0.2, 0.2),
    SelectedBGColor = Color(0.1, 0.1, 0.1),
}); 
backButton.OnLMBClick:Bind(function(ev, mb)
    menumanager.CurrentPage = menumanager.Pages["mainmenu"];
    --Game.CurrentGameContext = Game.MenuContext;
end)


local container = UIListContainer();
container.Padding = 0;
container.Parent = creditslist;

for _, text in pairs(credits) do

    local displayedText = text;
    local font = GraphicsEngine.Instance.Fonts.Arial14;
    local size = 16;
    if text:find(">>") then
        font = GraphicsEngine.Instance.Fonts.Arial20;
        size = 24;
        displayedText = text:gsub(">>", "");
    elseif text:find(">") then
        font = GraphicsEngine.Instance.Fonts.Arial16;
        size = 20;
        displayedText = text:gsub(">", "");
    end
    

    local label = Label(script,
    {
        TextColor = UITheme.SmallButtonTextColor,
        Text = displayedText,
        Font = font,
        Size = UICoords(1, size, 1.0, 0),
        BGColor = Color.Black * 0.0,
        TextXAlign = TextXAlignment.Center,
        Parent = container,
    });
end

return CreditsPage;