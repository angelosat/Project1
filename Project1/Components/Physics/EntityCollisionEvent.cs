namespace Start_a_Town_
{
    class EntityCollisionEvent(Entity source, Entity target) : EventPayloadBase
    {
        public readonly Entity Source = source, Target = target;
    }
}
