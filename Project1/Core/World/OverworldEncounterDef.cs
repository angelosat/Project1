using Project1.Framework.Helpers;
using System;

#nullable enable

namespace Project1.Core.World
{
    public sealed class OverworldEncounterDef(string name, Type workerType, int ticks = 0/*, Type runtimeType*/) : Def(name)
    {
        //public readonly Type RuntimeType = runtimeType;
        internal OffmapActivity Worker => field ??= ActivatorSafe<OffmapActivity>.CreateInstance(workerType);
        internal int Duration = ticks;
    }
}
