namespace Start_a_Town_
{
    public sealed record BlockHitEvent(MapBase Map, IntVec3 Global) : IEventPayload { }
}
