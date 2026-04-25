using Project1.Core.Interactions;

namespace Project1.Core.Towns.Services.Repairing;

internal sealed class Interaction_Vendor_WaitItemSubmit : InteractionLogic
{
    internal override void OnStart(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var reqid = i.Actor.CurrentPlan.ServiceRequest;
        var req = i.Actor.Town.ServiceRequests.Get(reqid);
        req.MarkVendorWaiting();
    }

    internal override bool HasSucceeded(Interaction i)
    {
        //var req = i.Actor.CurrentPlan.ServiceRequest;
        //if (i.Actor.Map.World.Get(req.Item).Cell == req.Counter.Value.Above)
        //    return true;
        var reqid = i.Actor.CurrentPlan.ServiceRequest;
        var req = i.Actor.Town.ServiceRequests.Get(reqid);
        if (req.IsItemSubmitted(i.Actor.Map.World))
            return true;
        return false;
    }
    internal override void OnSuccess(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var reqid = i.Actor.CurrentPlan.ServiceRequest;
        var req = i.Actor.Town.ServiceRequests.Get(reqid);
        req.MarkVendorWorking();
    }
}
