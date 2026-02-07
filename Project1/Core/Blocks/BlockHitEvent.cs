using Project1.Core.Base;
using Project1.Core.Simulation;

namespace Project1.Core.Blocks
{
    public record struct BlockHitEvent(Block Block, MapBase Map, IntVec3 Global, int Amount) : IEventPayload { }
}
