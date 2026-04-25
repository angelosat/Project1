using Project1.Core.Interactions;
using Project1.Core.Resources;
using Project1.Core.Towns.Services.Repairing;

namespace Project1.Core.Towns.Services;

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
        var reqid = i.Actor.CurrentPlan.ServiceRequest;
        var req = i.Actor.Town.ServiceRequests.Get(reqid);
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
        => i.Actor.Resources.ApplyAccumulatorDelta(ResourceDefOf.Patience, -.01f);
}
