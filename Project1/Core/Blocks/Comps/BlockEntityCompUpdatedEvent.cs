namespace Start_a_Town_
{
    public record struct BlockEntityCompUpdatedEvent(BlockEntityComp Comp) : IEventPayload { }
    public record struct BlockEntityUpdatedEvent(BlockEntity Entity) : IEventPayload { }
}
