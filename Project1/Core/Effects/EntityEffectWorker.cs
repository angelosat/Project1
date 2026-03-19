using Project1.Core.Entities.Actors;
using System;

namespace Project1.Core.Effects
{
    public abstract class EntityEffectWorker
    {
        protected abstract void OnStart(Actor actor, EntityEffectWrapper wrapper);
        protected virtual void OnTick(Actor actor, EntityEffectWrapper wrapper) { }
        protected virtual void OnFinish(Actor actor, EntityEffectWrapper wrapper) { }

        internal void Start(Actor actor, EntityEffectWrapper entityEffectWrapper)
        {
            if (actor.Net.IsClient)
                return;
            this.OnStart(actor, entityEffectWrapper);
        }
        internal void Finish(Actor actor, EntityEffectWrapper entityEffectWrapper)
        {
            if (actor.Net.IsClient)
                return;
            this.OnFinish(actor, entityEffectWrapper);
        }
        internal void Tick(Actor actor, EntityEffectWrapper entityEffectWrapper)
        {
            if (actor.Net.IsClient)
                return;
            this.OnTick(actor, entityEffectWrapper);
        }
    }
}
