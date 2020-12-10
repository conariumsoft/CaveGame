using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Entities
{
    public class Arrow : PhysicsEntity, IServerPhysicsObserver, IClientPhysicsObserver
    {
        public override float Mass => 0.2f

        public void ClientPhysicsTick(IClientWorld world, float step) => PhysicsStep(world, step);
        public void ServerPhysicsTick(IServerWorld world, float step) => PhysicsStep(world, step);
    
        
    
    }
}
