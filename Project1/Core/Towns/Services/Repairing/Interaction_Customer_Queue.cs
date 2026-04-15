using Project1.Core.Interactions;
using Project1.Core.Resources;
using System;

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
internal sealed class Interaction_Vendor_WaitPayment : InteractionLogic
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

        //if (i.Actor.World.Get(typed.Request.Money)?.IsSpawned ?? false)
        //    return true;
        //return false;

        var money = i.Actor.World.Get(typed.Request.Money);
        if (money is null)
            return false;
        if (money.HasOwner)
            throw new InvalidOperationException("money entity shouldn't have ended up on the counter without its owner set to null");
            //return false;
        if (!money.IsSpawned)
            return false;
        return true;
    }
}
internal sealed class Interaction_Vendor_WaitItemSubmit : InteractionLogic
{
    internal override void OnStart(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var req = i.Actor.CurrentPlan.ServiceRequest;
        req.MarkVendorWaiting();
    }

    internal override bool HasSucceeded(Interaction i)
    {
        var req = i.Actor.CurrentPlan.ServiceRequest;
        //if (i.Actor.Map.World.Get(req.Item).Cell == req.Counter.Value.Above)
        //    return true;
        if (req.IsItemSubmitted(i.Actor.Map.World))
            return true;
        return false;
    }
    internal override void OnSuccess(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var req = i.Actor.CurrentPlan.ServiceRequest;
        req.MarkVendorWorking();
    }
}

internal sealed class Interaction_RepairCustomer_WaitItemAvailable : InteractionLogic
{
    protected override InteractionContext_Customer CreateContextInt() => new();
    internal override bool HasSucceeded(Interaction i)
    {
        var req = (ServiceRequest_Repair)i.Actor.CurrentPlan.ServiceRequest;
        var item = i.Actor.World.Get(req.Item);
        if(item.IsSpawned)
            return true;
        return false;
    }
}
internal sealed class Interaction_RepairCustomer_WaitPriceAnnounce : InteractionLogic
{
    protected override InteractionContext_Customer CreateContextInt() => new();

    internal override bool HasSucceeded(Interaction i)
    {
        var req = (ServiceRequest_Repair)i.Actor.CurrentPlan.ServiceRequest;
        if (req.IsVendorWaitingPayment)
            return true;
        return false;
    }

    internal override bool HasFailed(Interaction i)
        => i.Actor.Resources.GetPercentage(ResourceDefOf.Patience) <= 0;

    internal override void OnTick(Interaction i)
        => i.Actor.Resources.ApplyDelta(ResourceDefOf.Patience, -.01f);
}
internal sealed class Interaction_Customer_Queue : InteractionLogic
{
    protected override InteractionContext_Customer CreateContextInt() => new();
    internal override void OnStart(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        i.Actor.Map.Town.ServiceRequests.Enqueue(i.Actor);
    }

    internal override bool HasSucceeded(Interaction i)
    {
        var req = i.Actor.CurrentPlan.ServiceRequest;
        if (req.IsVendorWaitingItemSubmit)
            return true;
        // can i shove this condition here too or do i need separate interactions?
        if (req.IsVendorWaitingPayment)
            return true;
        if (req.IsSucceeded)
            return true;
        return false;
    }

    internal override bool HasFailed(Interaction i)
        => i.Actor.Resources.GetPercentage(ResourceDefOf.Patience) <= 0;

    internal override void OnTick(Interaction i)
        => i.Actor.Resources.ApplyDelta(ResourceDefOf.Patience, -.01f);
}
