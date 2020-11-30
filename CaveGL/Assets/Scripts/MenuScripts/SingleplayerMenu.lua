import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame', 'CaveGame.Client')
import ('CaveGame', 'CaveGame.Client.UI')
import ('CaveGame', 'CaveGame.Core')
import ('CaveGame', 'CaveGame.Client.Menu')

local GetButton = require("Assets.Scripts.MenuScripts.MenuButton")

local SingleplayerMenu = UIRoot();

SingleplayerMenu.OnLoad:Bind(function(ev)
	-- TODO: Load the world listing
end)

SingleplayerMenu.OnUnload:Bind(function(ev)


end)


local title = Label(script, {
	Parent = SingleplayerMenu,
	Text = 'SINGLEPLAYER',
	BGColor = Color.Transparent,
	Size = UICoords(220, -20, 0, 0.1),
	AnchorPoint = Vector2(0,0),
	Position = UICoords(10,10, 0, 0),
	TextColor = Color.White,
	Font = GraphicsEngine.Instance.Fonts.Arial30,
	BorderSize = 0,
	TextWrap = false,
	TextXAlign = TextXAlignment.Center,
	TextYAlign = TextYAlignment.Center,
})

local worldListing = UIRect(script, {
	Parent = SingleplayerMenu,
	Size = UICoords(0, 0, 0.8, 0.65),
	Position = UICoords(0, 0, 0.1, 0.1)

});


local bottomToolbar = UIRect(script, {
	Parent = SingleplayerMenu,
	Size = UICoords(0, 0, 0.8, 0.1),
	Position = UICoords(0, 0, 0.1, 0.8)
});

local backbutton = GetButton("BACK");
backbutton.Parent = SingleplayerMenu;
backbutton.Size = UICoords(100, 30, 0, 0);
backbutton.Position = UICoords(10, 0, 0, 0.95);

backbutton.OnLMBClick:Bind(function()
	menumanager.CurrentPage = menumanager.Pages["mainmenu"];
end)


return SingleplayerMenu;