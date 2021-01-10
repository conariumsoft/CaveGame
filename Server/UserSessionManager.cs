using CaveGame.Core.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Server
{
    //
    public class UserSessionManager
    {


        public GameServer Server { get; private set; }
        public UserSessionManager(GameServer server)
        {
            Server = server;
        }


        public void HandleDisconnect(User user)
        {

        }

    }
}
