using Project1.Core.Interactions;
using System;

namespace Project1.Core.Towns.Services.Repairing
{
    sealed class InteractionContext_Vendor : InteractionContext
    {
        internal ServiceRequest Request => field ??= this.Actor.Town.ServiceRequests.Get(this.Actor.CurrentPlan.ServiceRequest);
    }
}
