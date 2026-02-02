using Start_a_Town_;

namespace Project1.Framework.Physics
{
    public record struct EntityAtRestEvent(Entity Entity, bool AtRest) : IEventPayload { }
    public record struct EntityCollisionEvent(Entity Source, Entity Target) : IEventPayload { }
    public record struct EntityHitGroundEvent(Entity Entity, float Force) : IEventPayload { }

}
