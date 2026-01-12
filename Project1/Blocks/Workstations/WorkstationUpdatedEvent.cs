namespace Start_a_Town_
{
    public record struct WorkstationUpdatedEvent(BlockWorkstationComp Comp) : IEventPayload { }
}
