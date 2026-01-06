using System.Collections.Generic;

namespace Start_a_Town_
{
    internal class EntityLifecycleManager : MapComponent
    {
        readonly MapBase Map;
        readonly Queue<Entity> KilledEntities = [];
        public EntityLifecycleManager(MapBase map)
        {
            this.Map = map;
            map.Events.ListenTo<EntityKilledEvent>(OnEntityKilled);
        }

        private void OnEntityKilled(EntityKilledEvent @event)
        {
            this.KilledEntities.Enqueue(@event.Entity);
        }

        public override void Tick()
        {
            while (this.KilledEntities.Count > 0)
            {
                var entity = this.KilledEntities.Dequeue();
                this.Map.Despawn(entity);
                this.Map.World.DisposeEntity(entity);
            }
        }
    }
}
