using Project1.Core.Entities.Actors;
using Project1.Core.World.Frontiers;

namespace Project1.Core.World.Actions;

internal class WorldActionData(FrontierRuntime frontier, Actor actor)
{
    internal readonly FrontierRuntime Frontier = frontier;
    internal readonly Actor Actor = actor;
}
