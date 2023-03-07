namespace CaveGame.Server
{
    public class ChunkManager
    {
        protected GameServer Server { get; private set; }
        public ChunkManager(GameServer server)
        {
            Server = server;
        }



    }
}