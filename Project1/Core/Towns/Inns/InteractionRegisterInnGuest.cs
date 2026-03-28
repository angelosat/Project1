using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using Project1.Core.Resources;
using System.Collections.Generic;

namespace Project1.Core.Towns.Inns
{
    sealed class InnTransactionContext : InteractionContext
    {
        internal InnManager Manager => field ??= this.Actor.Map.Town.InnManager;
        internal Queue<Actor> Queue => field ??= this.Manager.GetQueue(this.Target.Global);
        internal Actor NextGuest => field ??= this.Queue.Peek();
        internal InnTransaction Transaction => field ??= this.Manager.GetTransactionByGuest(this.NextGuest);
    }
    sealed class InteractionWaitPayForBed : InteractionLogic
    {
        protected override InteractionContext CreateContextInternal()
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
    sealed class InteractionPayForBed : InteractionLogic
    {
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Actor;
            var global = i.Target.Global;
            var count = i.Context.Count;
            var hauled = actor.Hauled;
            InteractionHelpers.TryDepositCarriedItemInsideBlockOrSpawn(actor, global, count);
            actor.Map.Town.InnManager.GetTransactionByGuest(actor).MarkPaid(hauled);
        }
    }
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
            var money = i.Actor.Map.World.Get<Entity>(transaction.Money);
            if (money.Cell != transaction.Desk.Above)
                return;
            manager.RegisterGuest(i.Target.Global);
        }
    }
    sealed class InteractionCheckIn : InteractionLogic
    {
        sealed class Context : InteractionContext
        {
            internal IResourceView Patience => field ??= this.Actor.Resources.View(ResourceDefOf.Patience);
            internal override float GetPercentage(Interaction i) => ((Context)i.Context).Patience.Percentage;

        }

        protected override InteractionContext CreateContextInternal()
            => new Context();

        internal override void OnStart(Interaction i)
            => i.Actor.Map.Town.InnManager.TryEnqueue(i.Actor, i.Target.Global);

        internal override bool HasSucceeded(Interaction i)
            => i.Actor.HasCheckedIn || i.Actor.Map.Town.InnManager.GetTransactionByGuest(i.Actor).IsAwaitingPayment;
        internal override void OnSuccess(Interaction i)
            => i.Actor.AI.State.Log.Write($"I have checked in successfully");

        internal override bool HasFailed(Interaction i)
       => i.Actor.Resources.GetValue(ResourceDefOf.Patience) <= 0;
        internal override void OnFailure(Interaction i)
            => i.Actor.Map.Town.InnManager.AbortQueuing(i.Actor);

        internal override void OnTick(Interaction i)
            => i.Actor.Resources.ApplyDelta(ResourceDefOf.Patience, -.01f);
    }
}
