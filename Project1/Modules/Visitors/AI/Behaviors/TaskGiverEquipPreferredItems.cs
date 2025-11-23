using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskGiverEquipPreferredItems : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
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
                    return new AITask(typeof(BehaviorEquipItemNew), new TargetArgs(item));
                    // TODO check world incase the item is available in the map but not inside inveotry? return a pickup task in that case?
                    // TODO return equipping taskbehavior
                    // add previously equipped to todiscard?
                }
            }
            return null;
        }
    }
}
