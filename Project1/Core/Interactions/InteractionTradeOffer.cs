using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Trading;
using System;

namespace Project1.Core.Interactions;

sealed class InteractionContext_Trade : InteractionContext
{
    internal TownComp_Trade Manager => field ??= this.Actor.Map.Town.Trades;
    internal TradeRuntime TradeByGiver => field ??= this.Manager.GetTradeByGiver(this.Actor);
    internal TradeRuntime TradeByRecipient => field ??= this.Manager.GetTradeByRecipient(this.Actor);
}

sealed class InteractionTradeComplete : InteractionLogic
{
    protected override InteractionContext_Trade CreateContextInt() => new();

    TradeRuntime Trade(InteractionContext ctx) => ((InteractionContext_Trade)ctx).TradeByRecipient;

    internal override void OnAnimationHook(Interaction i)
    {
        var actor = i.Actor;
        if (actor.Net.IsClient)
            return;
        var other = i.Target.Entity as Actor;
        var item = other.Hauled ?? throw new InvalidOperationException();
        actor.Inventory.HaulNew(item, item.StackSize);
        actor.Map.Town.Trades.MarkComplete(actor);
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
    internal override void OnAnimationHook(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        i.Actor.Map.Town.Trades.MarkOffered(i.Actor);
    }
    internal override bool HasSucceeded(Interaction i)
    {
        var actor = i.Actor;
        var target = i.Target.Entity as Actor;
        var manager = actor.Map.Town.Trades;
        var trade = manager.GetTradeByGiver(actor);
        if (actor.Hauled?.RefId != trade.Item && target.Hauled.RefId == trade.Item)
            return true;
        return false;
    }
    internal override bool HasFailed(Interaction i)
    {
        var actor = i.Actor;
        var manager = actor.Map.Town.Trades;
        var trade = manager.GetTradeByGiver(actor);
        if (trade.IsDeclined)
            return true;
        return false;
    }
}
