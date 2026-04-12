using Project1.Core.Towns.Services.Shops;

namespace Project1.Core.Interactions;

sealed class InteractionRingUpTransactionFinish : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ServiceRequest_Shop Transaction => field ??= this.Actor.Map.Town.Shops.GetTransactionBySeller(this.Actor);
    }
    protected override InteractionContext CreateContextInt() => new Context();
    public override bool CanPerform(InteractionContext ctx) => !((Context)ctx).Transaction.IsFailed;
    public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
    internal override void OnFinish(Interaction i)
    {
        var ctx = i.Context;
        var typedCtx = (Context)ctx;
        var actor = ctx.Actor;
        if (actor.Net.IsClient)
            return;
        InteractionHelpers.TrySwapHauledItem(actor, ctx.Target.Entity, ctx.Count);
        actor.Map.Town.Shops.MarkProcessed(actor);
    }
}
