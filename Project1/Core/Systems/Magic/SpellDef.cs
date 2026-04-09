using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Systems.Magic;

public sealed class SpellDef(string name, SpellSchoolDef school, Type workerType, int durationSeconds) : Def(name)
{
    public readonly SpellSchoolDef School = school;
    public readonly int DurationSeconds = durationSeconds;
    public readonly SpellWorker Worker = ActivatorSafe<SpellWorker>.CreateInstance(workerType);
}
