using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using System.Collections.Generic;

namespace Project1.Core.Systems.Effects;

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

    internal virtual string Label(Def target) => $"{this.Def.Verb} {target.LabelReadable}";

    internal virtual Color GetTint(Def target) => Color.White;

    internal virtual IEnumerable<(StatDef statDef, float value)> GetStatModifiers(Actor actor,
                                                                                  EntityEffectWrapper runtime)
        => [];
}
