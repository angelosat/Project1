using Project1.Framework.Blocks;

namespace Start_a_Town_
{
    public sealed record BlockDestroyedEvent(Block Block, MapBase Map, IntVec3 Global) : IEventPayload { }
}
