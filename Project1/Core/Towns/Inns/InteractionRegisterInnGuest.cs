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
        {
            i.Progress.SetMax(Ticks.FromMinutes(10));
        }
        internal override void OnTick(Interaction i)
        {
            i.Progress.ApplyDelta(1);
        }
        internal override bool IsFinished(Interaction i)
        {
            var typedCtx = (Context)i.Context;
            var manager = typedCtx.Manager;
            if (i.Progress.Percentage >= 1)
            {
                manager.RegisterGuest(i.Target.Global);
                return true;
            }
            var guest = typedCtx.NextGuest;
            if (!manager.IsQueuing(guest))
                return true;
            if (typedCtx.Queue.Peek() != guest)
                return true;
            return false;
        }
    }
    sealed class InteractionCheckIn : InteractionLogic
    {
        sealed class Context : InteractionContext
        {
            public override float ProgressBarPercentage => this.Actor.HasCheckedIn ? 1 : 0;
        }
        internal override void OnStart(Interaction i)
        {
            i.Actor.Map.Town.InnManager.TryEnqueue(i.Actor, i.Target.Global);
        }
        
        internal override bool IsFinished(Interaction i)
        {
            var ctx = i.Context;
            var actor = ctx.Actor;
            var comp = actor.Resources;
            if (actor.HasCheckedIn)
            {
                actor.AI.State.Log.Write($"I have checked in successfully");
                return true;
            }
            if (comp.GetValue(ResourceDefOf.Patience) == 0)
            {
                actor.Map.Town.InnManager.AbortQueuing(actor);
                return true;
            }
            comp.ApplyDelta(ResourceDefOf.Patience, -.01f);
            return false;
        }
    }
}
