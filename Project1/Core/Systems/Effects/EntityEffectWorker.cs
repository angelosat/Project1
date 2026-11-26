using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Systems.Effects;

public abstract class EntityEffectWorker
{
    public abstract EffectDef Def { get; }

    internal virtual EntityEffectRuntime CreateRuntime(Def? context, int magnitude)
        => new(this.Def, context, magnitude, 0);

    protected abstract void OnStart(Actor actor, EntityEffectRuntime runtime);

    protected virtual void OnTick(Actor actor, EntityEffectRuntime runtime) { }

    protected virtual void OnFinish(Actor actor, EntityEffectRuntime runtime) { }

    internal void Start(Actor actor, EntityEffectRuntime runtime)
    {
        if (actor.Net.IsClient)
            return;
        this.OnStart(actor, runtime);
    }

    internal void Finish(Actor actor, EntityEffectRuntime runtime)
    {
        if (actor.Net.IsClient)
            return;
        this.OnFinish(actor, runtime);
    }

    internal void Tick(Actor actor, EntityEffectRuntime runtime)
    {
        if (actor.Net.IsClient)
            return;
        this.OnTick(actor, runtime);
    }

    internal virtual string Label(Def? context) => $"{this.Def.Verb} {context?.LabelReadable}";
    internal virtual string GetString(Def? context, int magnitude) 
        => $"{this.Def.Verb} [{magnitude}] {context?.LabelReadable}";

    internal virtual Color GetTint(Def context) => Color.White;

    internal virtual IEnumerable<(StatDef statDef, float value)> GetStatModifiers(Actor actor)
        => [];
}
