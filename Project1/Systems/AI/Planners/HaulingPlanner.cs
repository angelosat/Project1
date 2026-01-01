using System;
using System.Linq;

namespace Start_a_Town_
{
    internal class HaulingPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var carried = actor.Hauled as Entity;

            var stockpiles = actor.Map.Town.ZoneManager.GetZones<Stockpile>().OrderByDescending(i => i.Priority);
            var mapItems = actor.Map.GetEntities().Where(e => actor.CanReach(e) && actor.CanReserve(e)).Cast<Entity>().SortByReachableRegionDistance(actor).ToList();

            // if actor is currently carrying something
            if (carried is not null)
            {
                // if stack = stackmax
                if (carried.IsStackFull)
                {
                    // iterate stockpiles by priority 
                    foreach (var stockpile in stockpiles)
                    {
                        //until the first one that accepts it - intvec3? or targetargs place = stockpile.findplacefor(item)
                        var place = stockpile.FindPlaceFor(carried);

                        if (place is not null)
                            // emit godeliver task at place
                            return new Plan(PlanDefOf.GoPlace, place);
                    }
                } 
                // else if can carry more
                else
                {
                    // start iterating stockpiles until
                    foreach (var stockpile in stockpiles)
                    {
                        int availableCapacity = stockpile.AvailableCapacityFor(carried);
                        // var diff = stockpile.howmanycanacceptof(item) - actor.hauled.stacksize;
                        // diff > 0
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
                            var place = stockpile.FindPlaceFor(carried);
                            return new Plan(PlanDefOf.GoPlace, place);
                        }
                    }
                }
                // carried item is useless so place it in current cell (or throw/let it drop)
                // TODO: return null and let a final cleanup planner drop it at feet or at nearest empty cell
                //IntVec3 empty = actor.FindNearestEmptyCellOrCurrent();
                return new Plan(PlanDefOf.DropCarried, new TargetArgs(actor.Map, actor.Cell));
            }
            // if actor has empty hands
            // iterate map items
            foreach (var item in mapItems)
            {
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
                    if (currentStockpile == null || 
                        !currentStockpile.Accepts(item) || 
                        stockpile.Priority >= currentStockpile.Priority)
                        return new Plan(PlanDefOf.GoHaul, item) { AmountA = Math.Min(item.StackSize, availableCapacity) };
                }
            }
            
            return null;
        }
        //var shouldTransfer = !currentStockpile?.Accepts(item) ?? true;

        //if (shouldTransfer && availableCapacity > 0)

        // if the candidate stockiples accepts the item - stockpile.howmanycanacceptof(item) > 0
        // and the item is currently not at a stockpile,
        // if item is already at a stockpile with a lower priority than the candidate stockpile,
        // or the current stockpile no longer accepts the item, 

        // then emit simple gohaul task to pick up stockpile.howmanycanacceptof(item) quantity of the target item

        static bool IsItemAtBestStockpile(Entity item)
        {
            var stockpiles = item.Map.Town.ZoneManager.GetZones<Stockpile>();
            var currentStockpile = stockpiles.FirstOrDefault(s => s.Contains(item));
            if (currentStockpile == null)
                return false;
            var betterStockpile = stockpiles
                .Where(s =>
                    s != currentStockpile && 
                    s.Priority > currentStockpile.Priority &&
                    s.CanAccept(item))
                .OrderByDescending(s => s.Priority)
                .FirstOrDefault();
            return betterStockpile == null && currentStockpile.Accepts(item);
        }
    }
}
