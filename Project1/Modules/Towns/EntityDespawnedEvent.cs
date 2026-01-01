namespace Start_a_Town_
{
    internal class EntityDespawnedEvent(Entity entity) : IEventPayload
    {
        public readonly Entity Entity = entity;
    }
    internal class ZoneDeletedEvent(Zone zone) : IEventPayload
    {
        public readonly Zone Zone = zone;
    }
}