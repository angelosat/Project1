using System;

namespace Project1.Core.World.Actions;

public abstract class WorldActionWorker
{
    internal abstract Type DataType { get; }// => typeof(WorldActionData);

    internal virtual float GetDurationMultiplier(WorldActionData data)
        => 1;

    internal virtual string GetStatusText(WorldActionRuntime runtime)
        => runtime.Def.LabelReadable;

    internal abstract void Resolve(WorldActionData data);
}
