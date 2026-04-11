using Project1.Core.Entities;
using Project1.Core.Interactions;
using Project1.Core.Resources;
using System.Linq;

namespace Project1.Core.Towns.Services.Repairing;

sealed class InteractionContext_Customer : InteractionContext
{
    internal IResourceView Patience => field ??= this.Actor.Resources.View(ResourceDefOf.Patience);
    internal override float GetPercentage(Interaction i)
        => this.Patience.Percentage;
}

sealed class InteractionContext_Vendor : InteractionContext
{
    internal ServiceRequest Request => field ??= this.Actor.CurrentPlan.ServiceRequest;
}
internal sealed class Interaction_Repair_MoneyWait : InteractionLogic
{
    protected override InteractionContext_Vendor CreateContextInt() => new();
    internal override void OnStart(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var typed = (InteractionContext_Vendor)i.Context;
        typed.Request.MarkVendorWaitingPayment();
    }
    internal override bool HasSucceeded(Interaction i)
    {
        var typed = (InteractionContext_Vendor)i.Context;
        //var moneyCell = typed.Request.Counter.Value.Above;
        //var itemsInCell = i.Actor.Map.GetEntitiesAt(moneyCell);
        //if (itemsInCell.FirstOrDefault(e => e.Def == ItemDefOf.Coins && e.StackSize >= typed.Request.Price) is Entity money)
        //{
        //    //typed.Request.Money = money.RefId;
        //    return true;
        //}
        if (i.Actor.World.Get(typed.Request.Money) is Entity item && item.Cell == typed.Request.Counter.Value.Above)
            return true;
        return false;
    }
}
internal sealed class Interaction_RepairServing : InteractionLogic
{
    internal override void OnStart(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var req = i.Actor.CurrentPlan.ServiceRequest;
        //req.IsVendorWaiting = true;
        req.MarkVendorWaiting();
    }

    internal override bool HasSucceeded(Interaction i)
    {
        var req = (ServiceRequest_Repair)i.Actor.CurrentPlan.ServiceRequest;
        if (i.Actor.Map.World.Get(req.Item).Cell == req.Counter.Value.Above)
            return true;
        return false;
    }
    internal override void OnSuccess(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var req = i.Actor.CurrentPlan.ServiceRequest;
        //req.IsVendorWorking = true;
        req.MarkVendorWorking();
    }
}

internal sealed class Interaction_RepairCustomerWaitItem : InteractionLogic
{
    protected override InteractionContext_Customer CreateContextInt() => new();
    internal override bool HasSucceeded(Interaction i)
    {
        var req = (ServiceRequest_Repair)i.Actor.CurrentPlan.ServiceRequest;
        var item = i.Actor.World.Get(req.Item);
        if (item.Cell == req.Counter.Value.Above)
            return true;
        return false;
    }
}
internal sealed class Interaction_RepairCustomerWaitPrice : InteractionLogic
{
    protected override InteractionContext_Customer CreateContextInt() => new();

    internal override bool HasSucceeded(Interaction i)
    {
        var req = (ServiceRequest_Repair)i.Actor.CurrentPlan.ServiceRequest;
        if (req.IsVendorWaitingPayment)
            return true;
        //var item = i.Actor.Map.World.Get(req.Item);
        //if (item.Cell == req.Counter.Value.Above && item.Resources.GetPercentage(ResourceDefOf.Durability) >= 1)
        //    return true;
        return false;
    }

    internal override bool HasFailed(Interaction i)
        => i.Actor.Resources.GetPercentage(ResourceDefOf.Patience) <= 0;

    internal override void OnTick(Interaction i)
        => i.Actor.Resources.ApplyDelta(ResourceDefOf.Patience, -.01f);
}
internal sealed class Interaction_RepairQueue : InteractionLogic
{
    protected override InteractionContext_Customer CreateContextInt() => new();
    internal override void OnStart(Interaction i)
    {
        //i.Actor.Map.Town.ServiceRequests.Enqueue(i.Actor, i.Target.Global);
        if (i.Actor.Net.IsClient)
            return;
        i.Actor.Map.Town.ServiceRequests.Enqueue(i.Actor);
    }

    internal override bool HasSucceeded(Interaction i)
    {
        var req = i.Actor.CurrentPlan.ServiceRequest;
        if (req.IsVendorWaiting)
            return true;
        return false;
    }

    internal override bool HasFailed(Interaction i)
        => i.Actor.Resources.GetPercentage(ResourceDefOf.Patience) <= 0;

    internal override void OnTick(Interaction i)
        => i.Actor.Resources.ApplyDelta(ResourceDefOf.Patience, -.01f);
}
