using Start_a_Town_;

namespace Project1.Framework.Effects
{
    public abstract class EntityEffectWorker
    {
        public abstract void OnStart(Actor actor, EntityEffectWrapper wrapper);
        public virtual void Tick(Actor actor, EntityEffectWrapper wrapper) { }
        public virtual void OnFinish(Actor actor, EntityEffectWrapper wrapper) { }
    }
}
