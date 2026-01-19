namespace Start_a_Town_
{
    public abstract class EntityEffectWorker
    {
        public abstract void OnStart(Actor actor, EntityEffectWrapper wrapper);
        public virtual void Tick(Actor actor, EntityEffectWrapper wrapper) { }
        public virtual void OnFinish(Actor actor, EntityEffectWrapper wrapper) { }
    }
}
