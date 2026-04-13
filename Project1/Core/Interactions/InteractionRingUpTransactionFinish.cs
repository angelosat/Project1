using Project1.Core.Towns.Services.Shops;
using System.Diagnostics;

namespace Project1.Core.Interactions;

sealed class InteractionRingUpTransactionFinish : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ServiceRequest_Shop Transaction => field ??= this.Actor.Map.Town.Shops.GetTransactionBySeller(this.Actor);
    }
    protected override InteractionContext CreateContextInt() => new Context();
    //public override bool CanPerform(InteractionContext ctx) => !((Context)ctx).Transaction.IsFailed;
    //public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
    internal override void OnStart(Interaction i)
    {
        Debug.Assert(i.Actor.CurrentPlan.ServiceRequest != null);
    }
    internal override void OnFinish(Interaction i)
    {
        var ctx = i.Context;
        var typedCtx = (Context)ctx;
        var actor = ctx.Actor;
        if (actor.Net.IsClient)
            return;

        var carried = actor.Hauled;
        var req = i.Actor.CurrentPlan.ServiceRequest;
        Debug.Assert(carried.RefId == req.Item);
        carried.SetOwnerNew(req.Customer);
        Debug.Assert(ctx.Target.Entity.RefId == req.Money);
        ctx.Target.Entity.SetOwnerNew(null);

        InteractionHelpers.TrySwapHauledItem(actor, ctx.Target.Entity, ctx.Count);

        actor.Map.Town.Shops.MarkProcessed(actor);
    }
}
