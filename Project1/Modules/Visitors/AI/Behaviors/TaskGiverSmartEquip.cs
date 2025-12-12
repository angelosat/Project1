using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.AI.Behaviors
{
    internal class TaskGiverSmartEquip : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var manager = actor.ItemPreferences;
            Dictionary<GearType, (GearType slot, Entity currentItem, Entity newItem, int score)> bestPerSlot = [];
            Dictionary<GearType, (GearType slot, Entity item, int score)> currentPerSlot = [];
            foreach (var gt in actor.GetGearTypes())
            {
                (GearType slot, Entity item, int score) bestInSlot = (null, null, 0);

                var current = actor.GetEquipmentSlot(gt);
                var currentItemScore = current is not null ? manager.GetTotalSituationalScoreFor(current) : 0;
                var candidates = manager.GetItemsBySituationalScore(actor, i => i.Def.GearType == gt);
                currentPerSlot[gt] = (gt, current, currentItemScore);
                if (currentItemScore > 0)
                    bestInSlot = (slot: gt, item: current, score: currentItemScore);

                foreach (var candidate in candidates)
                {
                    if (candidate.score > 0)
                    {
                        if (!actor.Inventory.Contains(candidate.item))
                            continue;
                        //do more checks here if necessary
                        if (candidate.score > bestInSlot.score)
                            bestInSlot = (gt, candidate.item, candidate.score);
                    }
                }
                bestPerSlot[gt] = new(gt, current, bestInSlot.item, bestInSlot.score);
            }

            (Entity item, int score) mostImpactful = (null, 0);

            foreach (var (slot, currentItem, newItem, newScore) in bestPerSlot.Values)
            {
                var current = currentPerSlot[slot];
                if (newScore > 0)
                {
                    if (newScore > current.score && newScore > mostImpactful.score)
                        mostImpactful = (newItem, newScore);
                    continue;
                }
                if (current.score < 0)
                {
                    var harm = Math.Abs(current.score);
                    if (harm > mostImpactful.score)
                        mostImpactful = (current.item, current.score);
                }
            }

            if (mostImpactful.item != null)
            {
                if (mostImpactful.score > 0)
                    return new AITask(TaskDefOf.Equip, new TargetArgs(mostImpactful.item));
                else if (mostImpactful.score < 0)
                    return new AITask(TaskDefOf.Unequip, new TargetArgs(mostImpactful.item));
            }

            return null;
        }
    }
}
