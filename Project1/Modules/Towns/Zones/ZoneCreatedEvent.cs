namespace Start_a_Town_
{
    public sealed record ZoneCreatedEvent(Zone Zone) : IEventPayload { }
    public sealed record ZoneDeletedEvent(Zone Zone) : IEventPayload { }
}
