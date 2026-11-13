using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using System;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Systems;

static internal class SmartEquipSystem
{
    static internal EquipStatusDecision EvaluateEquipmentChange(Actor actor)
    {
        var manager = actor.ItemPreferences;
        var world = actor.World;
        Dictionary<GearSlotDef, (GearSlotDef slot, Entity? currentItem, Entity? newItem, int score)> mostImpactfulPerSlot = [];
        Dictionary<GearSlotDef, (GearSlotDef slot, Entity? item, int score)> currentPerSlot = [];
        foreach (var gt in actor.GetGearTypes())
        {
            (GearSlotDef slot, Entity? item, int score) mostImpactfulInSlot = (gt, null, 0);

            var current = actor.GetEquipmentSlot(gt);
            var currentItemScore = current is not null ? manager.GetTotalSituationalScoreFor(current) : 0;
            //var candidates = manager.GetItemsBySituationalScore(actor, i => i.Def.GearType == gt);
            var candidates = manager.GetItemsBySituationalScoreNewNew(actor, i => i.Def.GearType == gt);
            currentPerSlot[gt] = (gt, current, currentItemScore);
            if (currentItemScore > 0)
                mostImpactfulInSlot = (slot: gt, item: current, score: currentItemScore);

            foreach (var (itemId, score) in candidates)
            {
                var item = world.Get(itemId);
                if (score > 0)
                {
                    if (!actor.Inventory.Contains(item))
                        continue;
                    //do more checks here if necessary
                    if (score > mostImpactfulInSlot.score)
                        mostImpactfulInSlot = (gt, item, score);
                }
                if (score < 0)
                {
                    if (!actor.Inventory.Contains(item))
                        continue;
                    //do more checks here if necessary
                    if (score < mostImpactfulInSlot.score)
                        mostImpactfulInSlot = (gt, item, score);
                }
            }
            mostImpactfulPerSlot[gt] = new(gt, current, mostImpactfulInSlot.item, mostImpactfulInSlot.score);
        }

        (Entity? item, int delta) mostImpactful = (null, 0);
        foreach (var (slot, currentItem, newItem, newScore) in mostImpactfulPerSlot.Values)
        {
            var current = currentPerSlot[slot];
            if (newScore > 0)
            {
                var delta = newScore - current.score;
                if (delta > 0 && delta > mostImpactful.delta)
                    mostImpactful = (newItem, delta);
                continue;
            }
            if (current.score < 0)
            {
                var harm = Math.Abs(current.score);
                if (harm > mostImpactful.delta)
                    mostImpactful = (current.item, current.score);
            }
        }

        return new(mostImpactful.item?.RefId ?? EntityRefId.Null, mostImpactful.delta);
    }
}
