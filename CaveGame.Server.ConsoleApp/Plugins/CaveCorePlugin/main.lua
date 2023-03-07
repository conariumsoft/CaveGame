--
import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import('CaveGame', 'CaveGame.Common')
import('CaveGame', 'CaveGame.Server')
import('CaveGame', 'CaveGame.Common.Game.Entities')
import('CaveGame', 'CaveGame.Common.Game')

plugin.OnPluginLoad:Bind(function(event)
    print("PluginLoad!")
end)
plugin.OnPluginUnload:Bind(function(event)
    print("PluginUnload!")
end)

local binding = server.OnChatMessageFromClient:Bind(function(event)

    if event.Message == "!kek" then
        server:Chat("test", Color(255,255,0))
        event.Cancelled = true
    end
end)


local function getuser(name)
    for user in list(server.ConnectedUsers) do
            
        if user.Username == name then
           return user
        end
    end
end

local function combine(stringlist, start)
    local messg = ""

    if (#stringlist > start) then
        for i = start, #stringlist do
            messg = messg .. stringlist[i]
            if (i< #stringlist) then
                messg = messg .. " "
            end
        end
    end
   

    return messg
end

server.OnServerCommand:Bind(function(commandEvent)
    local command = commandEvent.Command;
    local args = commandEvent.Arguments;



    
    if command == "list" then
        for definition in list(server.Commands) do

            local str = ""
            for v in list(definition.Args) do
                str = str .. "<" .. v .. "> "
            end

            server.Output:Out("%2"..definition.Keyword .. " "..str .. " %f("..definition.Description..")")
            --server.Output:Out()
        end
    end

    if command == "version" then

    end

    if command == "kick" then
        local targetName = args[1];
        local user = getuser(targetName)
        if user then
            
        end
        for user in list(server.ConnectedUsers) do
            
            if user.Username == targetName then
               
                local reason = combine(definition.Args, 2) or "Kicked by server!";
                user:Kick(reason);
                server:Chat(user.Username .. " has been kicked from the server.", Color(255,255,255));
                return;
            end
        end
    end

    if command == "plugins" then
        for plug in list(server.PluginManager.Plugins) do

            server.Output:Out("%2"..plug.PluginName .. "%f version "..plug.PluginVersion.." by "..plug.PluginAuthor .. " %3["..plug.PluginDescription.."]%f")
        end
    end

    if command == "msg" or command == "pm" then
        local targetName = args[1]

        --local message = combine
          local user = getuser(targetName)
          
          if user then
              server:Message(user, combine(args, 2))
              return t;
          end
          server.Output:Out("%4Player not found!")

    end
    commandEvent:SetCancelled(true) -- invoke warning about nonexistant command?
end)

server:BindCommand("kick", "Kicks a player", {"playername", "reason"})
server:BindCommand("list", "Shows all commands", {})
server:BindCommand("playerlist", "Shows all commands", {})
server:BindCommand("shutdown", "Stop the server", {})
server:BindCommand("version", "Shows server version", {})
server:BindCommand("plugins", "Lists plugins", {})
server:BindCommand("time", "Sets the time", {})
server:BindCommand("seed", "Displays game seed", {})
server:BindCommand("whois", "Lists information about a player", {})
server:BindCommand("pm", "Send a private message", {})
server:BindCommand("msg", "Send a private message", {"player", "message"})