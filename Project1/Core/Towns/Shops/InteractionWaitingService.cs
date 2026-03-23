using Project1.Core.Interactions;

namespace Project1.Core.Towns.Shops
{
    internal sealed class InteractionWaitForPayment : InteractionLogic
    {
        sealed class Context : InteractionContext
        {
            internal ShopTransaction Transaction => field ??= this.Actor.Map.Town.ShopManager.GetTransactionBySeller(this.Actor);
            internal int? Price => field ??= this.Actor.World.GetEntity(this.Transaction.Item).GetValueTotal();
            //public override float ProgressPercentage => 0;
            public override float ProgressPercentage => this.Transaction?.IsPaid ?? false ? 1 : 0 ;
        }

        protected override InteractionContext CreateContextInternal()
            => new Context();

        public override bool CanPerform(InteractionContext ctx)
        {
            var typedCtx = (Context)ctx;
            if (typedCtx.Transaction.IsComplete)
                return false;
            //var moneyCell = ctx.Target.Global;
            //var price = typedCtx.Price;
            //var counterEntities = ctx.Actor.Map.GetEntitiesAtNew(moneyCell);
            ////int price = ctx.Actor.World.GetEntity(transaction.Item).GetValueTotal();
            //if (counterEntities.FirstOrDefault(e => e.Def == ItemDefOf.Coins) is not Entity coins)
            //    return false;
            //if (coins.StackSize < price)
            //    return false;
            return true;
        }
    }
    internal sealed class InteractionWaitingService : InteractionLogic
    {
        sealed class Context : InteractionContext
        {
            internal ShopTransaction Transaction => field ??= this.Actor.Map.Town.ShopManager.GetTransaction(this.Actor);

            public override float ProgressPercentage => this.Transaction?.WaitingForPayment ?? false ? 1 : 0;
            //public override float ProgressPercentage => 0;
        }

        protected override InteractionContext CreateContextInternal()
            => new Context();

        public override bool CanPerform(InteractionContext ctx)
        {
            var typedCtx = (Context)ctx;
            typedCtx.Transaction.Tick();
            if (typedCtx.Transaction.IsCancelled)
                return false;
            if (typedCtx.Transaction.WaitingForPayment)
                return false;
            return true;
        }
    }
}
