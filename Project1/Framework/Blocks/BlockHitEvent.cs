using Project1.Framework.Base;
using Project1.Framework.WorldGen;

namespace Project1.Framework.Blocks
{
    public record struct BlockHitEvent(Block Block, MapBase Map, IntVec3 Global, int Amount) : IEventPayload { }
}
