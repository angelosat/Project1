using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Interactions;

namespace Project1.Core.Systems.Magic;

public static class SpellDefOf
{
    public static readonly SpellDef Healing = new("Healing", SpellSchoolDefOf.Holy, typeof(SpellWorker_Healing), 5);
    public static readonly SpellDef Teleporting = new("Teleporting", SpellSchoolDefOf.Common, typeof(SpellWorker_Healing), 5);
    static public readonly PlanDef PlanCastSpell = new("CastSpell", typeof(BehaviorExecutePlanNew), InteractionDefOf.CastSpell);

    static SpellDefOf()
    {
        Def.Register(typeof(SpellDefOf));
    }
}