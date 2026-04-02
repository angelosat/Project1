using Project1.Core.Entities;
using Project1.Core.Interactions;

namespace Project1.Core.Towns.Inns
{
    sealed class InteractionWaitPayForBed : InteractionLogic
    {
        protected override InteractionContext CreateContextInt()
            => new InnTransactionContext();
        internal override void OnStart(Interaction i)
        {
            var typedCtx = (InnTransactionContext)i.Context;
            typedCtx.Manager.AssignClerk(i.Target.Global, i.Actor);
        }
        internal override bool HasSucceeded(Interaction i)
        {
            var typedCtx = (InnTransactionContext)i.Context;
            var transaction = typedCtx.Transaction;
            if (transaction.Money == EntityRefId.Null)
                return false;
            var money = i.Actor.Map.World.Get<Entity>(transaction.Money);
            if (money.Cell != transaction.Desk.Above)
                return false;
            return true;
        }
        internal override void OnTick(Interaction i)
        {
            
        }
    }
}
