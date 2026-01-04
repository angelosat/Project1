namespace Start_a_Town_
{
    internal sealed record BlockEntityRemovedEvent(BlockEntity Entity) : IEventPayload { }
    internal sealed record BlockEntityAddedEvent(BlockEntity Entity) : IEventPayload { }
}