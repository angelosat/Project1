using Microsoft.VisualBasic;
using Start_a_Town_.Interactions;
using System;

namespace Start_a_Town_
{
    public class InteractionDef : Def
    {
        public readonly Type InteractionClass;
        public readonly InteractionLogic Logic;

        public InteractionDef(string name, Type interactionClass, Type workerType = null) : base(name)
        {
            this.InteractionClass = interactionClass;
            this.Logic = ActivatorSafe<InteractionLogic>.CreateInstance(workerType ?? typeof(InteractionLogic));
        }

        public Interaction Create(Actor actor, TargetArgs target)
        {
            var interaction = ActivatorSafe<Interaction>.CreateInstance(this.InteractionClass);
            interaction.Def = this;
            interaction.Context = this.CreateContext(actor, target);
            return interaction;
        }

        internal InteractionContext CreateContext(Actor actor, TargetArgs target)
        {
            return this.Logic.CreateContext(actor, target);
        }
    }
    public class InteractionLogic
    {
        public virtual bool CanPerform(InteractionContext ctx) { return true; }
        public virtual bool CanFinish(InteractionContext ctx) { return true; }
        public virtual bool WillFinish(InteractionContext ctx, int workAmount) { return true; }
        public virtual void ApplyWork(InteractionContext ctx, int workAmount) { }
        protected virtual InteractionContext CreateContextInternal() => new();
        internal InteractionContext CreateContext(Actor actor, TargetArgs target)
        {
            var ctx = this.CreateContextInternal();
            ctx.Actor = actor;
            ctx.Target = target;
            return ctx;
        }
    }
    public class InteractionContext
    {
        public MapBase Map;
        public Actor Actor;
        public TargetArgs Target;
        public virtual float ProgressPercentage { get; }

    }
}
