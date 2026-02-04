using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using Start_a_Town_;
using Start_a_Town_.AI.Behaviors;

namespace Project1.Core.Towns.AI.Behaviors
{
    class TaskGiverEquipPreferredItems : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var prefs = actor.ItemPreferences;
            foreach(var gt in actor.GetGearTypes())
            {
                var item = prefs.GetPreference(gt);
                var equipped = actor.GetEquipmentSlot(gt);
                if (equipped == item)
                    continue;
                else
                {
                    return new Plan(typeof(BehaviorEquipItemNew), new TargetArgs(item));
                    // TODO check world incase the item is available in the map but not inside inveotry? return a pickup task in that case?
                    // TODO return equipping taskbehavior
                    // add previously equipped to todiscard?
                }
            }
            return null;
        }
    }
}
