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
                                             typeof(Effect_ModifyNeed),
                                             typeof(NeedDef),
                                             typeof(EffectData_ModifyNeed),
                                             typeof(EffectConfig_ModifyNeed)
        //,
        //baseDuration: null,
        //baseMagnitude: 10
        );

    static public EffectDef ResourceFortify = new("FortifyResource",
                                                  "Fortify",
                                                  typeof(Effect_ResourceFortify),
                                                  typeof(ResourceDef),
                                                  typeof(EffectData_ResourceFortify), 
                                                  typeof(EffectConfig_ResourceFortify)
        //,
                                                  //                                          baseDuration: Ticks.FromDays(1),
                                                  //                                          baseMagnitude: 50
                                                  );
    //{ 
    //School = SpellSchoolDefOf.Holy 
    //};

    static public EffectDef ResourceDelta = new("RestoreResource",
                                                  "Restore",
                                                  typeof(Effect_ResourceDelta),
                                                  typeof(ResourceDef),
                                                  typeof(EffectData_ResourceDelta), 
                                                  typeof(EffectConfig_ResourceDelta)
        //,
        //                                          baseDuration: 0,
        //                                          baseMagnitude: 50
        );
    //{ 
    //    School = SpellSchoolDefOf.Holy,
    //};

    //static public EffectDef SpellDamage = new("DamageResource",
    //                                          "Damage",
    //                                          typeof(Effect_DamageResource),
    //                                          typeof(ResourceDef)
    //    //,
    //                                          //baseDuration: 0,
    //                                          //baseMagnitude: 50
    //    );

    //static public EffectDef Burning = new("Burning",
    //                                      "Burn",
    //                                      typeof Effect_DoT(),
    //                                      null
    //    //,
    //    //                                  context: SpellSchoolDefOf.Fire,
    //    //                                  baseDuration: null,
    //    //                                  tickPeriod: (int)Ticks.FromSeconds(.5f),
    //    //                                  tickCount: 10,
    //    //                                  baseMagnitude: 2
    //    );

    //static public EffectDef Mending = new("Mending",
    //                                  "Mending",
    //                                  new Effect_HoT(),
    //                                  null
    //    //,
    //    //                              context: SpellSchoolDefOf.Nature,
    //    //                              baseDuration: null,
    //    //                              tickPeriod: (int)Ticks.FromSeconds(.5f),
    //    //                              tickCount: 10,
    //    //                              baseMagnitude: 2
    //    );

    static public EffectDef TownTeleport = new("TownPortal",
                                               "Restore",
                                               typeof(Effect_TownTeleport),
                                               null,
                                               typeof(EffectData_TownTeleport),
                                               typeof(EffectConfig_TownTeleport));

    static public EffectDef ConsumableCooldown = new("PotionCooldown",
                                                     "Cooldown",
                                                     typeof(Effect_ConsumableCooldown),
                                                     null,
                                                     typeof(EffectData_ConsumableCooldown),
                                                     typeof(EffectConfig_ConsumableCooldown));

    static public EffectDef Fled = new("Fled",
                                       "Flee",
                                       typeof(Effect_Flee),
                                       null,
                                       typeof(EffectData_Flee),
                                       typeof(EffectConfig_Flee))
    {
        StatMods = new() { { StatDefOf.AvoidCombatEncounter, 100f } }
    };

    static EffectDefOf()
    {
        Def.Register(typeof(EffectDefOf));
    }
}
