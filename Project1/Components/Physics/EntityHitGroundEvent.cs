namespace Start_a_Town_
{
    class EntityHitGroundEvent(Entity entity, float force) : EventPayloadBase
    {
        public Entity Entity = entity;
        public float Force = force;
    }
}
