using Project1.Core.Entities;
using Project1.Framework.Events;

namespace Project1.Core.Simulation.Physics;

public record struct EntityAtRestEvent(Entity Entity, bool AtRest) : IEventPayload;
public record struct EntityCollisionEvent(Entity Source, Entity Target) : IEventPayload;
public record struct EntityHitGroundEvent(Entity Entity, float Force) : IEventPayload;
