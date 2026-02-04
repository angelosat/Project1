using Project1.Framework.Entities.Actors;

namespace Project1.Framework.Effects
{
    public abstract class EntityEffectWorker
    {
        public abstract void OnStart(Actor actor, EntityEffectWrapper wrapper);
        public virtual void Tick(Actor actor, EntityEffectWrapper wrapper) { }
        public virtual void OnFinish(Actor actor, EntityEffectWrapper wrapper) { }
    }
}
