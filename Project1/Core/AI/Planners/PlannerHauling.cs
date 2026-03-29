using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Stockpiles;
using System;
using System.Linq;

namespace Project1.Core.AI.Planners
{
    internal class PlannerHauling : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var carried = actor.Hauled;
            var map = actor.Map;
            var stockpiles = map.Hauling.AllTargets.OrderByDescending(i => i.Priority);//  map.Town.ZoneManager.GetZones<Stockpile>().OrderByDescending(i => i.Priority);
            var mapItems = map.Haulables
                .Where(actor.CanReachAndReserve)
                .SortByReachableRegionDistance(actor)
                .Union(map.Hauling.InventoryItems)
                .ToList();

            // if actor is currently carrying something
            if (carried is not null)
            {
                // if stack = stackmax
                if (carried.IsStackFull)
                {
                    // iterate stockpiles by priority 
                    foreach (var stockpile in stockpiles)
                    {
                        ////until the first one that accepts it - intvec3? or targetargs place = stockpile.findplacefor(item)
                        //var place = stockpile.FindPlaceFor(carried);

                        //if (place is not null)
                        //    // emit godeliver task at place
                        //    return new Plan(PlanDefOf.HaulToStockpile, place);

                        //until the first one that accepts it - intvec3? or targetargs place = stockpile.findplacefor(item)
                        var places = stockpile.GetCandidateCells(carried).Where(actor.CanReachAndReserve);
                        foreach (var cell in places)
                            // emit godeliver task at place
                            return new Plan(PlanDefOf.HaulToStockpile, new TargetArgs(actor.Map, cell)) { Continuation = PlanContinuationPolicy.Yield };
                    }
                }
                // else if can carry more
                else
                {
                    // start iterating stockpiles until
                    foreach (var stockpile in stockpiles)
                    {
                        int availableCapacity = stockpile.GetAvailableSpaceFor(carried);
                        var diff = availableCapacity - carried.StackSize;
                        if (diff > 0)
                        {
                            // then iterate map items 
                            foreach (var item in mapItems)
                            {
                                var currentStockpile = stockpiles.FirstOrDefault(s => s.Contains(item));
                                if (currentStockpile != null && currentStockpile.Accepts(item))
                                    continue; // skip items already properly stored
                                //until a valid one is found and emit gohaul with target amount math.min(diff, target.stacksize)
                                if (carried.CanAbsorb(item))
                                    return new Plan(PlanDefOf.GoHaul, item) { AmountA = Math.Min(diff, item.StackSize) };
                            }
                            var places = stockpile.GetCandidateCells(carried).Where(actor.CanReachAndReserve);
                            foreach (var cell in places)
                                // emit godeliver task at place
                                return new Plan(PlanDefOf.HaulToStockpile, new TargetArgs(actor.Map, cell)) { Continuation = PlanContinuationPolicy.Yield };
                        }
                    }
                }
                // carried item is useless so place it in current cell (or throw/let it drop)
                // TODO: return null and let a final cleanup planner drop it at feet or at nearest empty cell
                // this is the final cleanup planner??
                var freeCells = actor.Map.FindNearestEmptyCellsOrCurrent(actor.Cell.Below, 3);
                var finalCell = freeCells.FirstOrDefault();
                //return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, actor.Cell));
                return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, finalCell));
            }
            // if actor has empty hands
            // iterate map items
            foreach (var item in mapItems)
            {
                if (!actor.CanReachAndReserve(item))
                    continue;
                if (actor.Map.Town.IsClaimedBySystem(item))
                    continue;
                var currentStockpile = stockpiles.FirstOrDefault(s => s.Contains(item));
                // iterate map stockpiles sorted by priority
                foreach (var stockpile in stockpiles)
                {
                    if (stockpile == currentStockpile)
                        continue;

                    int availableCapacity = stockpile.GetAvailableSpaceFor(item);
                    if (availableCapacity == 0)
                        continue;
                    // TODO only consider unreserved quantity of the stack

                    if (currentStockpile is null ||
                        !currentStockpile.Accepts(item) ||
                        stockpile.Priority > currentStockpile.Priority)
                    {
                        var t = new TargetArgs(item);
                        var unreservedAmount = map.Town.ReservationManager.GetUnreservedAmount(t);
                        return new Plan(PlanDefOf.GoHaul, item) { AmountA = Math.Min(unreservedAmount, availableCapacity) };
                    }
                }
            }

            return null;
        }
    }
    internal class PlannerHaulingOld : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var carried = actor.Hauled;
            var map = actor.Map;
            var stockpiles = map.Town.ZoneManager.GetZones<Stockpile>().OrderByDescending(i => i.Priority);
            var mapItems = map.Haulables.Where(actor.CanReachAndReserve).SortByReachableRegionDistance(actor).ToList();

            // if actor is currently carrying something
            if (carried is not null)
            {
                // if stack = stackmax
                if (carried.IsStackFull)
                {
                    // iterate stockpiles by priority 
                    foreach (var stockpile in stockpiles)
                    {
                        ////until the first one that accepts it - intvec3? or targetargs place = stockpile.findplacefor(item)
                        //var place = stockpile.FindPlaceFor(carried);

                        //if (place is not null)
                        //    // emit godeliver task at place
                        //    return new Plan(PlanDefOf.HaulToStockpile, place);

                        //until the first one that accepts it - intvec3? or targetargs place = stockpile.findplacefor(item)
                        var places = stockpile.FindPlacesFor(carried).Where(actor.CanReachAndReserve);
                        foreach(var cell in places)
                            // emit godeliver task at place
                            return new Plan(PlanDefOf.HaulToStockpile, new TargetArgs(actor.Map, cell)) { Continuation = PlanContinuationPolicy.Yield };
                    }
                } 
                // else if can carry more
                else
                {
                    // start iterating stockpiles until
                    foreach (var stockpile in stockpiles)
                    {
                        int availableCapacity = stockpile.AvailableCapacityFor(carried);
                        var diff = availableCapacity - carried.StackSize;
                        if(diff > 0)
                        {
                            // then iterate map items 
                            foreach(var item in mapItems)
                            {
                                var currentStockpile = stockpiles.FirstOrDefault(s => s.Contains(item));
                                if (currentStockpile != null && currentStockpile.Accepts(item))
                                    continue; // skip items already properly stored
                                //until a valid one is found and emit gohaul with target amount math.min(diff, target.stacksize)
                                if (carried.CanAbsorb(item))
                                    return new Plan(PlanDefOf.GoHaul, item) { AmountA = Math.Min(diff, item.StackSize) };
                            }
                            var places = stockpile.FindPlacesFor(carried).Where(actor.CanReachAndReserve);
                            foreach (var cell in places)
                                // emit godeliver task at place
                                return new Plan(PlanDefOf.HaulToStockpile, new TargetArgs(actor.Map, cell)) { Continuation = PlanContinuationPolicy.Yield };
                        }
                    }
                }
                // carried item is useless so place it in current cell (or throw/let it drop)
                // TODO: return null and let a final cleanup planner drop it at feet or at nearest empty cell
                // this is the final cleanup planner??
                var freeCells = actor.Map.FindNearestEmptyCellsOrCurrent(actor.Cell.Below, 3);
                var finalCell = freeCells.FirstOrDefault();
                //return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, actor.Cell));
                return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, finalCell));
            }
            // if actor has empty hands
            // iterate map items
            foreach (var item in mapItems)
            {
                if (!actor.CanReachAndReserve(item))
                    continue;
                if (actor.Map.Town.IsClaimedBySystem(item))
                    continue;
                var currentStockpile = stockpiles.FirstOrDefault(s => s.Contains(item));
                // iterate map stockpiles sorted by priority
                foreach (var stockpile in stockpiles)
                {
                    if (stockpile == currentStockpile)
                        continue;

                    int availableCapacity = stockpile.AvailableCapacityFor(item);
                    if (availableCapacity == 0)
                        continue;
                    // TODO only consider unreserved quantity of the stack

                    if (currentStockpile is null ||
                        !currentStockpile.Accepts(item) ||
                        stockpile.Priority > currentStockpile.Priority)
                    {
                        var t = new TargetArgs(item);
                        var unreservedAmount = map.Town.ReservationManager.GetUnreservedAmount(t);
                        return new Plan(PlanDefOf.GoHaul, item) { AmountA = Math.Min(unreservedAmount, availableCapacity) };
                    }
                }
            }
            
            return null;
        }
    }
}
