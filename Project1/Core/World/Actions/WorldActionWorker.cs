namespace Project1.Core.World.Actions;

public abstract class WorldActionWorker
{
    internal virtual int GetDurationMultiplier(WorldActionData data)
        => 1;

    internal abstract void Resolve(WorldActionData data);
}
