using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverEquipSelf : TaskGiver
    {
        static AITask TryDropUnnecessaryItems(Actor actor)
        {
            if (actor.Inventory.All.FirstOrDefault(i => !actor.ItemPreferences.IsPreference(i)) is Entity item)
                //return new AITask(typeof(TaskBehaviorDropInventoryItem), item);
                return new AITask(TaskDefOf.DropInventory, item);// { TargetA = item };
            return null;
        }

        protected override AITask TryAssignTask(Actor actor)
        {
            // TODO associate labors with tool, if labor is enabled, look for and store tools in inventory. if labor is disabled, remove unnecessary tools from inventory
            // TODO flag jobs for which a tool is already acquired so as to not recheck everything all the time
            if (!actor.IsTownMember)
                return null; // TODO instead of doing this, check if the tool is claimable
            //if (TryDropUnnecessaryItems(actor) is AITask task)
            //    return task;
            var map = actor.Map;
            var jobs = actor.GetJobs();
            var manager = actor.ItemPreferences;

            //foreach (var job in jobs)
            //{
            //    var context = job.Def;
            //    var preferredTool = manager.GetPreference(context, out var existingScore);
            //    if (preferredTool is not null)
            //    {
            //        if (!actor.Inventory.Contains(preferredTool))
            //        {
            //            // if it's not inside inventory, instead of going to pick it up...
            //            // ...remove preference and check for a tool next tick (it might be a leftover from a previous failed behavior, or the item might no longer be available)
            //            /// wait.. WHY NOT go pick it up?
            //            manager.RemovePreference(context);
            //        }
            //        else
            //        {
            //            if (!job.Enabled)
            //            {
            //                manager.RemovePreference(context);
            //                return new AITask(TaskDefOf.DropCarried) { TargetA = preferredTool };
            //            }
            //        }
            //    }
            //}

            var potentialAll = manager.GetPotential();
            foreach (var (role, item, score) in potentialAll)
            {
                if (!actor.CanReserve(item as Entity))
                    continue;
                if (!actor.CanReach(item))
                    continue;

                manager.Commit(role, item, score);
                return new AITask(TaskDefOf.PickUp) { TargetA = item, AmountA = 1 };
            }
            return null;
        }

        public override TaskDef CanGiveTask(Actor actor, TargetArgs target)
        {
            if (target.Object is not Entity item)
                return null;
            var itemmanager = actor.ItemPreferences;
            var (role, _) = itemmanager.FindBestRole(item);
            if (role is not null)
                return TaskDefOf.PickUp;
            return null;
        }

        public override AITask TryTaskOn(Actor actor, TargetArgs target, bool ignoreOtherReservations = false)
        {
            if (target.Object is not Entity item)
                return null;
            var itemmanager = actor.ItemPreferences;
            var (role, score) = itemmanager.FindBestRole(item);
            if (role is null)
                return null;
            itemmanager.Commit(role, item, score);
            return new AITask(typeof(TaskBehaviorStoreInInventory)) { TargetA = target, AmountA = 1 };
        }
    }
}
