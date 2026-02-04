using Project1.Framework.Base;

namespace Start_a_Town_
{
    public record struct ZoneCreatedEvent(Zone Zone) : IEventPayload { }
    public record struct ZoneDeletedEvent(Zone Zone) : IEventPayload { }
}
