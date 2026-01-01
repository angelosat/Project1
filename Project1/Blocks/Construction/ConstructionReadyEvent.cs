namespace Start_a_Town_
{
    public sealed record ConstructionReadyEvent(BlockConstructionComp Source) : IEventPayload { }
    public sealed record ConstructionFinishedEvent(BlockConstructionComp Source) : IEventPayload { }
    public sealed record ConstructionUpdatedEvent(BlockConstructionComp Source) : IEventPayload { }
}
