namespace Start_a_Town_
{
    internal class EntityDisposedEvent(Entity entity) : EventPayloadBase
    {
        public readonly Entity Entity = entity;
    }
}