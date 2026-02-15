using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Framework.Events;

namespace Project1.Core.Entities
{
    internal record struct EntityStackDecreased(Entity Entity, int Amount) : IEventPayload { }
    internal record struct EntityStackIncreased(Entity Entity, int Amount) : IEventPayload { }
    internal record struct EntityRegisteredEvent(Entity Entity, bool Immediate = false) : IEventPayload { }
    internal record struct EntityDisposedEvent(Entity Entity) : IEventPayload { }
    internal record struct EntitySpawnedEvent(Entity Entity, bool Immediate = false) : IEventPayload { }
    internal record struct EntityDespawnedEvent(Entity Entity) : IEventPayload { }
    internal record struct EntityForbiddenEvent(Entity Entity) : IEventPayload { }
    internal record struct ActorGearUpdatedEvent(Actor Actor, Entity NewItem, Entity OldItem) : IEventPayload { }
    internal record struct ActorNeedOverridenEvent(Actor Actor, NeedDef Need, float Percentage) : IEventPayload { }
    internal record struct ActorNeedUpdatedEvent(Need Need) : IEventPayload { }
    internal record struct EntityCompUpdatedEvent(EntityComp Comp) : IEventPayload { }
    internal record struct EntityKilledEvent(Entity Entity) : IEventPayload { }
    internal record struct EntityFootStepEvent(Entity Entity) : IEventPayload { }
}
