import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame', 'CaveGame.Client')
import ('CaveGame', 'CaveGame.Client.UI')
import ('CaveGame', 'CaveGame.Core')
import ('CaveGame', 'CaveGame.Client.Menu')

local GetButton = require("Assets.Scripts.MenuScripts.MenuButton")

local SingleplayerMenu = UIRoot();



local title = Label(script, {
	Parent = SingleplayerMenu,
	Text = 'SINGLEPLAYER',
	BGColor = Color.Transparent,
	Size = UICoords(0, 0, 0.4, 0.1),
	AnchorPoint = Vector2(0.5,0.5),
	Position = UICoords(0,0, 0.5, 0.05),
	TextColor = Color.White,
	Font = GraphicsEngine.Instance.Fonts.Arial30,
	BorderSize = 0,
	TextWrap = false,
	TextXAlign = TextXAlignment.Center,
	TextYAlign = TextYAlignment.Center,
})

local worldListing = ScrollRect(script, {
	Parent = SingleplayerMenu,
	Size = UICoords(0, 0, 0.8, 0.65),
	Position = UICoords(0, 0, 0.1, 0.1),
	BGColor = Color(0,0,0.4),
	CanvasSize = 8.0,
	ScrollbarWidth = 16,
	CanvasPosition = 0.0,
});

local selectedWorld = nil;


local bottomToolbar = UIRect(script, {
	Parent = SingleplayerMenu,
	Size = UICoords(0, 0, 0.8, 0.1),
	Position = UICoords(0, 0, 0.1, 0.8),
	BGColor = Color(0,0,0),
});

local function toolbarButton(text, index)
	return TextButton(script, {
		Parent = bottomToolbar,
		Size = UICoords(0, 0, 0.26, 0.8),
		AnchorPoint = Vector2(0.5, 0.5),
		Position = UICoords(0, 0, 0.5 + (0.34*index), .5),
		UnselectedBGColor = Color(0.2, 0.2, 0.2),
		SelectedBGColor = Color(0.1, 0.1, 0.1),
		TextColor = Color.White,
		Font = GraphicsEngine.Instance.Fonts.Arial14,
		Text = text,
		TextXAlign = TextXAlignment.Center,
		TextYAlign = TextYAlignment.Center,
	});
end

local loadWorld = toolbarButton("Load World", -1);

loadWorld.OnLMBClick:Bind(function()
	if selectedWorld then
		print("LOAD "..selectedWorld.Name );

		game:EnterLocalGame(selectedWorld); 

	end
end)

local deleteWorld = toolbarButton("Delete World", 0);


local function refresh()
for obj in list(worldListing:FindChildrenWithName("Entry")) do
		worldListing.Children:Remove(obj);
		obj.Parent = nil;
	end
	-- TODO: Load the world listing
	for obj in list(SavedWorldManager.GetWorldsOnFile()) do
		local instance = ContextButton(script, {
			Parent = worldListing,
			Size = UICoords(0, 60, 1.0, 0),
			Position = UICoords(0, 0, 0, 0),
			UnselectedBGColor = Color(0.05, 0.05, 0.05),
			SelectedBGColor = Color(0.2, 0.2, 0.2),
			ActivatedBGColor = Color(0.3, 0.1, 0.1),
			Name = "Entry",
		});

		local text = Label(script, {
			Name = "WorldName",
			Parent = instance,
			Size = UICoords(-10, 0, 0.5, 1),
			Position = UICoords(10,0,0,0),
			Font = GraphicsEngine.Instance.Fonts.Arial20,
			Text = obj.Name,
			TextColor = Color.White,
			TextYAlign = TextYAlignment.Center,
		});

		instance.OnSelected:Bind(function()
			selectedWorld = obj
		end)
		instance.OnUnselected:Bind(function()
			selectedWorld = nil
		end)
		instance.ContextNodes:Add(loadWorld);
	end

end

deleteWorld.OnLMBClick:Bind(function()
	if not selectedWorld == nil then
		SavedWorldManager.DeleteSave(selectedWorld)
		refresh()
	end
end)


local newWorld = toolbarButton("New World", 1);

newWorld.OnLMBClick:Bind(function()
	menumanager.CurrentPage = menumanager.Pages["newworldmenu"]
end)


SingleplayerMenu.OnLoad:Bind(function(ev)
	refresh()
end)



SingleplayerMenu.OnUnload:Bind(function(ev)
	
end)






local backbutton = GetButton("BACK");
backbutton.Parent = SingleplayerMenu;
backbutton.Size = UICoords(100, 30, 0, 0);
backbutton.Position = UICoords(10, 0, 0, 0.95);

backbutton.OnLMBClick:Bind(function()
	menumanager.CurrentPage = menumanager.Pages["mainmenu"];
end)


return SingleplayerMenu;