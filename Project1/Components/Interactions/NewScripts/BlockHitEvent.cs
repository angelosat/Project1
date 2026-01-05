namespace Start_a_Town_
{
    public sealed record BlockHitEvent(Block Block, MapBase Map, IntVec3 Global, int Amount) : IEventPayload { }
}
