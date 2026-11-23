using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Simulation;
using Project1.Core.Systems.Effects;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Entities;

public sealed class EffectsComp : EntityComp
{
    public ChangeNotifier Notifier = new();

    public event Action<(IEnumerable<EntityEffectWrapper> added, IEnumerable<EntityEffectWrapper> removed)> Changed;

    public override EntityCompDef CompDef => EntityCompDefOf.Effects;

    public new class Spec : Spec<EffectsComp> { }

    public EntityEffectWrapper GetEffect(EffectDef def) => this.ActiveEffects.First(e => e.Def == def);

    public EntityEffectWrapper? GetEffect(EffectDef def, Def target) => this.ActiveEffects.FirstOrDefault(e => e.Def == def && e.Target == target);

    public override string Name => "Effects";

    List<EntityEffectWrapper> ActiveEffects = [];

    public IReadOnlyList<EntityEffectWrapper> Active => this.ActiveEffects;

    public void Apply(EntityEffectWrapper effect)
    {
        effect.Start(this.Owner as Actor);
        if (!effect.IsInstant)
            AddInt(effect);
        else
            effect.Finish(this.Owner as Actor);
        this.Owner.World.Events.Post(new ActorEffectAppliedEvent(this.Owner as Actor, effect));
    }

    private void AddInt(EntityEffectWrapper f)
    {
        this.ActiveEffects.Add(f);
        this.Changed?.Invoke(([f], []));
    }

    internal void RemoveInternal(EffectDef f)
    {
        var wrapper = this.ActiveEffects.First(w => w.Def == f); // HACK
        this.ActiveEffects.Remove(wrapper);
        this.Changed?.Invoke(([], [wrapper]));
    }
    
    //internal void Remove(EffectDef effect)
    //{
    //    var relevantEffects = this.ActiveEffects.Where(f => f.Def == effect);
    //    foreach (var f in relevantEffects)
    //    {
    //        f.Finish(this.Owner as Actor);
    //        RemoveInternal(f.Def);
    //    }
    //}

    public bool TryGet(EffectDef def, Def target, out ulong ticksRemaining)
    {
        if (this.ActiveEffects.FirstOrDefault(f => f.Def == def && f.Target == target) is not EntityEffectWrapper runtime)
        {
            ticksRemaining = default;
            return false;
        }
        ticksRemaining = runtime.RemainingDuration(this.Owner.World.CurrentTick);
        return true;
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

    readonly List<EntityEffectWrapper> toRemove = [];

    public override void Tick()
    {
        var actor = this.Owner as Actor;
        foreach (var w in this.ActiveEffects)
        {
            w.Tick(actor);
            if (w.IsExpired || w.IsFinished)
            {
                w.Finish(actor);
                toRemove.Add(w);
            }
        }
        foreach (var f in toRemove)
            this.Owner.World.Events.Post(new ActorEffectRemovedEvent((StaticWorld)this.Owner.World, this.Owner.RefId, f.Def));
            //this.RemoveInternal(f);
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
        => base.GetDetailedGui();

    //public override IEnumerable<(StatDef target, float value)> GetModifiers()
    //    => this.ActiveEffects.SelectMany(e => e.GetStatModifiers((Actor)this.Owner));

    public float GetStatMod(StatDef target)
        => this.ActiveEffects.SelectMany(e => e.GetStatModifiers((Actor)this.Owner))
                             .Where(b => b.statDef == target)
                             .Sum(b => b.value);

    internal bool Any(EffectDef effect, Def target)
        => this.ActiveEffects.Any(f => f.Def == effect && f.Target == target);

    internal bool Any(EffectDef effect)
        => this.ActiveEffects.Any(f => f.Def == effect);
}
