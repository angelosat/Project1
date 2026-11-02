using System;

namespace Project1.Core.World.Actions;

public sealed class WorldActionDef(string name, Type workerType, Type dataType = null, int duration = 0) : Def(name)
{
    public readonly int BaseDuration = duration;
    public readonly WorldActionWorker Worker = (WorldActionWorker)Activator.CreateInstance(workerType);
    public readonly Type DataType = dataType ?? typeof(WorldActionData);
}
