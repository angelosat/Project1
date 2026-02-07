using Project1.Core.Base;

namespace Project1.Core
{
    public record struct BlockEntityCompUpdatedEvent(BlockEntityComp Comp) : IEventPayload { }
    public record struct BlockEntityUpdatedEvent(BlockEntity Entity) : IEventPayload { }
}
