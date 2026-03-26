using Project1.Core.Entities.Actors;
using System;

namespace Project1.Core.Interactions
{
    public class InteractionLogic
    {
        public virtual bool CanPerform(InteractionContext ctx) { return true; }
        public virtual bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
        public virtual bool WillFinish(InteractionContext ctx, int workAmount) { return true; }
        public virtual void ApplyWork(InteractionContext ctx, int workAmount) { }
        protected virtual InteractionContext CreateContextInternal() => new();
        internal InteractionContext CreateContext(Actor actor, TargetArgs target, int count)
        {
            var ctx = this.CreateContextInternal();
            ctx.Actor = actor;
            ctx.Target = target;
            ctx.Count = count;
            return ctx;
        }
        internal virtual void OnStart(Interaction i) { }
        internal virtual void OnTick(Interaction i) { }
        internal virtual void OnFinish(Interaction i) { }
        internal virtual bool IsFinished(Interaction i) => false;
        internal virtual int CalculateMax(InteractionContext ctx) => 100;
        internal virtual void OnProgressAdded(Interaction i, int delta) { }

    }
}