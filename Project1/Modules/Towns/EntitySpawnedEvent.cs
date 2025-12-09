namespace Start_a_Town_
{
    internal class EntitySpawnedEvent(Entity entity) : EventPayloadBase
    {
        public readonly Entity Entity = entity;
    }
}