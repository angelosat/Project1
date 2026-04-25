using Project1.Core.Interactions;
using System;

namespace Project1.Core.Towns.Services.Repairing
{
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
}
