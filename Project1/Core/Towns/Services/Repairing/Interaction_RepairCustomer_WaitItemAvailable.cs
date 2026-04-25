using Project1.Core.Interactions;
using System;

namespace Project1.Core.Towns.Services.Repairing
{
    internal sealed class Interaction_RepairCustomer_WaitItemAvailable : InteractionLogic
    {
        protected override InteractionContext_Customer CreateContextInt() => new();
        internal override bool HasSucceeded(Interaction i)
        {
            //var req = (ServiceRequest_Repair)i.Actor.CurrentPlan.ServiceRequest;
            var reqid = i.Actor.CurrentPlan.ServiceRequest;
            var req = (ServiceRequest_Repair)i.Actor.Town.ServiceRequests.Get(reqid);
            var item = i.Actor.World.Get(req.Item);
            if (item.IsSpawned)
                return true;
            return false;
        }
    }
}
