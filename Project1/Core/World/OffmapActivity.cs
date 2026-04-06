using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.World;

internal abstract class OffmapActivity
{
    internal abstract void Tick(FrontierWrapper frontier, Actor actor);
}
