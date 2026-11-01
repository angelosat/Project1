using Project1.Core.Entities.Actors;
using Project1.Core.World.Frontiers;
using Project1.Framework.Helpers;

namespace Project1.Core.World.Actions;

public abstract class WorldActionWorker
{
    internal virtual int GetDurationMultiplier(WorldActionData data)
        => 1;

    internal virtual string GetStatusText(WorldActionRuntime runtime)
        => runtime.Def.LabelReadable;

    internal abstract void Resolve(WorldActionData data);


}
