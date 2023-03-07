import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame', 'CaveGame.Client')
import ('CaveGame', 'CaveGame.Client.UI')
import ('CaveGame', 'CaveGame.Common')
import ('CaveGame', 'CaveGame.Client.Menu')
import ('System', 'System')

local GetButton = require("Assets.Scripts.MenuScripts.MenuButton")

local NewWorldMenu = UIRoot();

local title = Label(script, {
    Size = UICoords(0,40,0.5,0),
    AnchorPoint = Vector2(0.5, 0),
    Position = UICoords(0, 10, 0, 0),
    TextColor = Color.White,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
    Text = "Create New World",
    Parent = NewWorldMenu,
});

local worldNameForm = TextInputLabel(script, {
    Size = UICoords(500, 40, 0, 0),
    Position = UICoords(0, 0, 0.5, 0.3),
    AnchorPoint = Vector2(.5, 0.5),
    Parent = NewWorldMenu,
    BGColor = Color(0.2, 0.2, 0.3),
    BorderColor = Color.DarkBlue,
    Font = GraphicsEngine.Instance.Fonts.Arial12,
    BackgroundText = "World Name",
    BackgroundTextColor = Color.Gray,
    TextColor = Color.White,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
});
worldNameForm.Input.Focused = false;

local worldSeedForm = TextInputLabel(script, {
    Size = UICoords(500, 40, 0, 0),
    Position = UICoords(0, 0, 0.5, 0.5),
    AnchorPoint = Vector2(.5, 0.5),
    Parent = NewWorldMenu,
    BGColor = Color(0.2, 0.2, 0.3),
    BorderColor = Color.DarkBlue,
    Font = GraphicsEngine.Instance.Fonts.Arial12,
    BackgroundText = "World Seed",
    BackgroundTextColor = Color.Gray,
    TextColor = Color.White,
    TextYAlign = TextYAlignment.Center,
    TextXAlign = TextXAlignment.Center,
});
worldNameForm.Input.Focused = false;

local confirmButton = GetButton("CONFIRM & CREATE");
confirmButton.Parent = NewWorldMenu;
confirmButton.Size = UICoords(220, 30, 0, 0);
confirmButton.Position = UICoords(0, 0, .5, 0.8);
confirmButton.AnchorPoint = Vector2(0.5, 0.5);

confirmButton.OnLMBClick:Bind(function()
    local data = WorldMetadata();
    data.Name = worldNameForm.Input.InternalText

    data.Seed = (#worldSeedForm.Input.InternalText>0) and SavedWorldManager.CalculateHash(worldSeedForm.Input.InternalText) or 55
    game:EnterLocalGame(data)
end)

local backbutton = GetButton("BACK");
backbutton.Parent = NewWorldMenu;
backbutton.Size = UICoords(100, 30, 0, 0);
backbutton.Position = UICoords(10, 0, 0, 0.95);

backbutton.OnLMBClick:Bind(function()
	menumanager.CurrentPage = menumanager.Pages["mainmenu"];
end)


return NewWorldMenu;