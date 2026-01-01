namespace Start_a_Town_
{
    class EntityHitGroundEvent(Entity entity, float force) : IEventPayload
    {
        public Entity Entity = entity;
        public float Force = force;
    }
}
