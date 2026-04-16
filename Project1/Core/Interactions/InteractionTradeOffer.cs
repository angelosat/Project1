using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Trading;
using System;

namespace Project1.Core.Interactions;

sealed class InteractionContext_Trade : InteractionContext
{
    internal TownComp_Trade Manager => field ??= this.Actor.Map.Town.Trades;
    internal TradeRuntime Trade => field ??= this.Manager.GetTradeById(this.Actor.CurrentPlan.TradeId);
}

sealed class InteractionTradeComplete : InteractionLogic
{
    protected override InteractionContext_Trade CreateContextInt() => new();

    static TradeRuntime Trade(InteractionContext ctx) => ((InteractionContext_Trade)ctx).Trade;

    internal override void OnAnimationHook(Interaction i)
    {
        var actor = i.Actor;
        if (actor.Net.IsClient)
            return;
        var other = i.Target.Entity as Actor;
        var item = other.Hauled ?? throw new InvalidOperationException();
        actor.Inventory.HaulNew(item, item.StackSize);
        actor.Map.Town.Trades.MarkComplete(Trade(i.Context).Id);
    }
    internal override bool HasSucceeded(Interaction i)
    {
        if (i.Actor.Hauled?.RefId == Trade(i.Context).Item)
            return true;
        return false;
    }
}
sealed class InteractionTradeOffer : InteractionLogic
{
    protected override InteractionContext_Trade CreateContextInt() => new();
    static TradeRuntime Trade(InteractionContext ctx) => ((InteractionContext_Trade)ctx).Trade;

    internal override void OnAnimationHook(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        i.Actor.Map.Town.Trades.MarkOffered(Trade(i.Context).Id);
    }
    internal override bool HasSucceeded(Interaction i)
    {
        var actor = i.Actor;
        var target = i.Target.Entity as Actor;
        var trade = Trade(i.Context);
        if (actor.Hauled?.RefId != trade.Item && target.Hauled.RefId == trade.Item)
            return true;
        return false;
    }
    internal override void OnSuccess(Interaction i)
    {
        var trade = Trade(i.Context);
        var item = i.Actor.World.Get(trade.Item);
        item.SetOwnerNew(null);// i.Target.Entity as Actor);
    }
    internal override bool HasFailed(Interaction i)
    {
        var actor = i.Actor;
        var manager = actor.Map.Town.Trades;
        var trade = Trade(i.Context);
        if (trade.IsDeclined)
            return true;
        return false;
    }
}
