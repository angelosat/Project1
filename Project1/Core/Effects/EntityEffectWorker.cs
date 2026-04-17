using Project1.Core.Entities.Actors;

namespace Project1.Core.Effects;

//public sealed class EntityEffectController_Duration : EntityEffectController
//{
//    internal override void Tick(Actor actor, EntityEffectWrapper runtime)
//    {
//        runtime.RemainingBudget = runtime.RemainingBudget.Value - 1;
//    }
//}
//public abstract class EntityEffectController
//{
//    internal abstract void Tick(Actor actor, EntityEffectWrapper runtime); 
//}

public abstract class EntityEffectWorker
{
    public abstract EffectDef Def { get; }
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

    internal string Label(Def target) => $"{this.Def.Verb} {target.LabelReadable}";
}
