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
    int NextInstanceId => ++field;
    public ChangeNotifier Notifier = new();

    public event Action<(IEnumerable<EntityEffectRuntime> added, IEnumerable<EntityEffectRuntime> removed)> Changed;

    public override EntityCompDef CompDef => EntityCompDefOf.Effects;

    public new class Spec : Spec<EffectsComp> { }

    public EntityEffectRuntime GetEffect(EffectDef def) => this.ActiveEffects.First(e => e.Def == def);

    public EntityEffectRuntime? GetEffect(EffectDef def, Def target) => this.ActiveEffects.FirstOrDefault(e => e.Def == def && e.Target == target);

    public override string Name => "Effects";

    List<EntityEffectRuntime> ActiveEffects = [];

    public IReadOnlyList<EntityEffectRuntime> Active => this.ActiveEffects;

    public void Apply(EntityEffectRuntime runtime)
    {
        if (this.Any(runtime.Def, runtime.Target))
        {
            //throw new InvalidOperationException(
            $"Tried to apply duplicate effect {runtime.Def}:{runtime.Target}"
                .ToConsole();
            //);
            return;
        }
        runtime.Start(this.Owner as Actor);
        if (!runtime.IsInstant)
        //AddInt(effect);
        {
            this.ActiveEffects.Add(runtime);
            this.Changed?.Invoke(([runtime], []));
        }
        else
            runtime.Finish(this.Owner as Actor);
        //this.Owner.World.Events.Post(new ActorEffectAppliedEvent(this.Owner as Actor, effect));
    }

    internal void RemoveInternal(EffectDef f)
    {
        var wrapper = this.ActiveEffects.First(w => w.Def == f); // HACK
        this.ActiveEffects.Remove(wrapper);
        this.Changed?.Invoke(([], [wrapper]));
    }
    
    internal void Abort(EffectDef effect, Def target)
    {
        var relevantEffects = this.ActiveEffects
            .Where(f => f.Def == effect && f.Target == target);

        foreach (var f in relevantEffects)
        {
            if (f.IsFinished || f.IsExpired)
                continue;

            f.Abort();
            f.Finish(this.Owner as Actor);

            this.Owner.Map.World.Events.Post(
                new ActorEffectAbortedEvent(this.Owner as Actor, f));
        }
    }

    readonly List<EntityEffectRuntime> toRemove = [];

    public override void Tick()
    {
        var actor = this.Owner as Actor;
        foreach (var w in this.ActiveEffects)
        {
            if (!w.IsFinished)
                w.Tick(actor);

            if (w.IsExpired || w.IsFinished)
            {
                w.Finish(actor);
                toRemove.Add(w);
            }
        }
        foreach (var f in toRemove)
            if(actor.Net.IsServer)
            this.Owner.World.Events.Post(new ActorEffectRemovedIndexedEvent((StaticWorld)this.Owner.World, this.Owner.RefId, this.ActiveEffects.IndexOf(f)));
        toRemove.Clear();
    }

    public override void TickOffMap()
    {
        this.Tick();
    }

    public override void Write(IDataWriter w)
    {
        IOHelper.Write(w, this.ActiveEffects);
    }

    public override void Read(IDataReader r)
    {
        this.ActiveEffects = r.ReadList<EntityEffectRuntime>();
    }

    internal override void SaveExtra(SaveTag tag)
    {
        tag.Save("ActiveEffects", this.ActiveEffects);
    }

    internal override void LoadExtra(SaveTag tag)
    {
        this.ActiveEffects = tag.LoadList<EntityEffectRuntime>("ActiveEffects");
    }

    internal override GroupBox GetDetailedGui()
        => base.GetDetailedGui();

    //public override IEnumerable<(StatDef target, float value)> GetModifiers()
    //    => this.ActiveEffects.SelectMany(e => e.GetStatModifiers((Actor)this.Owner));

    public float GetStatMod(StatDef target)
        => this.ActiveEffects.SelectMany(e => e.GetStatModifiers())
                             .Where(b => b.statDef == target)
                             .Sum(b => b.value);

    internal bool Any(EffectDef effect, Def target)
        => this.ActiveEffects.Any(f => f.Def == effect && f.Target == target);

    internal bool Any(EffectDef effect)
        => this.ActiveEffects.Any(f => f.Def == effect);

    internal void RemoveAt(int effectIndex)
    {
        var f = this.ActiveEffects[effectIndex];
        this.ActiveEffects.RemoveAt(effectIndex);
        this.Changed?.Invoke(([], [f]));
    }
}
