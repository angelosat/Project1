using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns;

sealed class PlannerRestocking : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        var map = actor.Map;
        var restockables = GetRestockables(map);

        var target = restockables.FirstOrDefault(c =>
            c.GetPercentage(ResourceDefOf.Cash) < .5f &&
            actor.CanReachAndReserve(c.Parent.OriginGlobal));

        if (target is null)
            return null;

        var deficit = target.GetDeficit(ResourceDefOf.Cash);
        var candidateCoinsInStockpiles = map.Stockpiles.AllItems.Where(i => i.Def == ItemDefOf.Coins);

        if (actor.Hauled is Entity carried)
        {
            var carriedCashValue = carried.StackSize;

            if (deficit > carriedCashValue)
            {
                foreach (var nextItem in candidateCoinsInStockpiles.Where(i => actor.CanReachAndReserve(i) && carried.CanAbsorb(i)))
                {
                    var nextCoins = nextItem.StackSize;
                    var amountToPickUp = Math.Min(carried.StackMax - carried.StackSize, (int)(deficit / nextCoins));
                    return new Plan(PlanDefOf.GoHaul, nextItem) { AmountA = amountToPickUp };
                }
            }
            return new Plan(PlanDefOf.GoPlace, new TargetArgs(map, target.Parent.OriginGlobal)) { TargetB = new TargetArgs(target.Parent) };
        }
        foreach (var i in candidateCoinsInStockpiles)
        {
            if (!actor.CanReachAndReserve(i))
                continue;
            var iCashvalue = i.StackSize;
            var amountToPickUp = Math.Min(i.StackSize, (int)(i.StackSize * deficit / iCashvalue));
            return new Plan(PlanDefOf.GoHaul, i) { AmountA = amountToPickUp };
        }

        return null;
    }

    static IEnumerable<BlockResourcesComp> GetRestockables(MapBase map)
        => map.BlockEntities
            .Where(e => e.HasComp<BlockResourcesComp>())
            .Select(e => e.GetComp<BlockResourcesComp>())
            .Where(c => c.HasResource(ResourceDefOf.Cash));

}
