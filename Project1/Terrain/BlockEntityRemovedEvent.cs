namespace Start_a_Town_
{
    internal record struct BlockEntityRemovedEvent(BlockEntity Entity) : IEventPayload { }
    internal record struct BlockEntityAddedEvent(BlockEntity Entity) : IEventPayload { }
}