using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.Resources;
using System.Linq;

namespace Project1.Core.Towns.Services.Inns;

internal sealed class PlannerLodgingCheckIn : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsTownMember)
            return null;
        var map = actor.Map;
        var manager = actor.Map.Town.Inns;
        if (actor.HasCheckedIn)
            return null;
        var price = 100; // TODO: query manager for price
        if (manager.TryGetTransaction(actor, out var req))
        {
            if (req.IsPaidFor)
                return new Plan(InnsDefOf.PlanCheckIn, new InteractionTarget(map, req.Counter.Value.Above));
            if (req.IsVendorWaitingPayment)
            {
                if (actor.Hauled is Entity carried)
                {
                    if (!req.IsMoneyAllocated)
                    {
                        if (carried.Def != ItemDefOf.Coins)
                            return null;
                        if (carried.StackSize != price)
                            return null;
                        req.AllocateMoney(carried);
                    }
                    else
                    {
                        throw new System.Exception();
                        if (carried.RefId != req.Money)
                            return null;
                    }
                    return new Plan(PlanDefOf.GoPlace, new InteractionTarget(map, req.Counter.Value.Above));
                }
                else
                {
                    if (req.IsMoneyAllocated)
                        return new Plan(InnsDefOf.PlanCheckIn, new InteractionTarget(map, req.Counter.Value.Above));
                    if (!actor.Inventory.TryGet(e => e.Def == ItemDefOf.Coins && e.StackSize >= price, out Entity money))
                        return null;
                    return new Plan(PlanDefOf.RetrieveFromInventory, money) { AmountA = price };
                }
            }
            return null;
        }

        if (actor.Needs.GetPercentage(NeedDefOf.Energy) > .5f) // TODO: make it variable
            return null;
        if (actor.Resources.GetPercentage(ResourceDefOf.Patience) < .5f) // TODO: make it variable
            return null;
        var servicePoints = manager.GetServicePoints();
        if (!servicePoints.Any())
            return null;
        if (!actor.TryChoosePosition(servicePoints, out var desk))
            return null;
       
        if (!actor.Inventory.TryGet(e => e.Def == ItemDefOf.Coins && e.StackSize >= price, out _))
            return null;
        return new Plan(InnsDefOf.PlanCheckIn, new InteractionTarget(actor.Map, desk));
    }
}
