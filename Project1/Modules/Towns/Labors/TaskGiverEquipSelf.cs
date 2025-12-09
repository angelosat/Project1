using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverEquipSelf : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            // TODO associate labors with tool, if labor is enabled, look for and store tools in inventory. if labor is disabled, remove unnecessary tools from inventory
            // TODO flag jobs for which a tool is already acquired so as to not recheck everything all the time
            if (!actor.IsTownMember)
                return null; // TODO instead of doing this, check if the tool is claimable
            if (TryDropUnnecessaryItems(actor) is AITask task)
                return task;
            var map = actor.Map;
            var jobs = actor.GetJobs();
            var manager = actor.ItemPreferences;

            foreach (var job in jobs)
            {
                var context = job.Def;
                var preferredTool = manager.GetPreference(context, out var existingScore);
                if (preferredTool is not null)
                {
                    if (!actor.Inventory.Contains(preferredTool))
                    {
                        // if it's not inside inventory, instead of going to pick it up...
                        // ...remove preference and check for a tool next tick (it might be a leftover from a previous failed behavior, or the item might no longer be available)
                        manager.RemovePreference(context);
                    }
                    else
                    {
                        if (!job.Enabled)
                        {
                            manager.RemovePreference(context);
                            //return new AITask(typeof(TaskBehaviorDropItem), preferredTool);
                            return new AITask(TaskDefOf.DropCarried) { TargetA = preferredTool };
                        }
                    }
                }
            }

            //var allitems = map.GetEntities().OfType<Entity>();
            var allitems = map.GetEntities<Tool>();

            var allPrefs = manager.EvaluateAll(allitems);
            foreach(var pref in allPrefs)
            {
                var item = pref.item;
                if (!actor.CanReserve(item as Entity))
                    continue;
                if (!actor.CanReach(item))
                    continue;

                manager.AddPreference(pref.role, item, pref.score);
                return new AITask(TaskDefOf.PickUp) { TargetA = item, AmountA = 1 };
            }
            return null;
            foreach (var item in allitems)
            {
                var roles = manager.FindAllRoles(item);
                if (!roles.Any())
                    continue;
                var finalRoles = roles
                    .Where(r => jobs.Any(j => j.Enabled && j.Def == r.role))
                    .Where(r=> manager.GetPreference(r.role, out var existingScore) is var existing && r.score > existingScore);
                if (!finalRoles.Any())
                    continue;

                /// TODO: find the best item , dont just pickup the first item. because then the actor will go and and pick up another item
                /// right now the actor's itempreferencemanager, takes an item and scores it for that actor based on its role. 
                /// if all checks pass, the itempreference is stored in the manager with the score that was awarded (manager.AddPreference())
                /// however if next time the taskgiver runs, the next item has a higher score, then the same thing will happen for that item.
                /// i must pool scores for all items and go for the highest one
                /// but also somehow prevent scoring all items again every frame . the actor shouldn't scan items every frame to replace his tools with better one. 
                /// maybe its better to hook to a entityspawnevent and check that if the new spawn item could replace a tool, then make a task
                /// the manager should hook to the map's onentityspawnevent, and whenever it fires, store tha entity in a notscannedyet queue. 
                /// then this taskgiver will pull all items from the notscannedyet pool and find the best item to go pick up
                /// but i must also somehow handle the situtation where while the actor is in the middle of going and picking up a preffered item, that a new item gets spawned that's even better
                /// then the taskgiver must somehow still run and compare any new items to the one currently going for, and cancel the current behavior
                /// 
                /// let's tackle one thing at a time. pooling all item scores. at first step this will pull items form world.getentities, and then it will pull from preferecemanager.getunscanneditems

                if (!actor.CanReserve(item as Entity))
                    continue;
                if (!actor.CanReach(item))
                    continue;
                foreach (var role in finalRoles)
                    manager.AddPreference(role.role, item, role.score);
                return new AITask(TaskDefOf.PickUp) { TargetA = item, AmountA = 1 };
            }

            return null;
        }
        static AITask TryDropUnnecessaryItems(Actor actor)
        {
            if (actor.Inventory.All.FirstOrDefault(i => !actor.ItemPreferences.IsPreference(i)) is Entity item)
                //return new AITask(typeof(TaskBehaviorDropInventoryItem), item);
                return new AITask(TaskDefOf.DropInventory) { TargetA = item };
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
            itemmanager.AddPreference(role, item, score);
            return new AITask(typeof(TaskBehaviorStoreInInventory)) { TargetA = target, AmountA = 1 };
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
        //protected override AITask TryAssignTask(Actor actor)
        //{
        //    // TODO associate labors with tool, if labor is enabled, look for and store tools in inventory. if labor is disabled, remove unnecessary tools from inventory
        //    // TODO flag jobs for which a tool is already aquired so as to not recheck everything all the time
        //    if (!actor.IsCitizen)
        //        return null; // TODO instead of doing this, check if the tool is claimable
        //    if (DropUnnecessaryItems(actor) is AITask task)
        //        return task;
        //    var jobs = actor.GetJobs();
        //    foreach (var job in jobs)
        //    {
        //        var itemmanager = actor.ItemPreferences;
        //        var toolUse = job.Def.ToolUse;
        //        if (toolUse is null)
        //            continue;
        //        var preferredTool = itemmanager.GetPreference(toolUse, out var existingScore);

        //        // see if there are better tools lying around
        //        if (job.Enabled)
        //        {
        //            var potentialTools = actor.Map.Find(i => i.ToolComponent?.Props?.ToolUse == toolUse);
        //            var scoredTools = potentialTools.Select(i => new { item = i, score = itemmanager.GetScore(toolUse, i) }).OrderByDescending(i => i.score);
        //            foreach (var tool in scoredTools)
        //            {
        //                if (tool.score <= existingScore)
        //                    break;
        //                if (!actor.CanReserve(tool.item))
        //                    continue;
        //                if (!actor.CanReach(tool.item))
        //                    continue;
        //                itemmanager.AddPreference(toolUse, tool.item, tool.score);
        //                return new AITask(TaskDefOf.PickUp) { TargetA = tool.item, AmountA = 1 };
        //            }
        //        }

        //        // otherwise, if there are no better tools to go pick up, do actions with the existing preferred tool
        //        if (preferredTool is not null)
        //        {
        //            if (!actor.Inventory.Contains(preferredTool))
        //            {
        //                // if it's not inside inventory, instead of going to pick it up...
        //                // ...remove preference and check for a tool next tick (it might be a leftover from a previous failed behavior, or the item might no longer be available)
        //                itemmanager.RemovePreference(toolUse);
        //            }
        //            else
        //            {
        //                if (!job.Enabled)
        //                {
        //                    itemmanager.RemovePreference(toolUse);
        //                    return new AITask(typeof(TaskBehaviorDropItem), preferredTool);
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}

    }
}
