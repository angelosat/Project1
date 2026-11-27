using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Serialization;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Magic;
using Project1.Core.Systems.Quality;
using Project1.Core.Towns.Services.Spells;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Project1.Core.Systems.Consumables;

public sealed class ConsumableComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Consumable;
    public override string Name { get; } = "Consumable";

    public SpellDef Spell;

    public Tier Tier;

    public EffectDef PrimaryEffect => this.Spell!.Effects.First().Key.Def;

    public bool HasEffect(EffectDef effect, Def? target) => this.Spell.Effects.Keys.Any(f => f.Def == effect && f.Target == target);

    internal int GetMagnitude(EffectDef effect, Def? target)
        => this.Spell.Effects.GetValueOrDefault(new(effect, target));

    internal override IEnumerable<Control> GetTooltipControls()
    {
        //foreach (var (key, magnitude) in this.Spell.Effects)
        //{
        //    //var text = key.Def.Worker.GetString(key.Target, magnitude);
        //    var text = key.Def.Worker.GetString(key.Target, magnitude);
        //    var box = new GroupBox().AddControlsLineWrap(LabelNew.ParseNewNew(text), 200);
        //    yield return box;
        //}
        foreach (var asd in this.Spell.Periodic.Prepend( this.Spell.Instant))
        {
            //var text = key.Def.Worker.GetString(key.Target, magnitude);
            var text = asd.GetString(1f);
            var box = new GroupBox().AddControlsLineWrap(LabelNew.ParseNewNew(text), 200);
            yield return box;
        }
    }

    internal override void CopyFrom(EntityComp source)
    {
        var comp = source as ConsumableComp;
        this.Spell = comp.Spell;
    }

    internal override void Validate()
    {
        var quality = this.Owner.QualityComp.Tier;
        var mod = quality.Multiplier;
    }

    internal override void PostMapLoad()
    {
        var profile = (ConsumableDef)this.Owner.Profile;
        this.Owner.Name = profile.Worker.GetLabel(this);
        this.Owner.Body.Sprite = profile.Sprite;
    }

    public override void Write(IDataWriter w)
    {
        w.Write(this.Spell);
    }

    public override void Read(IDataReader r)
    {
        this.Spell = r.ReadDef<SpellDef>();
    }
    internal override void SaveExtra(SaveTag tag)
    {
        tag.Save("Spell", this.Spell);
    }
    internal override void LoadExtra(SaveTag tag)
    {
        this.Spell = tag.LoadDef<SpellDef>("Spell");
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
