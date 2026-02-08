using Project1.Core.Base;
using Project1.Core.Simulation;
using Project1.Framework.Math;

namespace Project1.Core.Blocks
{
    public sealed record BlockDestroyedEvent(Block Block, MapBase Map, IntVec3 Global) : IEventPayload { }
}
