using Project1.Core.Base;

namespace Project1.Core.Entities
{
    internal record struct EntityStackDecreased(Entity Entity, int Amount) : IEventPayload { }
    internal record struct EntityStackIncreased(Entity Entity, int Amount) : IEventPayload { }
    internal record struct EntityRegisteredEvent(Entity Entity, bool Immediate = false) : IEventPayload { }
    internal record struct EntityDisposedEvent(Entity Entity) : IEventPayload { }
    internal record struct EntitySpawnedEvent(Entity Entity, bool Immediate = false) : IEventPayload { }
    internal record struct EntityDespawnedEvent(Entity Entity) : IEventPayload { }
}
