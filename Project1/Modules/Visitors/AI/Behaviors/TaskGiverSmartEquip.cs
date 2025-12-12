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
            foreach (var gt in actor.GetGearTypes())
            {
                foreach(var candidate in manager.GetItemsBySituationalScore(actor, i => i.Def.GearType == gt))
                {
                    var equipped = actor.GetEquipmentSlot(gt);
                    if (equipped == candidate.item)
                        break;
                    if (!actor.Inventory.Contains(candidate.item))
                        continue;
                    // do more checks here if necessary
                    if(candidate.score>best.score)
                        best = (gt, candidate.item, candidate.score);
                }
            }
            if(best.item != null)
                //return new AITask(typeof(BehaviorEquipItemNew), new TargetArgs(best.item));
                return new AITask(TaskDefOf.Equip, new TargetArgs(best.item));
            return null;
        }
    }
}
