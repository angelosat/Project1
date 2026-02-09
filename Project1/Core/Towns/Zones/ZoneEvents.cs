using Project1.Core.Entities;
using Project1.Framework.Events;

namespace Project1.Core.Towns.Zones
{
    public record struct ZoneCreatedEvent(Zone Zone) : IEventPayload { }
    public record struct ZoneDeletedEvent(Zone Zone) : IEventPayload { }
    internal record struct EntityEnteredZoneEvent(Entity Entity, Zone Zone) : IEventPayload { }
    internal record struct EntityExitedZoneEvent(Entity Entity, Zone Zone) : IEventPayload { }
}
