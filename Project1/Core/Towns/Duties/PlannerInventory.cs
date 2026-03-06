using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Reservations;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Duties;
using System.Linq;

namespace Project1.Core.Towns.Labors
{
    class PlannerInventory : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            // TODO associate labors with tool, if labor is enabled, look for and store tools in inventory.
            // if labor is disabled, remove unnecessary tools from inventory
            // TODO flag jobs for which a tool is already acquired so as to not recheck everything all the time
            if (!actor.IsTownMember)
                return null; // TODO instead of doing this, check if the tool is claimable

            var map = actor.Map;
            var jobs = actor.ActiveDuties;
            var manager = actor.ItemPreferences;

            // if carrying an item that's set as an itempreference, then store it in inventory
            if (actor.Hauled is Entity carried)
            {
                if (manager.IsUseful(carried))
                    return new Plan(PlanDefOf.StoreInInventory) { Continuation = PlanContinuationPolicy.Yield };
                else // else fallback to next planner
                    return null;
            }

            // Query manager for potential map items to go carry/pick up
            var potentialAll = manager.GetPotential();
            foreach (var (role, item, score) in potentialAll)
            {
                if (!actor.CanReserve(item as Entity))
                    continue;
                if (!actor.CanReach(item))
                    continue;

                manager.Commit(role, item, score);
                return new Plan(PlanDefOf.GoHaul) { TargetA = item };
            }

            // take out from inventory items that are not an item preference
            if (actor.Inventory.All.FirstOrDefault(i => !actor.ItemPreferences.IsUseful(i)) is Entity junk)
                return new Plan(PlanDefOf.RetrieveFromInventory, junk);

            return null;
        }

        public override PlanDef CanGiveTask(Actor actor, TargetArgs target)
        {
            if (target.Object is not Entity item)
                return null;
            var itemmanager = actor.ItemPreferences;
            var (role, _) = itemmanager.FindBestRole(item);
            if (role is not null)
                return PlanDefOf.PickUp;
            return null;
        }

        //public override Plan TryTaskOn(Actor actor, TargetArgs target, bool ignoreOtherReservations = false)
        //{
        //    if (target.Object is not Entity item)
        //        return null;
        //    var itemmanager = actor.ItemPreferences;
        //    var (role, score) = itemmanager.FindBestRole(item);
        //    if (role is null)
        //        return null;
        //    itemmanager.Commit(role, item, score);
        //    return new Plan(typeof(TaskBehaviorStoreInInventory)) { TargetA = target, AmountA = 1 };
        //}
    }
}
