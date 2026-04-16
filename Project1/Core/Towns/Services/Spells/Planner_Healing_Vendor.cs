using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Magic;

namespace Project1.Core.Towns.Services.Spells;

internal class Planner_Healing_Vendor : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        var map = actor.Map;
        var manager = map.Town.Spells;
        if (manager.TryGetRequestByCaster(actor, out var existing))
        {
            //if (existing.IsDisposed)
            //    return null;
            var target = map.World.Get<Actor>(existing.Customer);
            if (!actor.CanReachAndReserve(target))
                return null;
            if (!existing.IsPaidFor)
            {
                var trade = map.Town.Trades.GetTradeById(existing.PaymentId);
                if (trade.IsOffered)
                    return new Plan(PlanDefOf.TradeComplete, target) { ServiceRequest = existing, TradeId = existing.PaymentId };
                if (trade.IsComplete)
                {
                    map.Town.Trades.MarkDisposed(trade);
                    manager.MarkPaid(existing, actor);
                    return new Plan(PlanDefOf.StoreInInventory);
                }
                map.Town.Trades.MarkAccepted(trade.Id);
                return new Plan(HealingDefOf.PlanHealingWaitPay) { ServiceRequest = existing, TradeId = existing.PaymentId };
            }
            else
            {
                if (existing.IsTargetReady)
                    return new Plan(SpellDefOf.PlanCastSpell, target) { ServiceRequest = existing, Spell = existing.Spell, Continuation = PlanContinuationPolicy.Yield };
                return new Plan(HealingDefOf.PlanHealingWaitCaster) { ServiceRequest = existing };
            }
        }
        var allRequests = manager.PendingRequests;
        foreach (var req in allRequests)
        {
            if (req.IsVendorAssigned)
                continue;
            var target = map.World.Get<Actor>(req.Customer);
            if (!actor.CanReach(target))
                continue;
            manager.MarkAccepted(req, actor);
            return null;
        }
        return null;
    }
}
