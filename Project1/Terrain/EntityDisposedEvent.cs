namespace Start_a_Town_
{
    internal class EntityDisposedEvent(Entity entity) : IEventPayload
    {
        public readonly Entity Entity = entity;
    }
}