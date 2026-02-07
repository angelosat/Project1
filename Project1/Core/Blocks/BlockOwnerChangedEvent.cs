using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core;

namespace Project1.Core.Blocks
{
    public record struct BlockOwnerChangedEvent(BlockEntity Entity, Actor Actor) : IEventPayload { }
    public record struct PlayerChangedBlockOwnerEvent(BlockEntity Entity, Actor Actor) : IEventPayload { }
}
