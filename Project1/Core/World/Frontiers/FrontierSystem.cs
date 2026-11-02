using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;

#nullable enable

namespace Project1.Core.World.Frontiers;

public static class FrontierSystem
{
    extension(Actor actor)
    {
        public FrontierRuntime? Frontier => ((StaticWorld)actor.World).Frontiers.GetFrontier(actor);
    }
}
