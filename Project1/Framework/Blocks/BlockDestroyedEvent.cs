using Project1.Framework.Base;
using Project1.Framework.WorldGen;
using Start_a_Town_;

namespace Project1.Framework.Blocks
{
    public sealed record BlockDestroyedEvent(Block Block, MapBase Map, IntVec3 Global) : IEventPayload { }
}
