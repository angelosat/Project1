namespace Start_a_Town_
{
    public record struct BlockHitEvent(Block Block, MapBase Map, IntVec3 Global, int Amount) : IEventPayload { }
}
