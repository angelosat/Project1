using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Magic;
using System.Diagnostics;

namespace Project1.Core.Towns.Healing;

internal class PlannerHealingOffer : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        var map = actor.Map;
        var manager = map.Town.SpellManager;
        if(manager.TryGetRequestByCaster(actor, out var existing))
        {
            if (existing.IsDisposed)
                return null;
            var target = map.World.Get<Actor>(existing.TargetId);
            if (!actor.CanReachAndReserve(target))
                return null;
            if (existing.IsWaitingPay)
            {
                var trade = map.Town.Trades.GetTradeByRecipient(actor);
                if (trade.IsOffered)
                    return new Plan(PlanDefOf.TradeComplete, target);
                if (trade.IsComplete)
                {
                    map.Town.Trades.MarkDisposed(trade);
                    manager.MarkPaid(existing, actor);
                    return new Plan(PlanDefOf.StoreInInventory);
                }
                map.Town.Trades.MarkAccepted(actor);
                return new Plan(HealingDefOf.PlanHealingWaitPay);
            }
            if (existing.IsPaid)
                return new Plan(SpellDefOf.PlanCastSpell, target) { Spell = SpellDefOf.Healing, Continuation = PlanContinuationPolicy.Yield };
            return new Plan(HealingDefOf.PlanHealingWaitCaster);
        }
        var allRequests = manager.PendingRequests;
        foreach(var req in allRequests)
        {
            if (!req.IsPending)
                continue;
            var target = map.World.Get<Actor>(req.TargetId);
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
        var manager = map.Town.SpellManager;
        if(manager.TryGetRequestByTarget(actor, out var existing))
        {
            var caster = map.World.Get<Actor>(existing.CasterId);
            if (!actor.CanReachAndReserve(caster))
                return null;
            if (existing.IsAccepted && !actor.IsHauling)
                return new Plan(HealingDefOf.PlanHealingSeek, caster);

            if (existing.IsWaitingPay)
            {
                if(!actor.Map.Town.Trades.TryGetTradeByGiver(actor, out var trade))
                    trade = actor.Map.Town.Trades.Request(actor, caster);

                if (trade.IsComplete)
                    return new Plan(HealingDefOf.PlanHealingSeek, caster);

                if (!trade.IsAccepted)
                    return null;

                if (actor.Hauled is Entity carried)
                {
                    if (carried.Def != ItemDefOf.Coins || carried.StackSize != existing.Price)
                        return null;
                    actor.Map.Town.Trades.MarkItem(actor, carried);
                    return new Plan(PlanDefOf.TradeOffer, caster);
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
