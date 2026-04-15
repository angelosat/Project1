using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Magic;
using System.Diagnostics;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Project1.Core.Towns.Services.Spells;

internal class Planner_Healing_Customer : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        //var healthPerc = actor.Resources.GetPercentage(ResourceDefOf.Health);
        //var threshold = .5f;
        //if (healthPerc > threshold)
        //    return null;
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
                    return new Plan(HealingDefOf.PlanHealingSeek, caster) { ServiceRequest = existing };
                    //return new Plan(TownServicesDefOf.PlanQueue, caster) { ServiceRequest = existing };
                return null;
            }
            else
            {
                var trade = actor.Map.Town.Trades.GetTradeById(existing.PaymentId);

                if (trade.IsComplete)
                    return new Plan(HealingDefOf.PlanHealingSeek, caster) { ServiceRequest = existing };
                    //return new Plan(TownServicesDefOf.PlanQueue, caster) { ServiceRequest = existing };

                if (!trade.IsAccepted)
                    return null;

                if (actor.Hauled is Entity carried)
                {
                    if (carried.Def != ItemDefOf.Coins || carried.StackSize != existing.Price)
                        return null;
                    actor.Map.Town.Trades.MarkItem(trade.Id, carried);
                    return new Plan(PlanDefOf.TradeOffer, caster) { ServiceRequest = existing, TradeId = trade.Id };
                }
                else
                {
                    var money = actor.Inventory.First(i => i.Def == ItemDefOf.Coins);
                    return new Plan(PlanDefOf.RetrieveFromInventory, money) { ServiceRequest = existing, AmountA = existing.Price };
                }
            }

            throw new UnreachableException();
        }
        if (actor.IsHauling)
            return null;
        //var req = manager.Request(actor, SpellDefOf.Healing);
        var availableSpells = manager.GetAvailableSpells();
        var scored = availableSpells.Select(s => (tag: s, score: Score(actor, s.Spell))).OrderByDescending(s => s.score);
        var (tag, score) = scored.FirstOrDefault();
        if(tag is not null)
            manager.Request(actor, tag.Spell);
        return null;
    }
    static int Score(Actor customer, SpellDef spell)
        => spell.Effects.Sum(a => SpellSystem.Score(customer, a.effect, a.target));
    //{
        //var total = 0;
        //foreach (var (effect, target) in spell.Effects)
        //    total += SpellSystem.Score(customer, effect, target);
        //return total;
    //}
}
//internal class Planner_Healing_Customer : Planner
//{
//    protected override Plan TryPlan(Actor actor)
//    {
//        var healthPerc = actor.Resources.GetPercentage(ResourceDefOf.Health);
//        var threshold = .5f;
//        if (healthPerc > threshold)
//            return null;
//        var map = actor.Map;
//        var manager = map.Town.Spells;
//        if (manager.TryGetRequestByTarget(actor, out var existing))
//        {
//            var caster = map.World.Get<Actor>(existing.Vendor);
//            if (!actor.CanReachAndReserve(caster))
//                return null;
//            if (existing.IsPaidFor)
//            {
//                if (existing.IsCasterReady && !actor.IsHauling)
//                    return new Plan(HealingDefOf.PlanHealingSeek, caster);
//                return null;
//            }
//            else
//            {
//                var trade = actor.Map.Town.Trades.GetTradeById(existing.PaymentId);

//                if (trade.IsComplete)
//                    return new Plan(HealingDefOf.PlanHealingSeek, caster);

//                if (!trade.IsAccepted)
//                    return null;

//                if (actor.Hauled is Entity carried)
//                {
//                    if (carried.Def != ItemDefOf.Coins || carried.StackSize != existing.Price)
//                        return null;
//                    actor.Map.Town.Trades.MarkItem(trade.Id, carried);
//                    return new Plan(PlanDefOf.TradeOffer, caster) { TradeId = trade.Id };
//                }
//                else
//                {
//                    var money = actor.Inventory.First(i => i.Def == ItemDefOf.Coins);
//                    return new Plan(PlanDefOf.RetrieveFromInventory, money) { AmountA = existing.Price };
//                }
//            }

//            throw new UnreachableException();
//        }
//        if (actor.IsHauling)
//            return null;
//        var req = manager.Request(actor, SpellDefOf.Healing);
//        return null;
//    }
//}
