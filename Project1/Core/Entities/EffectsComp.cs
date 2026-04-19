using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Effects;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Entities;

sealed class Gui_Effects : SelectionBoundControl
{
    readonly Table<EntityEffectWrapper> Table;
    EffectsComp Comp;
    public Gui_Effects()
    {
        this.Table = new Table<EntityEffectWrapper>()
            .AddColumn("name", 128, e => new LabelNew(e.Def.LabelReadable))
            .AddColumn("remaining", 128, e => new Label(() => e.RemainingTimespan(this.Comp.Owner.World.CurrentTick).ToString()));
        var scrollbox = ScrollableBoxNewNewNew.FromWidth(this.Table, this.Table.RowWidth, Label.DefaultHeight * 16);
        this.Controls.Add(scrollbox.ToPanelLabeled("Active Effects"));
        //this.AddControls(this.Table);
    }
    protected internal override void OnBind(ISelectable selectable)
    {
        if (selectable is not Actor actor)
            return;

        this.Comp?.Changed -= Comp_Changed;
        this.Comp = actor.Effects;
        this.Comp.Changed += Comp_Changed;
        this.Table.ClearControls();
        this.Table.AddItems(actor.Effects.Active);
    }

    private void Comp_Changed((IEnumerable<EntityEffectWrapper> added, IEnumerable<EntityEffectWrapper> removed) e)
    {
        this.Table.RemoveItems(e.removed);
        this.Table.AddItems(e.added);
    }
}

public sealed class EffectsComp : EntityComp
{
    public ChangeNotifier Notifier = new();
    public event Action<(IEnumerable<EntityEffectWrapper> added, IEnumerable<EntityEffectWrapper> removed)> Changed;
    public override EntityCompDef CompDef => EntityCompDefOf.Effects;
    public new class Spec : Spec<EffectsComp> { }
    public EntityEffectWrapper GetEffect(EffectDef def) => this.ActiveEffects.First(e => e.Def == def);
    //public EntityEffectWrapper GetEffect(EffectDef def, Def target) => this.ActiveEffects.First(e => e.Def == def && e.Target == target);
    public EntityEffectWrapper? GetEffect(EffectDef def, Def target) => this.ActiveEffects.FirstOrDefault(e => e.Def == def && e.Target == target);
    public override string Name => "Effects";

    List<EntityEffectWrapper> ActiveEffects = [];
    public IReadOnlyList<EntityEffectWrapper> Active => this.ActiveEffects;
    public void Apply(EntityEffectWrapper effect)
    {
        effect.Start(this.Owner as Actor);
        if (!effect.IsInstant)
        {
            AddInt(effect);
            this.Owner.World.Events.Post(new ActorEffectAppliedEvent(this.Owner as Actor, effect));
        }
        else
            effect.Finish(this.Owner as Actor);

    }

    private void AddInt(EntityEffectWrapper f)
    {
        this.ActiveEffects.Add(f);
        this.Changed?.Invoke(([f], []));
    }
    private void RemoveInt(EntityEffectWrapper f)
    {
        this.ActiveEffects.Remove(f);
        this.Changed?.Invoke(([], [f]));
    }
    //[Obsolete("add EntityEffectWrapper instead")]
    //public void Apply(EffectDef effectt)
    //{
    //    var wrapper = new EntityEffectWrapper(effectt, null, 1, 0);
    //    this.Apply(wrapper);
    //}

    internal void Remove(EffectDef effect)
    {
        var relevantEffects = this.ActiveEffects.Where(f => f.Def == effect);
        foreach (var f in relevantEffects)
        {
            f.Finish(this.Owner as Actor);
            RemoveInt(f);
        }
    }

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
        //this.ActiveEffects.RemoveAll(w => w.IsFinished);
        foreach (var f in toRemove)
            this.RemoveInt(f);
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

    internal bool Any(EffectDef effect, Def target)
        => this.ActiveEffects.Any(f => f.Def == effect && f.Target == target);    
}
