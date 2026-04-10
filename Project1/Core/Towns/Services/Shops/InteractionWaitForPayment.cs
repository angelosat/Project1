using Project1.Core.Interactions;

namespace Project1.Core.Towns.Services.Shops;

internal sealed class InteractionWaitForPayment : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ServiceRequest_Shop Transaction => field ??= this.Actor.Map.Town.Shops.GetTransactionBySeller(this.Actor);
        internal int? Price => field ??= this.Actor.World.Get(this.Transaction.Item).GetValueTotal();
        public override float ProgressBarPercentage => this.Transaction?.IsPaid ?? false ? 1 : 0 ;
    }

    protected override InteractionContext CreateContextInt()
        => new Context();

    public override bool CanPerform(InteractionContext ctx)
    {
        var typedCtx = (Context)ctx;
        if (typedCtx.Transaction.IsComplete)
            return false;
        return true;
    }
}
