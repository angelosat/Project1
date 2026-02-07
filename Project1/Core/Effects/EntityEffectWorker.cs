using Project1.Core.Entities.Actors;

namespace Project1.Core.Effects
{
    public abstract class EntityEffectWorker
    {
        public abstract void OnStart(Actor actor, EntityEffectWrapper wrapper);
        public virtual void Tick(Actor actor, EntityEffectWrapper wrapper) { }
        public virtual void OnFinish(Actor actor, EntityEffectWrapper wrapper) { }
    }
}
