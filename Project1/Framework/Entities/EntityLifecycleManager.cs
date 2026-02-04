using Project1.Framework.StaticMaps.Components;
using Project1.Framework.WorldGen;
using System.Collections.Generic;

namespace Project1.Framework.Entities
{
    internal class EntityLifecycleManager : MapComponent
    {
        readonly Queue<Entity> KilledEntities = [];
        public EntityLifecycleManager(MapBase map) : base(map)
        {
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
