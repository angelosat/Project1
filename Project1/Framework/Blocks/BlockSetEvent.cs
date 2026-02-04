using Project1.Framework.Base;
using Project1.Framework.Materials;
using Project1.Framework.Net.Packets;

namespace Project1.Framework.Blocks
{
    public record struct BlockSetEvent(SetBlockArgs args) : IEventPayload { }
    internal record struct PlayerPaintedBlockEvent(IntVec3 Global, Block Block, MaterialDef Material, byte State, int Variation, int Orientation) : IEventPayload { }
}
