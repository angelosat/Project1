using Project1.Framework;
using System;

namespace Project1.Core.World.Actions;

public sealed class WorldActionDef(string name, Type workerType, int duration = 0) : Def(name)
{
    public readonly int BaseDuration = duration;
    public readonly WorldActionWorker Worker = (WorldActionWorker)Activator.CreateInstance(workerType);
}
