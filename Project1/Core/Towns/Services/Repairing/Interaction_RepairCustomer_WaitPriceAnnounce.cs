using Project1.Core.Interactions;
using Project1.Core.Resources;
using System;

namespace Project1.Core.Towns.Services.Repairing
{
    internal sealed class Interaction_RepairCustomer_WaitPriceAnnounce : InteractionLogic
    {
        protected override InteractionContext_Customer CreateContextInt() => new();

        internal override bool HasSucceeded(Interaction i)
        {
            //var req = (ServiceRequest_Repair)i.Actor.CurrentPlan.ServiceRequest;
            var reqid = i.Actor.CurrentPlan.ServiceRequest;
            var req = (ServiceRequest_Repair)i.Actor.Town.ServiceRequests.Get(reqid);
            if (req.IsVendorWaitingPayment)
                return true;
            return false;
        }

        internal override bool HasFailed(Interaction i)
            => i.Actor.Resources.GetPercentage(ResourceDefOf.Patience) <= 0;

        internal override void OnTick(Interaction i)
            => i.Actor.Resources.ApplyAccumulatorDelta(ResourceDefOf.Patience, -.01f);
    }
}
