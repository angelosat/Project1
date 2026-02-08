using Project1.Core.Base;
using Project1.Core.Materials;
using Project1.Core.Net.Packets;
using Project1.Framework.Math;

namespace Project1.Core.Blocks
{
    public record struct BlockSetEvent(SetBlockArgs args) : IEventPayload { }
    internal record struct PlayerPaintedBlockEvent(IntVec3 Global, Block Block, MaterialDef Material, byte State, int Variation, int Orientation) : IEventPayload { }
}
