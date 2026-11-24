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
        manaCost: 20, 
        castTimeInSecs: 5, 
        [new(EffectDefOf.RestoreResource, ResourceDefOf.Health, 50)]);

    public static readonly SpellDef HealingPotion = new("HealingPotion",
    TargetType.Entity,
    subject: SpellSubject.Any,
    school: SpellSchoolDefOf.Holy,
    manaCost: 20,
    castTimeInSecs: 5,
    [new(EffectDefOf.RestoreResource, ResourceDefOf.Health, 10),
     new(EffectDefOf.PotionCooldown, null, 30)]);

    public static readonly SpellDef MaxHealthBuff = new("MaxHealthBuff", 
        TargetType.Entity,
        subject: SpellSubject.Any,
        SpellSchoolDefOf.Holy, 
        manaCost: 10, 
        castTimeInSecs: 1, 
        [new(EffectDefOf.FortifyResource, ResourceDefOf.Health, 10)]);
    public static readonly SpellDef Teleporting = new("Teleporting", 
        TargetType.Entity,
        subject: SpellSubject.Self,
        SpellSchoolDefOf.Common, 
        manaCost: 0, 
        castTimeInSecs: 5, 
        [new(EffectDefOf.TownTeleport, null, 0)]);

    static public readonly PlanDef PlanCastSpell = new("CastSpell", typeof(BehaviorExecutePlanNew), InteractionDefOf.CastSpell);

    static SpellDefOf()
    {
        Def.Register(typeof(SpellDefOf));
    }
}