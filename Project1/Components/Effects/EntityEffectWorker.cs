using System;

namespace Start_a_Town_
{
    public abstract class EntityEffectWorker
    {
        public abstract void OnStart(Actor actor, EntityEffectWrapper wrapper);
        public virtual void Tick(Actor actor, EntityEffectWrapper wrapper) { }
        public abstract void OnFinish(Actor actor, EntityEffectWrapper wrapper);
    }
}
