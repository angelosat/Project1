using Project1.Core.Interactions;

namespace Project1.Core.Towns.Shops
{
    internal sealed class InteractionWaitingService : InteractionLogic
    {
        sealed class Context : InteractionContext
        {
            internal ShopTransaction Transaction => field ??= this.Actor.Map.Town.ShopManager.GetTransaction(this.Actor);

            public override float ProgressPercentage => 0;
        }

        protected override InteractionContext CreateContextInternal()
            => new Context();

        public override bool CanPerform(InteractionContext ctx)
        {
            var typedCtx = (Context)ctx;
            typedCtx.Transaction.Tick();
            if (typedCtx.Transaction.IsCancelled)
                return false;
            return true;
        }
    }
}
