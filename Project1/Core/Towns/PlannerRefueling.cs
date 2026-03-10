using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Blocks.Comps;
using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns
{
    class PlannerRefueling : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var map = actor.Map;
            var refuelables = GetRefuelables(map);

            var target = refuelables.FirstOrDefault(c =>
                c.GetPercentage(ResourceDefOf.Fuel) < .5f &&
                actor.CanReachAndReserve(c.Parent.OriginGlobal));

            if (target is null)
                return null;

            var deficit = target.GetDeficit(ResourceDefOf.Fuel);
            var itemsInStockpiles = map.Stockpiles.AllItems.Where(CraftingSystem.IsFuel);

            if (actor.Hauled is Entity carried)
            {
                var carriedFuelValue = CraftingSystem.GetFuelValue(carried);

                if (deficit > carriedFuelValue)
                {
                    foreach (var nextItem in itemsInStockpiles.Where(i => actor.CanReachAndReserve(i) && carried.CanAbsorb(i)))
                    {
                        var nextItemFuel = CraftingSystem.GetFuelValue(nextItem);
                        if (nextItemFuel == 0)
                            continue;
                        var amountToPickUp = Math.Min(carried.StackMax - carried.StackSize, (int)(deficit / nextItemFuel));
                        return new Plan(PlanDefOf.GoHaul, nextItem) { AmountA = amountToPickUp };
                    }
                }
                return new Plan(PlanDefOf.Deposit, new TargetArgs(map, target.Parent.OriginGlobal)) { TargetB = new TargetArgs(target.Parent) };
            }
            foreach (var i in itemsInStockpiles)
            {
                if (!actor.CanReachAndReserve(i))
                    continue;
                var iFuelValue = CraftingSystem.GetFuelValue(i);
                var amountToPickUp = Math.Min(i.StackSize, (int)(deficit / iFuelValue));
                return new Plan(PlanDefOf.GoHaul, i) { AmountA = amountToPickUp };
            }

            return null;
        }
        //protected override Plan TryPlan(Actor actor)
        //{
        //    var map = actor.Map;
        //    var refuelables = GetRefuelables(map);

        //    var target = refuelables.FirstOrDefault(c => 
        //        c.Fuel.Percentage < .5f && 
        //        actor.CanReachAndReserve(c.Parent.OriginGlobal));

        //    if (target is null)
        //        return null;

        //    if (actor.Hauled is Entity carried)
        //        return new Plan(PlanDefOf.GoPlace, new TargetArgs(map, target.Parent.OriginGlobal)) { TargetB = new TargetArgs(target.Parent) };
        //    var items = map.Stockpiles.AllItems.Where(CraftingSystem.IsFuel);
        //    foreach (var i in items)
        //    {
        //        if (!actor.CanReachAndReserve(i))
        //            continue;
        //        return new Plan(PlanDefOf.GoHaul, i);
        //    }

        //    return null;
        //}
        //static IEnumerable<BlockFuelComp> GetRefuelables(MapBase map) => map.BlockEntities.Where(e => e.HasComp<BlockFuelComp>()).Select(e => e.GetComp<BlockFuelComp>());
        static IEnumerable<BlockResourcesComp> GetRefuelables(MapBase map)
            => map.BlockEntities
                .Where(e => e.HasComp<BlockResourcesComp>())
                .Select(e => e.GetComp<BlockResourcesComp>())
                .Where(c => c.HasResource(ResourceDefOf.Fuel));
          
    }
}
