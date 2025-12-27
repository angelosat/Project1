using Microsoft.VisualBasic;
using Start_a_Town_.Interactions;
using System;

namespace Start_a_Town_
{
    public class InteractionDef : Def
    {
        public readonly Type InteractionClass, CacheClass;
        public readonly InteractionWorker Worker;

        public InteractionDef(string name, Type interactionClass, Type workerType = null, Type cacheClass = null) : base(name)
        {
            this.InteractionClass = interactionClass;
            this.Worker = ActivatorSafe<InteractionWorker>.CreateInstance(workerType ?? typeof(InteractionWorker));
            this.CacheClass = cacheClass;
        }

        public Interaction Create(Actor actor, TargetArgs target)
        {
            var interaction = ActivatorSafe<Interaction>.CreateInstance(this.InteractionClass);
            interaction.Def = this;
            interaction.Cache = this.CreateCache(actor, target);
            return interaction;
        }

        internal InteractionCache CreateCache(Actor actor, TargetArgs target)
        {
            var cache = ActivatorSafe<InteractionCache>.CreateInstance(this.CacheClass ?? typeof(InteractionCache));
            cache.Actor = actor;
            cache.Target = target;
            return cache;
        }
    }
    public class InteractionWorker
    {
        public virtual bool CanPerform(InteractionCache ctx) { return true; }
        public virtual bool CanFinish(InteractionCache ctx) { return true; }
    }
    public class InteractionCache
    {
        public MapBase Map;
        public Actor Actor;
        public TargetArgs Target;
    }
   
}
