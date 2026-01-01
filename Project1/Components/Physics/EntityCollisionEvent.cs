namespace Start_a_Town_
{
    class EntityCollisionEvent(Entity source, Entity target) : IEventPayload
    {
        public readonly Entity Source = source, Target = target;
    }
}
