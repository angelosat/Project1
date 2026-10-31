using Project1.Core.Entities;
using Project1.Framework.UI;

namespace Project1.Core.Simulation;

public abstract class WorldComp(WorldBase world)
{
    //protected WorldBase World = world;
    protected StaticWorld World = (StaticWorld)world;

    internal virtual void Scan(Entity entity) { }

    internal virtual void EnsureInitialized() { }

    internal virtual void Tick() { }

    internal protected virtual (string label, Control control)? GetGui() => null;
}
