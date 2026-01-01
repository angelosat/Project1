namespace Start_a_Town_
{
    internal class EntitySpawnedEvent(Entity entity, bool immediate = false) : IEventPayload
    {
        public readonly Entity Entity = entity;
        public readonly bool Immediate = immediate;
    }
}