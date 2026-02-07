using Project1.Core.Base;

namespace Project1.Core.Entities
{
    public record struct EntityKilledEvent(Entity Entity) : IEventPayload { }
}
