using System;

namespace Start_a_Town_
{
    public abstract class EntityEffectWorker
    {
        public abstract void OnStart(Actor actor);
        public abstract void OnFinish(Actor actor);
    }
}
