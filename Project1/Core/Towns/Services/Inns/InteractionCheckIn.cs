using Project1.Core.Interactions;
using Project1.Core.Resources;

namespace Project1.Core.Towns.Services.Inns;

sealed class InteractionCheckIn : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal IResourceView Patience => field ??= this.Actor.Resources.View(ResourceDefOf.Patience);
        internal override float GetPercentage(Interaction i) => ((Context)i.Context).Patience.Percentage;

    }

    protected override InteractionContext CreateContextInt()
        => new Context();

    internal override void OnStart(Interaction i)
        => i.Actor.Map.Town.Inns.TryEnqueue(i.Actor, i.Target.Global);

    //internal override bool HasSucceeded(Interaction i)
    //    => i.Actor.HasCheckedIn || i.Actor.Map.Town.Inns.GetTransactionByGuest(i.Actor).IsAwaitingPayment;
    internal override bool HasSucceeded(Interaction i)
    {
        if (i.Actor.HasCheckedIn)
            return true;
        if (i.Actor.Map.Town.Inns.GetTransactionByGuest(i.Actor).IsVendorWaitingPayment)
            return true;
        return false;
    }

    internal override void OnSuccess(Interaction i)
        => i.Actor.AI.State.Log.Write($"I have checked in successfully");

    internal override bool HasFailed(Interaction i)
        => i.Actor.Resources.GetValue(ResourceDefOf.Patience) <= 0;
    internal override void OnFailure(Interaction i)
        => i.Actor.Map.Town.Inns.AbortQueuing(i.Actor);

    internal override void OnTick(Interaction i)
        => i.Actor.Resources.ApplyDelta(ResourceDefOf.Patience, -.01f);
}
