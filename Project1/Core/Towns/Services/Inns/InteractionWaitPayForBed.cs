using Project1.Core.Entities;
using Project1.Core.Interactions;

namespace Project1.Core.Towns.Services.Inns;

sealed class InteractionWaitPayForBed : InteractionLogic
{
    protected override InteractionContext CreateContextInt()
        => new InteractionContext_Inns();
    internal override void OnStart(Interaction i)
    {
        var typedCtx = (InteractionContext_Inns)i.Context;
        typedCtx.Transaction.MarkVendorWaitingPayment();
    }
    internal override bool HasSucceeded(Interaction i)
    {
        var typedCtx = (InteractionContext_Inns)i.Context;
        var transaction = typedCtx.Transaction;
        if (transaction.Money == EntityRefId.Null)
            return false;
        var money = i.Actor.Map.World.Get<Entity>(transaction.Money);
        if (money.Cell != transaction.Counter.Value.Above)
            return false;
        return true;
    }
    internal override void OnTick(Interaction i)
    {
        
    }
}
