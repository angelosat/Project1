using Project1.Core.Interactions;
using Project1.Core.Resources;

namespace Project1.Core.Towns.Inns
{
    sealed class InteractionCheckIn : InteractionLogic
    {
        sealed class Context : InteractionContext
        {
            internal IResourceView Patience => field ??= this.Actor.Resources.View(ResourceDefOf.Patience);
            internal override float GetPercentage(Interaction i) => ((Context)i.Context).Patience.Percentage;

        }

        protected override InteractionContext CreateContextInternal()
            => new Context();

        internal override void OnStart(Interaction i)
            => i.Actor.Map.Town.InnManager.TryEnqueue(i.Actor, i.Target.Global);

        internal override bool HasSucceeded(Interaction i)
            => i.Actor.HasCheckedIn || i.Actor.Map.Town.InnManager.GetTransactionByGuest(i.Actor).IsAwaitingPayment;
        internal override void OnSuccess(Interaction i)
            => i.Actor.AI.State.Log.Write($"I have checked in successfully");

        internal override bool HasFailed(Interaction i)
            => i.Actor.Resources.GetValue(ResourceDefOf.Patience) <= 0;
        internal override void OnFailure(Interaction i)
            => i.Actor.Map.Town.InnManager.AbortQueuing(i.Actor);

        internal override void OnTick(Interaction i)
            => i.Actor.Resources.ApplyDelta(ResourceDefOf.Patience, -.01f);
    }
}
