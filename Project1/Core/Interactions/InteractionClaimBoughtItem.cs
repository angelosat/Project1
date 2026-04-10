using Project1.Core.Towns.Services.Shops;

namespace Project1.Core.Interactions;

sealed class InteractionClaimBoughtItem : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ServiceRequest_Shop Transaction => field ??= this.Actor.Map.Town.Shops.GetTransactionBySeller(this.Actor);
    }

    protected override InteractionContext CreateContextInt() => new Context();
    internal override void OnFinish(Interaction i)
    {
        var ctx = i.Context;
        var actor = ctx.Actor;
        if (actor.Net.IsClient)
            return;
        var item = ctx.Target.Entity;
        actor.Inventory.HaulNew(item, item.StackSize);
        var manager = actor.AI.State.ItemPreferences;
        //var (role, score) = manager.GetPotential(item);
        //if (role is null)
        //    throw new Exception();
        //manager.Commit(role, item, score);
        manager.TryCommit(item);
        actor.Map.Town.Shops.FinishTransaction(actor);
    }
}
