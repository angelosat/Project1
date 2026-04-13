using Project1.Core.Entities;

namespace Project1.Core.Simulation;

internal abstract class WorldComp(WorldBase world)
{
    protected WorldBase World = world;

    internal virtual void Scan(Entity entity) { }
}
