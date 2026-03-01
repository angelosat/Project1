using Project1.Core.Entities;

namespace Project1.Core.Simulation.Physics
{
    internal class CollisionSystem(MapBase map) : SimulationSystem(map)
    {
        public void Handle(Entity a, Entity b)
        {
            if (this.Map.Net.IsClient)
                return;
            if (!b.CanAbsorb(a))
                return;
            if (a.IsReserved)
                return;
            /// revmoved the reserved check from canabsorb and placed it here, because canabsorb is called during legit behaviors that involve the items, 
            /// which means the items are reserved but still should be absorbable
            b.Add(a.StackSize);
            this.Map.World.DisposeEntity(a);
            this.Map.Events.Post(new EntityCollisionEvent(a, b));
        }
    }
}
