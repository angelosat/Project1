using Project1.Core.Needs;
using Project1.Core.Resources;
using Project1.Core.Stats;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Magic;
using Project1.Framework;

namespace Project1.Core.Systems.Effects;

[EnsureStaticCtorCall]
static class EffectDefOf
{
    static public EffectDef ModifyNeed = new("ModifyNeed",
                                             "Modify",
                                             new Effect_ModifyNeed(),
                                             typeof(NeedDef),
                                             baseDuration: null,
                                             baseMagnitude: 10);

    static public EffectDef FortifyResource = new("FortifyResource",
                                                  "Fortify",
                                                  new Effect_FortifyResource(),
                                                  typeof(ResourceDef),
                                                  baseDuration: Ticks.FromDays(1),
                                                  baseMagnitude: 50) 
    { 
        School = SpellSchoolDefOf.Holy 
    };
    
    static public EffectDef RestoreResource = new("RestoreResource",
                                                  "Restore",
                                                  new Effect_RestoreResource(),
                                                  typeof(ResourceDef),
                                                  baseDuration: 0,
                                                  baseMagnitude: 50) 
    { 
        School = SpellSchoolDefOf.Holy,
    };

    static public EffectDef SpellDamage = new("DamageResource",
                                              "Damage",
                                              new Effect_DamageResource(),
                                              typeof(ResourceDef),
                                              baseDuration: 0,
                                              baseMagnitude: 50);

    static public EffectDef Burning = new("Burning",
                                          "Burn",
                                          new Effect_DoT(),
                                          null,
                                          context: SpellSchoolDefOf.Fire,
                                          baseDuration: null,
                                          tickPeriod: (int)Ticks.FromSeconds(.5f),
                                          tickCount: 10,
                                          baseMagnitude: 2);

    static public EffectDef TownTeleport = new("TownPortal", "Restore", new Effect_TownTeleport(), null, baseDuration: 0);

    static public EffectDef ConsumableCooldown = new("PotionCooldown", "Cooldown", new Effect_ConsumableCooldown(), null, baseDuration: Ticks.FromMinutes(30));

    static public EffectDef Fled = new("Fled", "Flee", new Effect_Flee(), null, baseDuration: Ticks.FromHours(1))
    {
        StatMods = new() { { StatDefOf.AvoidCombatEncounter, 100f } }
    };

    static EffectDefOf()
    {
        Def.Register(typeof(EffectDefOf));
    }
}
