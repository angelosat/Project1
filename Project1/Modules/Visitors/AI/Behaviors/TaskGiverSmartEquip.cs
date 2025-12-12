using System;
using System.Collections.Generic;
using System.Text;

namespace Start_a_Town_.AI.Behaviors
{
    internal class TaskGiverSmartEquip : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var manager = actor.ItemPreferences;
            (GearType slot, Entity item, int score) best = (null, null, 0);
            (GearType slot, Entity item, int score) worst = (null, null, 0);
            foreach (var gt in actor.GetGearTypes())
            {
                foreach (var candidate in manager.GetItemsBySituationalScore(actor, i => i.Def.GearType == gt))
                {
                    var equipped = actor.GetEquipmentSlot(gt);
                    if (equipped == candidate.item)
                        break;
                    if (!actor.Inventory.Contains(candidate.item))
                        continue;
                    //do more checks here if necessary
                    if (candidate.score > best.score)
                            best = (gt, candidate.item, candidate.score);
                    // TODO handle unequip
                    //if (candidate.score > 0)
                    //{
                    //    var equipped = actor.GetEquipmentSlot(gt);
                    //    if (equipped == candidate.item)
                    //        break;
                    //    if (!actor.Inventory.Contains(candidate.item))
                    //        continue;
                    //    if (candidate.score > best.score)
                    //        best = (gt, candidate.item, candidate.score);
                    //}
                    //else
                    //{
                    //    if (candidate.score < worst.score)
                    //        worst = (gt, candidate.item, candidate.score);
                    //}
                }
            }
            //if(best.item != null)
            //    return new AITask(TaskDefOf.Equip, new TargetArgs(best.item));


            if (best.item != null)
                return new AITask(TaskDefOf.Equip, new TargetArgs(best.item));

            return null;
        }
    }
}
