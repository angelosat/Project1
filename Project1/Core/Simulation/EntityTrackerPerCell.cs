using Project1.Core.Entities;
using Project1.Framework;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Project1.Core.Simulation
{
    public sealed class EntityTrackerPerCell(MapBase map) : MapComponent(map), IEntityTracker
    {
        readonly Dictionary<IntVec3, HashSet<Entity>> Internal = [];
        //readonly Dictionary<IntVec3, HashSet<EntityRefId>> InternalRefs = [];

        private static readonly IReadOnlySet<Entity> Empty = ImmutableHashSet<Entity>.Empty;
        private static readonly IReadOnlySet<EntityRefId> EmptyRefs = ImmutableHashSet<EntityRefId>.Empty;

        public IReadOnlySet<Entity> GetEntitiesAt(IntVec3 cell)
            => this.Internal.TryGetValue(cell, out var list) ? list : Empty;
        //public IReadOnlySet<EntityRefId> GetEntitiesRefsAt(IntVec3 cell)
        //    => this.InternalRefs.TryGetValue(cell, out var list) ? list : EmptyRefs;

        public void OnEntityMoved(Entity entity, IntVec3 lastCell, IntVec3 nextCell)
        {
            if (lastCell == nextCell)
                return;
            this.Remove(entity, lastCell);
            this.Add(entity, nextCell);
        }

        private void Add(Entity entity, IntVec3 nextCell)
        {
            if (!this.Internal.TryGetValue(nextCell, out var nextlist))
                this.Internal[nextCell] = nextlist = [];
            nextlist.Add(entity);
        }

        private void Remove(Entity entity, IntVec3 lastCell)
        {
            if (this.Internal.TryGetValue(lastCell, out var list))
            {
                list.Remove(entity);
                if (list.Count == 0)
                    this.Internal.Remove(lastCell);
            }
        }

        public void OnEntitySpawned(Entity entity)
        {
            this.Add(entity, entity.Global);
        }

        public void OnEntityDespawned(Entity entity)
        {
            this.Remove(entity, entity.Global);
        }

        protected internal override void ResolveReferences()
        {
            foreach (var entity in this.Map.Entities)
                this.Add(entity, entity.Global);
        }
    }
}
