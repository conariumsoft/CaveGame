using System;
using System.Collections.Generic;
using System.Text;
using CaveGame.Core;

namespace CaveGame.Server
{
    public class LocalServer : GameServer
    {
        public LocalServer(ServerConfig config, WorldMetadata worldMDT) : base(config, worldMDT)
        {

        } 
    }
}
