namespace Start_a_Town_
{
    internal class EntityDespawnedEvent(Entity entity) : EventPayloadBase
    {
        public readonly Entity Entity = entity;
    }
    internal class ZoneDeletedEvent(Zone zone) : EventPayloadBase
    {
        public readonly Zone Zone = zone;
    }
}