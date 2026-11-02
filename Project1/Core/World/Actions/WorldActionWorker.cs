using Project1.Core.Entities.Actors;
using Project1.Core.World.Frontiers;

namespace Project1.Core.World.Actions;

public abstract class WorldActionWorker
{
    internal virtual WorldActionData CreateData(FrontierRuntime frontier, Actor actor)
        => new(frontier, actor);

    internal virtual float GetDurationMultiplier(WorldActionData data)
        => 1;

    internal virtual string GetStatusText(WorldActionRuntime runtime)
        => runtime.Def.LabelReadable;

    internal abstract void Resolve(WorldActionData data);


}
