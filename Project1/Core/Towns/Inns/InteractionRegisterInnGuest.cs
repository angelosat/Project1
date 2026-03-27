using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using Project1.Core.Resources;
using System.Collections.Generic;

namespace Project1.Core.Towns.Inns
{
    //sealed class InteractionRegisterInnGuest : InteractionLogic
    //{
    //    sealed class Context : InteractionContext
    //    {
    //        Queue<Actor> Queue => field ??= this.Actor.Map.Town.InnManager.GetQueue(this.Target.Global);
    //        Actor NextGuest => field ??= this.Queue.Peek();
    //    }
    //    internal override void OnFinish(Interaction i)
    //    {
    //        var manager = i.Actor.Map.Town.InnManager;
    //        manager.RegisterGuest(i.Target.Global);
    //    }
    //}
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
            //var transaction = typedCtx.Transaction;
            //transaction.AssignClerk(i.Actor);
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
        //=> i.Progress.SetMax(Ticks.FromMinutes(10));
        {
            i.Progress.SetMax(Ticks.FromMinutes(10));
            //var typedCtx = (Context)i.Context;
            //var transaction = typedCtx.Manager.GetTransactionByGuest(typedCtx.NextGuest);
            //transaction.AssignClerk(i.Actor);
        }
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
        //internal override void OnTick(Interaction i)
        //{
        //    var typedCtx = (Context)i.Context;
        //    var manager = typedCtx.Manager;
        //    if (i.Progress.Percentage >= 1)
        //    {
        //        manager.RegisterGuest(i.Target.Global);
        //        i.MarkSucceeded();
        //        return;
        //    }
        //    var guest = typedCtx.NextGuest;
        //    if (!manager.IsQueuing(guest))
        //    {
        //        i.MarkFailed();
        //        return;
        //    }
        //    if (typedCtx.Queue.Peek() != guest)
        //    {
        //        i.MarkFailed();
        //        return;
        //    }
        //}
        
        //internal override bool IsFinished(Interaction i)
        //{
        //    var typedCtx = (Context)i.Context;
        //    var manager = typedCtx.Manager;
        //    if (i.Progress.Percentage >= 1)
        //    {
        //        manager.RegisterGuest(i.Target.Global);
        //        return true;
        //    }
        //    var guest = typedCtx.NextGuest;
        //    if (!manager.IsQueuing(guest))
        //        return true;
        //    if (typedCtx.Queue.Peek() != guest)
        //        return true;
        //    return false;
        //}
    }
    sealed class InteractionCheckIn : InteractionLogic
    {
        sealed class Context : InteractionContext
        {
            public override float ProgressBarPercentage => this.Actor.HasCheckedIn ? 1 : 0;
        }
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

        //internal override void OnTick(Interaction i)
        //{
        //    var ctx = i.Context;
        //    var actor = ctx.Actor;
        //    var comp = actor.Resources;
        //    if (actor.HasCheckedIn)
        //    {
        //        actor.AI.State.Log.Write($"I have checked in successfully");
        //        i.MarkSucceeded();
        //        return;
        //    }
        //    if (comp.GetValue(ResourceDefOf.Patience) <= 0)
        //    {
        //        actor.Map.Town.InnManager.AbortQueuing(actor);
        //        i.MarkFailed();
        //        return;
        //    }
        //    comp.ApplyDelta(ResourceDefOf.Patience, -.01f);
        //}
        //internal override void OnFinish(Interaction i)
        //{
        //    i.Actor.Map.Town.InnManager.AbortQueuing(i.Actor);
        //}



        //internal override bool IsFinished(Interaction i)
        //{
        //    var ctx = i.Context;
        //    var actor = ctx.Actor;
        //    var comp = actor.Resources;
        //    if (actor.HasCheckedIn)
        //    {
        //        actor.AI.State.Log.Write($"I have checked in successfully");
        //        return true;
        //    }
        //    if (comp.GetValue(ResourceDefOf.Patience) == 0)
        //    {
        //        actor.Map.Town.InnManager.AbortQueuing(actor);
        //        return true;
        //    }
        //    comp.ApplyDelta(ResourceDefOf.Patience, -.01f);
        //    return false;
        //}
    }
}
