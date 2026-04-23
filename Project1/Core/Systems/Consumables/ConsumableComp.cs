using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Magic;
using Project1.Core.Systems.Quality;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Consumables;

public sealed class ConsumableComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Consumable;
    public override string Name { get; } = "Consumable";

    public SpellDef Spell;
    public List<EntityEffectWrapper> EffectsNew = [];
    public Tier Tier;

    public EntityEffectWrapper Effect => this.EffectsNew.FirstOrDefault();
    public bool HasEffectTarget(Def target) => this.EffectsNew.Any(f => f.Target == target);
    public override void OnTooltipCreated(Control tooltip)
    {
        foreach (var effect in this.EffectsNew)
            tooltip.Controls.Add(
                new Label(effect) { Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.ForestGreen }
                );
        if (this.Spell is SpellDef spell)
        {
            //tooltip.AddControlsBottomLeft(new LabelNew(() => $"Cast: {spell}") { TextColorFunc = () => Color.Aquamarine });
            tooltip.AddControlsBottomLeft(new LabelNew(spell) { TextColorFunc = () => Color.Aquamarine });
        }
    }
    internal override void CopyFrom(EntityComp source)
    {
        var comp = source as ConsumableComp;
        foreach (var f in comp.EffectsNew)
            this.EffectsNew.Add(new EntityEffectWrapper(f.Def,  f.Target, f.Budget, f.TicksPerUnit/*, f.Magnitude*/));
        this.Spell = comp.Spell;
    }
    internal override void Validate()
    {
        var quality = this.Owner.QualityComp.Tier;
        var mod = quality.Multiplier;
        foreach (var fx in this.EffectsNew)
            fx.Multiplier = mod;
    }

    public void Add(EntityEffectWrapper effect)
        => this.EffectsNew.Add(effect);

    internal override void ResolveReferencesNew()
    {
        var profile = (ConsumableDef)this.Owner.Profile;
        this.Owner.Name = profile.Worker.GetLabel(this);
        this.Owner.Body.Sprite = profile.Sprite;
    }

    public override void GetInventoryTooltip(Control tooltip)
    {
        this.OnTooltipCreated(tooltip);
    }

    public override void Write(IDataWriter w)
    {
        w.Write(this.EffectsNew);
        var hasspell = this.Spell is not null;
        w.Write(hasspell);
        if (hasspell)
            w.Write(this.Spell);
    }
    public override void Read(IDataReader r)
    {
        this.EffectsNew = r.ReadList<EntityEffectWrapper>();
            if (r.ReadBoolean())
            this.Spell = r.ReadDef<SpellDef>();
    }
    internal override void SaveExtra(SaveTag tag)
    {
        tag.Save("Effects", this.EffectsNew);
        if(this.Spell is not null)
        tag.Save("Spell", this.Spell);
    }
    internal override void LoadExtra(SaveTag tag)
    {
        this.EffectsNew = tag.LoadList<EntityEffectWrapper>("Effects");
        if (tag.TryLoadDef<SpellDef>("Spell", out var spell))
            this.Spell = spell;
    }

    internal void ApplyEffects(Actor actor)
    {
        foreach(var fx in this.EffectsNew)
            actor.Effects.Apply(fx.Clone());
    }

    public new class Spec : Spec<ConsumableComp>
    {
        Func<Entity, Entity> Byproduct;
        public Spec()
        {

        }
        protected override void ApplyDefaultsTo(ConsumableComp comp)
        {
        }
    }
}
