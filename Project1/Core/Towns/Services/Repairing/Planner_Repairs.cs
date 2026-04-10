using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Personality;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Project1.Core.Towns.Services.Repairing;

internal class Planner_Repairs : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        var manager = actor.Map.Town.Repairs;

        if(manager.TryGet(actor, out var existing))
        {
            var item = actor.World.Get(existing.Item);
            if (actor.Hauled is Entity carried)
            {
                if (carried != item)
                    return null;
                if (!actor.CanReachAndReserve(existing.Counter))
                    return null;
                return new Plan(PlanDefOf.)
            }
            else
                return new Plan(PlanDefOf.RetrieveFromInventory, item);
            throw new UnreachableException();
        }

        var inventory = actor.Inventory;
        var durThreshold = .5f + actor.Personality.GetPercentage(TraitDefOf.Deliberation) / 2f;
        //var damagedItems = inventory.FindAll(i => i.Resources.GetPercentage(ResourceDefOf.Durability) < durThreshold);
        //var mostDamaged = damagedItems.OrderBy(i=>)
        var damaged = inventory.Score(e => (int)e.Resources?.GetValueOrDefault(ResourceDefOf.Durability));
        var mostDamaged = damaged
            .Where(e => e.score < durThreshold)
            .OrderBy(i => i.score)
            .FirstOrDefault();
        if (mostDamaged.item is null)
            return null;
        var counters = manager.Counters;
        foreach(var counter in counters)
        {
            if (!actor.CanReachAndReserve(counter))
                continue;
            manager.Begin(actor, mostDamaged.item, mostDamaged.score, counter);
            return null;
        }
        return null;
    }
}
