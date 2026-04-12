using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Magic;
using System.Diagnostics;

namespace Project1.Core.Towns.Services.Healing;

internal class PlannerHealingOffer : Planner
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
                    return new Plan(PlanDefOf.TradeComplete, target) { TradeId = existing.PaymentId };
                if (trade.IsComplete)
                {
                    map.Town.Trades.MarkDisposed(trade);
                    manager.MarkPaid(existing, actor);
                    return new Plan(PlanDefOf.StoreInInventory);
                }
                map.Town.Trades.MarkAccepted(trade.Id);
                return new Plan(HealingDefOf.PlanHealingWaitPay) { TradeId = existing.PaymentId };
            }
            else
            {
                if (existing.IsTargetReady)
                    return new Plan(SpellDefOf.PlanCastSpell, target) { Spell = SpellDefOf.Healing, Continuation = PlanContinuationPolicy.Yield };
                return new Plan(HealingDefOf.PlanHealingWaitCaster);
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
internal class PlannerHealingSeek : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        var healthPerc = actor.Resources.GetPercentage(ResourceDefOf.Health);
        var threshold = .5f;
        if (healthPerc > threshold)
            return null;
        var map = actor.Map;
        var manager = map.Town.Spells;
        if (manager.TryGetRequestByTarget(actor, out var existing))
        {
            var caster = map.World.Get<Actor>(existing.Vendor);
            if (!actor.CanReachAndReserve(caster))
                return null;
            if (existing.IsPaidFor)
            {
                if (existing.IsCasterReady && !actor.IsHauling)
                    return new Plan(HealingDefOf.PlanHealingSeek, caster);
                return null;
            }
            else
            {
                var trade = actor.Map.Town.Trades.GetTradeById(existing.PaymentId);

                if (trade.IsComplete)
                    return new Plan(HealingDefOf.PlanHealingSeek, caster);

                if (!trade.IsAccepted)
                    return null;

                if (actor.Hauled is Entity carried)
                {
                    if (carried.Def != ItemDefOf.Coins || carried.StackSize != existing.Price)
                        return null;
                    actor.Map.Town.Trades.MarkItem(trade.Id, carried);
                    return new Plan(PlanDefOf.TradeOffer, caster) { TradeId = trade.Id };
                }
                else
                {
                    var money = actor.Inventory.First(i => i.Def == ItemDefOf.Coins);
                    return new Plan(PlanDefOf.RetrieveFromInventory, money) { AmountA = existing.Price };
                }
            }

            throw new UnreachableException();
        }
        if (actor.IsHauling)
            return null;
        var req = manager.Request(actor, SpellDefOf.Healing);

        return null;
    }
}
