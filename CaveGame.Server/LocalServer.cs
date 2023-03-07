using System;
using System.Collections.Generic;
using System.Text;
using CaveGame.Common;
using CaveGame.Common.World;

namespace CaveGame.Server
{
    public class LocalServer : GameServer
    {
        public LocalServer(ServerConfig config, WorldMetadata worldMDT) : base(config, worldMDT)
        {

        } 
    }
}
