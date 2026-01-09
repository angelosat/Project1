namespace Start_a_Town_
{
    public record struct BlockSetEvent(SetBlockArgs args) : IEventPayload { }
    internal record struct PlayerPaintedBlockEvent(IntVec3 Global, Block Block, MaterialDef Material, byte State, int Variation, int Orientation) : IEventPayload { }
}
