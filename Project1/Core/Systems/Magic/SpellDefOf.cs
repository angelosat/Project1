using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Interactions;
using Project1.Core.Resources;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Stats;
using Project1.Framework;

namespace Project1.Core.Systems.Magic;

[EnsureStaticCtorCall]
public static class SpellDefOf
{
    public static readonly SpellDef Fireball = new("Fireball",
        TargetType.Entity,
        subject: SpellSubject.Other,
        category: SpellCategory.Offensive,
        school: SpellSchoolDefOf.Fire,
        manaCost: 20,
        castTimeInSecs: 5,
        [new(EffectDefOf.SpellDamage, 0),
         new(EffectDefOf.Burning, 1)]);

    public static readonly SpellDef Healing = new("Healing", 
        TargetType.Entity, 
        subject: SpellSubject.Any,
        category: SpellCategory.Defensive,
        school: SpellSchoolDefOf.Holy, 
        manaCost: 20, 
        castTimeInSecs: 5, 
        [new(EffectDefOf.RestoreResource, ResourceDefOf.Health, 50)]);

    public static readonly SpellDef Mending = new("Mending",
        TargetType.Entity,
        subject: SpellSubject.Any,
        category: SpellCategory.Defensive,
        school: SpellSchoolDefOf.Nature,
        manaCost: 20,
        castTimeInSecs: 2,
        [new(EffectDefOf.Mending, ResourceDefOf.Health, 50)]);

    public static readonly SpellDef HealingPotion = new("HealingPotion",
        TargetType.Entity,
        subject: SpellSubject.Any,
        category: SpellCategory.Defensive,
        school: SpellSchoolDefOf.Holy,
        manaCost: 20,
        castTimeInSecs: 5,
        [new(EffectDefOf.RestoreResource, ResourceDefOf.Health, 10),
         new(EffectDefOf.ConsumableCooldown, null, 30)]);

    public static readonly SpellDef MaxHealthBuff = new("MaxHealthBuff", 
        TargetType.Entity,
        subject: SpellSubject.Any,
        category: SpellCategory.Defensive,
        SpellSchoolDefOf.Holy, 
        manaCost: 10, 
        castTimeInSecs: 1, 
        [new(EffectDefOf.FortifyResource, ResourceDefOf.Health, 10)]);

    public static readonly SpellDef Teleporting = new("Teleporting", 
        TargetType.Entity,
        subject: SpellSubject.Self,
        category: SpellCategory.Neutral,
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