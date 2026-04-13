using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System.Linq;

namespace Project1.Core.AI.Planners;

sealed class Planner_Inventory : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        // TODO associate labors with tool, if labor is enabled, look for and store tools in inventory.
        // if labor is disabled, remove unnecessary tools from inventory
        // TODO flag jobs for which a tool is already acquired so as to not recheck everything all the time
        //if (!actor.IsTownMember)
        //    return null; // TODO instead of doing this, check if the tool is claimable

        var map = actor.Map;
        //var jobs = actor.ActiveDuties;
        //var world = map.World;
        //var entitlements = world.Ownership.Get(actor);
        //foreach (var item in entitlements.Select(world.Get))
        //{
        //    if (!actor.CanReachAndReserve(item))
        //        continue;
        //    return new Plan(PlanDefOf.GoHaul, item);
        //}
        if (TryClaimEntitledItems(actor) is Plan claimPlan)
            return claimPlan;

        var manager = actor.ItemPreferences;

        // if carrying an item that's set as an itempreference, then store it in inventory
        if (actor.Hauled is Entity carried)
        {
            if (manager.IsUseful(carried))
                return new Plan(PlanDefOf.StoreInInventory) { Continuation = PlanContinuationPolicy.Yield };
            else // else fallback to next planner
                return null;
        }
        // let actors reach this point even if they are not a town member,
        // and only allow town members to evaluate and claim free map items 
        if (!actor.IsTownMember)
            return null; // TODO instead of doing this, check if the tool is claimable
        // Query manager for potential map items to go carry/pick up
        //manager.EvaluateOne();
        var potentialAll = manager.TryGetPotential();
        foreach (var (role, item, score) in potentialAll)
        {
            if (!actor.CanReachAndReserve(item))
                continue;

            manager.Commit(role, item, score);
            return new Plan(PlanDefOf.GoHaul) { TargetA = item };
        }

        // take out from inventory items that are not an item preference
        if (actor.Inventory.All.FirstOrDefault(i => !actor.ItemPreferences.IsUseful(i)) is Entity junk)
            return new Plan(PlanDefOf.RetrieveFromInventory, junk) { Continuation = PlanContinuationPolicy.Yield };

        return null;
    }

    public override PlanDef CanGiveTask(Actor actor, InteractionTarget target)
    {
        if (target.Object is not Entity item)
            return null;
        var itemmanager = actor.ItemPreferences;
        var (role, _) = itemmanager.FindBestRole(item);
        if (role is not null)
            return PlanDefOf.PickUp;
        return null;
    }

    static Plan TryClaimEntitledItems(Actor actor)
    {
        var world = actor.World;
        var entitlements = world.Ownership.Get(actor);
        if (actor.Hauled is Entity carried)
        {
            if (entitlements.Contains(carried.RefId))
                return new Plan(PlanDefOf.StoreInInventory);
            // if carrying an item that doesnt belong to the actor, return null or drop?
            // if this planner is a fallback planner, then drop.
            // otherwise must return null so that other planners might claim it
            //else
            //    return new Plan(PlanDefOf.GoPlace, actor.Map, actor.Cell);
            return null;
        }
        foreach (var item in entitlements.Select(world.Get))
        {
            if (!item.IsSpawned)
                continue;
            if (!actor.CanReachAndReserve(item))
                continue;
            return new Plan(PlanDefOf.GoHaul, item);
        }
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
