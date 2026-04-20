using System.Diagnostics;

namespace Project1.Core.Interactions;

sealed class InteractionRingUpTransactionFinish : InteractionLogic
{

    internal override void OnStart(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        Debug.Assert(i.Actor.CurrentPlan.ServiceRequest != null);
    }
    internal override void OnFinish(Interaction i)
    {
        var ctx = i.Context;
        var actor = ctx.Actor;
        if (actor.Net.IsClient)
            return;
        var money = ctx.Target.Entity;
        var carried = actor.Hauled;
        var reqid = i.Actor.CurrentPlan.ServiceRequest;
        var req = i.Actor.Town.ServiceRequests.Get(reqid);
        Debug.Assert(carried.RefId == req.Item);
        Debug.Assert(money.RefId == req.Money);
        carried.SetOwnerNew(req.Customer);
        money.SetOwnerNew(null);

        InteractionHelpers.TrySwapHauledItem(actor, money, ctx.Count);

        actor.Map.Town.Shops.MarkPaidFor(actor);
    }
}
