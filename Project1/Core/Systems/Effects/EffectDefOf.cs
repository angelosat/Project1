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
        );

    static public EffectDef ResourceFortify = new("FortifyResource",
                                                  "Fortify",
                                                  typeof(Effect_ResourceFortify),
                                                  typeof(ResourceDef),
                                                  typeof(EffectData_ResourceFortify), 
                                                  typeof(EffectConfig_ResourceFortify)
                                                  );

    static public EffectDef ResourceDelta = new("RestoreResource",
                                                  "Restore",
                                                  typeof(Effect_ResourceDelta),
                                                  typeof(ResourceDef),
                                                  typeof(EffectData_ResourceDelta), 
                                                  typeof(EffectConfig_ResourceDelta)
        );

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
