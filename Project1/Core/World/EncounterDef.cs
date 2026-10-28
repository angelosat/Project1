using Project1.Framework.Helpers;
using System;

#nullable enable

namespace Project1.Core.World;

public sealed class EncounterDef(string name, Type workerType, int minutes = 0/*, Type runtimeType*/) : Def(name)
{
    //public readonly Type RuntimeType = runtimeType;
    internal EncounterWorker Worker => field ??= ActivatorSafe<EncounterWorker>.CreateInstance(workerType);
    internal int DurationInMinutes = Ticks.FromMinutes(minutes);
}
