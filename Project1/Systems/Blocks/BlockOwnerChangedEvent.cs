namespace Start_a_Town_
{
    public record struct BlockOwnerChangedEvent(BlockEntity Entity, Actor Actor) : IEventPayload { }
    public record struct PlayerChangedBlockOwnerEvent(BlockEntity Entity, Actor Actor) : IEventPayload { }
}
