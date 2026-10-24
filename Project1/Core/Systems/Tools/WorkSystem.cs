using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Entities.Stats.Resolvers;
using Project1.Core.Gear;
using Project1.Core.Stats;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Tools;

[EnsureStaticCtorCall]
public static class WorkSystem
{
    static readonly Dictionary<StatDef, ToolUseDef> StatToTooluse = [];
    static readonly Dictionary<ToolUseDef, ToolUseUnarmedDef> ToolToUnarmed = [];
    static WorkSystem()
    {
        foreach (var def in StatSystem.AllStats.Where(s => s.Worker is StatResolver_ActorWorkEffectiveness))
            StatToTooluse[def] = def.TargetDef as ToolUseDef;

        foreach (var def in Def.Get<ToolUseUnarmedDef>())
            ToolToUnarmed[def.Source] = def;
    }
    static public bool TryGetStatToToolUse(StatDef def, out ToolUseDef capability)
        => StatToTooluse.TryGetValue(def, out capability);
    static public Dictionary<ToolUseDef, float> GetFinalWorkAmounts(Actor actor)
    {
        Dictionary<ToolUseDef, float> dic = [];
        var mainHand = actor.Gear.GetGear(GearSlotDefOf.Mainhand);
        if (mainHand is null)
        {
            var workTypes = Def.Get<ToolUseDef>();
            foreach (var work in workTypes)
            {
                var unarmedDef = StatSystem.GetDerivedStat(work);
                var unarmedStat = actor.Stats.Get(unarmedDef);
                dic.Add(work, unarmedStat);
            }
        }
        else
        {
            var profile = mainHand.Profile as GearProfileDef;
            var power = mainHand.ToolComponent.IntrinsicStats[StatDefOf.ItemPower].Value;
            foreach(var (capability, mod) in profile.ToolUsesModifiers)
            {
                var final = power * mod;
                dic.Add(capability, final);
            }
        }
        return dic;
    }
    static public float GetFinalWorkAmount(Actor actor, ToolUseDef capability)
    {
        if (capability is null)
            return 1f;
        var mainHand = actor.Gear.GetGear(GearSlotDefOf.Mainhand);
        if (mainHand is null)
        {
            var unarmedDef = ToolToUnarmed[capability];
            var unarmedStat = StatSystem.GetDerivedStat(unarmedDef);
            var unarmedStatValue = actor.Stats.Get(unarmedStat);
            return unarmedStatValue;
        }
        var profile = mainHand.Profile as GearProfileDef;
        var power = mainHand.ToolComponent.IntrinsicStats[StatDefOf.ItemPower].Value;
        var itemMod = profile.ToolUsesModifiers.GetValueOrDefault(capability);
        var actorMod = actor.Stats.Get(StatSystem.GetDerivedStat(capability));
        var final = power * itemMod * actorMod;
        final = Math.Max(final, 1);
        return final;
    }
}
