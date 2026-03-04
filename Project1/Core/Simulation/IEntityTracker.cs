using Project1.Core.Entities;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Simulation
{
    internal interface IEntityTracker
    {
        void OnEntitySpawned(Entity entity);
        void OnEntityDespawned(Entity entity);
        void OnEntityMoved(Entity entity, IntVec3 lastCell, IntVec3 nextCell);
        IReadOnlySet<Entity> GetEntitiesAt(IntVec3 cell);
    }
}
