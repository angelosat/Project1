using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Interactions;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Systems.Magic;

public sealed class SpellDef(string name, Type workerType, int durationSeconds) : Def(name)
{
    public readonly int DurationSeconds = durationSeconds;
    public readonly SpellWorker Worker = ActivatorSafe<SpellWorker>.CreateInstance(workerType);
}

public static class SpellDefOf
{
    public static readonly SpellDef Healing = new("Healing", typeof(SpellWorker_Healing), 5);
    static public readonly PlanDef PlanCastSpell = new("CastSpell", typeof(BehaviorExecutePlanNew), InteractionDefOf.CastSpell);

    static SpellDefOf()
    {
        Def.Register(typeof(SpellDefOf));
    }
}