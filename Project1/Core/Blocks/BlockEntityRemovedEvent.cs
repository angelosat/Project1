using Project1.Core.Base;
using Project1.Core;

namespace Project1.Core.Blocks
{
    internal record struct BlockEntityRemovedEvent(BlockEntity Entity) : IEventPayload { }
    internal record struct BlockEntityAddedEvent(BlockEntity Entity) : IEventPayload { }
}