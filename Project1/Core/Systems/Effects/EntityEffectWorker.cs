using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Systems.Effects;

public abstract class EntityEffectWorker
{
    //internal Actor Actor;
    //internal abstract void SetDto(EffectData dto);
    internal virtual void Start(Actor actor, EffectData dto) { }
    internal abstract void Perform(Actor actor, EffectData dto);
    internal virtual void Finish(Actor actor, EffectData dto) { }
    internal virtual float Score(Actor actor, EffectConfig config) => 0f;
    internal virtual IEnumerable<(StatDef statDef, float value)> GetStatModifiers() => [];
    internal abstract string GetString(EffectData dto);
}

public abstract class EntityEffectWorker<TConfig, TData> : EntityEffectWorker 
    where TData : EffectData<TConfig> 
    where TConfig : EffectConfig
{
    //public T Dto { get; private set; }
    //internal override void SetDto(EffectData dto)
    //    => this.Dto = (T)dto;

    //internal virtual EntityEffectRuntime CreateRuntime(EffectDef def, Def? context, int magnitude)
    //    => new(def, context, magnitude, 0);

    protected virtual void OnStart(Actor actor, TData dto) { }
    internal override sealed void Perform(Actor actor, EffectData dto)
    {
        if (actor.Net.IsClient)
            return;
        this.OnPerform(actor, (TData)dto);
    }
    protected virtual void OnPerform(Actor actor, TData dto) { }

    protected virtual void OnTick(Actor actor, EntityEffectRuntime runtime) { }

    protected virtual void OnFinish(Actor actor, TData dto) { }

    internal override sealed void Start(Actor actor, EffectData dto)
    {
        if (actor.Net.IsClient)
            return;
        this.OnStart(actor, (TData)dto);
    }

    internal override sealed void Finish(Actor actor, EffectData dto)
    {
        if (actor.Net.IsClient)
            return;
        this.OnFinish(actor, (TData)dto);
    }

    internal override sealed float Score(Actor actor, EffectConfig dto)
        => this.Score(actor, (TConfig)dto);

    protected virtual float Score(Actor actor, TConfig dto)
        => 0f;

    internal void Tick(Actor actor, EntityEffectRuntime runtime)
    {
        if (actor.Net.IsClient)
            return;
        this.OnTick(actor, runtime);
    }

    internal virtual string Label(EffectDef def, Def? context) => $"{def.Verb} {context?.LabelReadable}";
    internal virtual string GetString(EffectDef def, Def? context, int magnitude) 
        => $"{def.Verb} [{magnitude}] {context?.LabelReadable}";
    internal virtual string Description(TData dto)
        => string.Empty;

    internal virtual Color GetTint(EntityEffectRuntime runtime) => Color.White;
    internal virtual Color Tint(TData dto) => Color.White;

    internal sealed override string GetString(EffectData dto)
        => this.GetString((TData)dto);
    protected virtual string GetString(TData dto) => string.Empty;

}
