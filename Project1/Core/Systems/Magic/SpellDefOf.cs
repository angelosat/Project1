using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Interactions;
using Project1.Core.Resources;
using Project1.Core.Systems.Effects;
using Project1.Framework;

namespace Project1.Core.Systems.Magic;

[EnsureStaticCtorCall]
public static class SpellDefOf
{
    public static readonly SpellDef Healing = new("Healing", 
        TargetType.Entity, 
        subject: SpellSubject.Any,
        school: SpellSchoolDefOf.Holy, 
        //typeof(SpellWorker_RestoreHealth), 
        manaCost: 20, 
        castTimeInSecs: 5, 
        //effectDuration: 0,
        [(EffectDefOf.RestoreResource, ResourceDefOf.Health)]);
    public static readonly SpellDef MaxHealthBuff = new("MaxHealthBuff", 
        TargetType.Entity,
        subject: SpellSubject.Any,
        SpellSchoolDefOf.Holy, 
        //typeof(SpellWorker_FortifyHealth), 
        manaCost: 10, 
        castTimeInSecs: 1, 
        //effectDuration: Ticks.FromDays(1),
        [(EffectDefOf.FortifyResource, ResourceDefOf.Health)]);
    public static readonly SpellDef Teleporting = new("Teleporting", 
        TargetType.Entity,
        subject: SpellSubject.Self,
        SpellSchoolDefOf.Common, 
        //typeof(SpellWorker_Null),
        manaCost: 0, 
        castTimeInSecs: 5, 
        //effectDuration: 0,
        []);

    static public readonly PlanDef PlanCastSpell = new("CastSpell", typeof(BehaviorExecutePlanNew), InteractionDefOf.CastSpell);

    static SpellDefOf()
    {
        Def.Register(typeof(SpellDefOf));
    }
}