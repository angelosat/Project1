using System.Collections.Generic;
using Project1.Core.Resources;
using Project1.Core.Simulation;

namespace Project1.Core.Entities
{
    internal class EntityLifecycleSystem : SimulationSystem
    {
        readonly Queue<Entity> KilledEntities = [];
        public EntityLifecycleSystem(MapBase map) : base(map)
        {
            map.Events.ListenTo<EntityKilledEvent>(OnEntityKilled);
            map.World.Events.ListenTo<ResourceModifiedEvent>(OnEntityResourceAdjusted);
        }
        private void OnEntityResourceAdjusted(ResourceModifiedEvent e)
        {
            if (e.Def != ResourceDefOf.HitPoints)
                return;
            if (e.Delta > 0)
                return;
            this.KilledEntities.Enqueue(e.Entity);
        }

        private void OnEntityKilled(EntityKilledEvent @event)
        {
            if (this.Map.Net.IsClient)
                return;
            this.KilledEntities.Enqueue(@event.Entity);
        }
        public override void Tick()
        {
            if (this.Map.Net.IsClient)
                return;
            while (this.KilledEntities.Count > 0)
            {
                var entity = this.KilledEntities.Dequeue();
                entity.Kill();
                this.Map.Despawn(entity);
                this.Map.World.DisposeEntity(entity);
            }
        }
    }
    //internal class EntityLifecycleManager : MapComponent
    //{
    //    readonly Queue<Entity> KilledEntities = [];
    //    public EntityLifecycleManager(MapBase map) : base(map)
    //    {
    //        map.Events.ListenTo<EntityKilledEvent>(OnEntityKilled);
    //        map.World.Events.ListenTo<ResourceModifiedEvent>(OnEntityResourceAdjusted);
    //    }

    //    private void OnEntityResourceAdjusted(ResourceModifiedEvent e)
    //    {
    //        if (e.Def != ResourceDefOf.HitPoints)
    //            return;
    //        if (e.Delta > 0)
    //            return;
    //        this.KilledEntities.Enqueue(e.Entity);
    //    }

    //    private void OnEntityKilled(EntityKilledEvent @event)
    //    {
    //        if (this.Map.Net.IsClient)
    //            return;
    //        this.KilledEntities.Enqueue(@event.Entity);
    //    }
    //    public override void Tick()
    //    {
    //        if (this.Map.Net.IsClient)
    //            return;
    //        while (this.KilledEntities.Count > 0)
    //        {
    //            var entity = this.KilledEntities.Dequeue();
    //            entity.Kill();
    //            this.Map.Despawn(entity);
    //            this.Map.World.DisposeEntity(entity);
    //        }
    //    }
    //}
}