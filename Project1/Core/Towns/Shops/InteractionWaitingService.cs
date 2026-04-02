using Project1.Core.Interactions;
using Project1.Core.Resources;

namespace Project1.Core.Towns.Shops;
internal sealed class InteractionWaitingService : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ShopTransaction Transaction => field ??= this.Actor.Map.Town.ShopManager.GetTransaction(this.Actor);
        internal IResourceView Patience => field ??= this.Actor.Resources.View(ResourceDefOf.Patience);
        internal override float GetPercentage(Interaction i) => ((Context)i.Context).Patience.Percentage;
        //public override float ProgressBarPercentage => this.Transaction?.WaitingForPayment ?? false ? 1 : 0;
    }

    protected override InteractionContext CreateContextInt()
        => new Context();

    public override bool CanPerform(InteractionContext ctx)
    {
        var typedCtx = (Context)ctx;
        typedCtx.Transaction.Tick();
        if (typedCtx.Transaction.IsFailed)
            return false;
        if (typedCtx.Transaction.WaitingForPayment)
            return false;
        if (typedCtx.Transaction.IsProcessed)
            return false;
        return true;
    }

    internal override void OnTick(Interaction i)
            => i.Actor.Resources.ApplyDelta(ResourceDefOf.Patience, -.01f);

    internal override bool HasSucceeded(Interaction i)
    {
        var typedCtx = (Context)i.Context;
        if (typedCtx.Transaction.WaitingForPayment)
            return true;
        if (typedCtx.Transaction.IsProcessed)
            return true;
        return false;
    }

    internal override bool HasFailed(Interaction i)
    {
        var typedCtx = (Context)i.Context;
        if (typedCtx.Patience.Percentage <= 0)
            return true;
        if (typedCtx.Transaction.IsFailed)
            return true;
        return false;
    }
}
