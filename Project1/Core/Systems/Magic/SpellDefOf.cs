using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Interactions;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
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
        castTimeInSecs: 5
        )
    {
        //Instant = new EffectConfig_ResourceDelta(ResourceDefOf.Health, 10),
        EffectsNew = [(new EffectConfig_ResourceDelta { Resource = ResourceDefOf.Health, Delta = -10 }, EffectSchedule.Instant)]
        //{
        //    Resource = ResourceDefOf.Health,
        //    Delta = 10
        //}
    };

    public static readonly SpellDef Healing = new("Healing",
        TargetType.Entity,
        subject: SpellSubject.Any,
        category: SpellCategory.Defensive,
        school: SpellSchoolDefOf.Holy,
        manaCost: 20,
        castTimeInSecs: 5)
    {
        //Instant = new EffectConfig_ResourceDelta(ResourceDefOf.Health, 50),
        EffectsNew = [(new EffectConfig_ResourceDelta { Resource = ResourceDefOf.Health, Delta = 50 }, EffectSchedule.Instant)]

        //{
        //    Resource = ResourceDefOf.Health,
        //    Delta = 50
        //}
    };

    public static readonly SpellDef Mending = new("Mending",
        TargetType.Entity,
        subject: SpellSubject.Any,
        category: SpellCategory.Defensive,
        school: SpellSchoolDefOf.Nature,
        manaCost: 20,
        castTimeInSecs: 2)
    {
        //Periodic = [new EffectConfig_ResourceDelta(ResourceDefOf.Health, 5)],
        EffectsNew = [(new EffectConfig_ResourceDelta { Resource = ResourceDefOf.Health, Delta = 50 }, EffectSchedule.Timed(Ticks.FromMinutes(5), 5))]

        //{
        //    Resource = ResourceDefOf.Health,
        //    Delta = 5
        //}
        
    };

    public static readonly SpellDef HealingPotion = new("HealingPotion",
            TargetType.Entity,
            subject: SpellSubject.Any,
            category: SpellCategory.Defensive,
            school: SpellSchoolDefOf.Holy,
            manaCost: 20,
            castTimeInSecs: 5)
    {
        //Instant = new EffectConfig_ResourceDelta(ResourceDefOf.Health, 10)
        //{
        //    Resource = ResourceDefOf.Health,
        //    Delta = 10
        //}
        //,
        //Periodic = [new EffectConfig_ConsumableCooldown(ConsumableDefOf.Potion),

        //],
        EffectsNew = [
            (new EffectConfig_ResourceDelta { Resource = ResourceDefOf.Health, Delta = 10 }, EffectSchedule.Instant),
            (new EffectConfig_ConsumableCooldown { Family = ConsumableDefOf.Potion }, EffectSchedule.Timed(Ticks.FromMinutes(30), 1))
        ]
    };

    public static readonly SpellDef MaxHealthBuff = new("MaxHealthBuff",
        TargetType.Entity,
        subject: SpellSubject.Any,
        category: SpellCategory.Defensive,
        SpellSchoolDefOf.Holy,
        manaCost: 10,
        castTimeInSecs: 1)
    {
        //Instant = new EffectConfig_ResourceFortify(ResourceDefOf.Health, 10),
        EffectsNew = [(new EffectConfig_ResourceFortify { Resource = ResourceDefOf.Health, Delta = 10 }, EffectSchedule.Timed(Ticks.FromHours(1), 1))]
    };

    public static readonly SpellDef Teleporting = new("Teleporting",
        TargetType.Entity,
        subject: SpellSubject.Self,
        category: SpellCategory.Neutral,
        SpellSchoolDefOf.Common,
        manaCost: 0,
        castTimeInSecs: 5)
    {
        //Instant = new EffectConfig_TownTeleport()
        EffectsNew = [(new EffectConfig_TownTeleport(), EffectSchedule.Instant)]
    };

    static public readonly PlanDef PlanCastSpell = new("CastSpell", typeof(BehaviorExecutePlanNew), InteractionDefOf.CastSpell);

    static SpellDefOf() => Def.Register(typeof(SpellDefOf));
}