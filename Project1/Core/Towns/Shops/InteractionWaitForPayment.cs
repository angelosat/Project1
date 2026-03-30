using Project1.Core.Interactions;

namespace Project1.Core.Towns.Shops;

internal sealed class InteractionWaitForPayment : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ShopTransaction Transaction => field ??= this.Actor.Map.Town.ShopManager.GetTransactionBySeller(this.Actor);
        internal int? Price => field ??= this.Actor.World.GetEntity(this.Transaction.Item).GetValueTotal();
        public override float ProgressBarPercentage => this.Transaction?.IsPaid ?? false ? 1 : 0 ;
    }

    protected override InteractionContext CreateContextInternal()
        => new Context();

    public override bool CanPerform(InteractionContext ctx)
    {
        var typedCtx = (Context)ctx;
        if (typedCtx.Transaction.IsComplete)
            return false;
        return true;
    }
}
