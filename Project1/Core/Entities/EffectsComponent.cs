using Project1.Core.Effects;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Entities;

public sealed class EffectsComponent : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Effects;
    public new class Spec : Spec<EffectsComponent> { }
    public EntityEffectWrapper GetEffect(EffectDef def) => this.ActiveEffects.First(e => e.Def == def);
    //public EntityEffectWrapper GetEffect(EffectDef def, Def target) => this.ActiveEffects.First(e => e.Def == def && e.Target == target);
    public EntityEffectWrapper? GetEffect(EffectDef def, Def target) => this.ActiveEffects.FirstOrDefault(e => e.Def == def && e.Target == target);
    public override string Name => "Effects";

    List<EntityEffectWrapper> ActiveEffects = [];
    public void Apply(EntityEffectWrapper effect)
    {
        effect.Start(this.Owner as Actor);
        if (!effect.IsInstant)
        {
            this.ActiveEffects.Add(effect);
            this.Owner.World.Events.Post(new ActorEffectAppliedEvent(this.Owner as Actor, effect));
        }
        else
            effect.Finish(this.Owner as Actor);

    }
    [Obsolete("add EntityEffectWrapper instead")]
    public void Apply(EffectDef effectt)
    {
        var wrapper = new EntityEffectWrapper(effectt, null, 1, 0);
        this.Apply(wrapper);
    }

    internal void Remove(EffectDef effect)
    {
        var relevantEffects = this.ActiveEffects.Where(f => f.Def == effect);
        foreach (var f in relevantEffects)
        {
            f.Finish(this.Owner as Actor);
            this.ActiveEffects.Remove(f);
        }
    }
    internal void Abort(EffectDef effect, Def target)
    {
        var relevantEffects = this.ActiveEffects.Where(f => f.Def == effect && f.Target == target);
        foreach (var f in relevantEffects)
        {
            f.Abort();
            this.Owner.Map.World.Events.Post(new ActorEffectAbortedEvent(this.Owner as Actor, f));
        }
    }
    List<EntityEffectWrapper> toRemove = [];
    public override void Tick()
    {
        var actor = this.Owner as Actor;
        foreach (var w in this.ActiveEffects)
        {
            w.Tick(actor);
            if (w.IsFinished)
            {
                w.Finish(actor);
                toRemove.Add(w);
            }
        }
        this.ActiveEffects.RemoveAll(w => w.IsFinished);
        toRemove.Clear();
    }
    public override void Write(IDataWriter w)
    {
        IOHelper.Write(w, this.ActiveEffects);
    }
    public override void Read(IDataReader r)
    {
        this.ActiveEffects = r.ReadList<EntityEffectWrapper>();
    }
    internal override void SaveExtra(SaveTag tag)
    {
        tag.Save("ActiveEffects", this.ActiveEffects);
    }
    internal override void LoadExtra(SaveTag tag)
    {
        this.ActiveEffects = tag.LoadList<EntityEffectWrapper>("ActiveEffects");
    }
    internal override GroupBox GetDetailedGui()
    {
        return base.GetDetailedGui();
    }
}
