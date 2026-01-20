using System;
using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    internal class SmartEquipPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
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

                foreach (var (item, score) in candidates)
                {
                    if (score > 0)
                    {
                        if (!actor.Inventory.Contains(item))
                            continue;
                        //do more checks here if necessary
                        if (score > bestInSlot.score)
                            bestInSlot = (gt, item, score);
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
                    return new Plan(PlanDefOf.Equip, new TargetArgs(mostImpactful.item));
                else if (mostImpactful.score < 0)
                    return new Plan(PlanDefOf.Unequip, new TargetArgs(mostImpactful.item));
            }

            // evaluate if there's an item to be moved from inventory to the haul/carry slot
            (Entity item, int score) best = default; 
            foreach(var item in actor.Inventory.Contents)
            {
                var score = manager.GetTotalSituationalScoreFor(item);
                if (score <= 0)
                    continue;
                if (score < best.score)
                    continue;
                best.item = item;
                best.score = score;
            }
            if (best.item is not null)
                return new Plan(PlanDefOf.RetrieveFromInventory, best.item);

            return null;
        }
    }
}
