using Project1.Core.Towns.Services.Shops;

#nullable enable

namespace Project1.Core.Interactions;

sealed class InteractionRingUp : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ServiceRequest_Shop Transaction => field ??= this.Actor.Map.Town.Shops.GetTransactionBySeller(this.Actor);
        //internal int? Price => field ??= this.Actor.World.Get(this.Transaction.Item).GetValueTotal();
        internal int? Price => field ??= this.Actor.Map.Town.Shops.GetPrice(this.Transaction.Item)!.Value;
        public override float ProgressBarPercentage => 0;
    }
    protected override InteractionContext CreateContextInt() => new Context();
    public override bool CanPerform(InteractionContext ctx) => !((Context)ctx).Transaction.IsFailed;
    public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
    internal override void OnFinish(Interaction i)
    {
        var ctx = i.Context;
        var actor = ctx.Actor;
        if (actor.Net.IsClient)
            return;
        var global = ctx.Target.Global;
        var count = ctx.Count;
        var typedCtx = (Context)ctx;
        var item = ctx.Target.Entity;
        actor.Inventory.HaulNew(item, item.StackSize);
        actor.Map.Town.Shops.RingUp(actor, item);
    }
}
