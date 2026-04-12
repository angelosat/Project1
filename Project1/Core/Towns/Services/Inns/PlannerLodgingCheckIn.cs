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
        if (manager.TryGetTransaction(actor, out var transaction))
        {
            
            if (transaction.IsPaid)
                return new Plan(InnsDefOf.PlanCheckIn, new InteractionTarget(map, transaction.Desk));
            if (transaction.IsAwaitingPayment)
            {
                if (actor.Hauled is Entity carried)
                {
                    if (carried.Def != ItemDefOf.Coins)
                        return null;
                    if (carried.StackSize != price)
                        return null;
                    return new Plan(InnsDefOf.PlanPayCheckIn, new InteractionTarget(map, transaction.Desk.Above));
                }
                else
                {
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
