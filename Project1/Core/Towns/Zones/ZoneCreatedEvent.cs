using Project1.Core.Base;

namespace Project1.Core.Towns.Zones
{
    public record struct ZoneCreatedEvent(Zone Zone) : IEventPayload { }
    public record struct ZoneDeletedEvent(Zone Zone) : IEventPayload { }
}
