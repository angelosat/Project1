using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using System.Collections.Generic;

namespace Project1.Core.Towns.Inns
{
    sealed class InteractionRegisterInnGuest : InteractionLogic
    {
        sealed class Context : InteractionContext
        {
            internal InnManager Manager => field ??= this.Actor.Map.Town.InnManager;
            internal Queue<Actor> Queue => field ??= this.Manager.GetQueue(this.Target.Global);
            internal Actor NextGuest => field ??= this.Queue.Peek();
        }
        protected override InteractionContext CreateContextInternal()
            => new Context();
        internal override void OnStart(Interaction i)
            => i.Progress.SetMax(Ticks.FromMinutes(10));
        
        internal override void OnSuccess(Interaction i)
        {
            var typedCtx = (Context)i.Context;
            var manager = typedCtx.Manager;
            var transaction = typedCtx.Manager.GetTransactionByGuest(typedCtx.NextGuest);
            //var money = i.Actor.Map.World.Get<Entity>(transaction.Money);
            //if (money.Cell != transaction.Desk.Above)
            //    return;
            manager.RegisterGuest(i.Target.Global);
        }
    }
}
