using Start_a_Town_;

namespace Project1.Framework.Blocks
{
    internal record struct BlockEntityRemovedEvent(BlockEntity Entity) : IEventPayload { }
    internal record struct BlockEntityAddedEvent(BlockEntity Entity) : IEventPayload { }
}