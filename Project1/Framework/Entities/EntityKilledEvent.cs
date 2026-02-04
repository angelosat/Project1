using Project1.Framework.Base;

namespace Project1.Framework.Entities
{
    public record struct EntityKilledEvent(Entity Entity) : IEventPayload { }
}
