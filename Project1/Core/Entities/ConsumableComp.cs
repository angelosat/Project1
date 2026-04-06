using Microsoft.Xna.Framework;
using Project1.Core.Effects;
using Project1.Core.Helpers;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Entities;

public class ConsumableComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Consumable;
    public override string Name { get; } = "Consumable";

    public List<EntityEffectWrapper> EffectsNew = [];
    public GameObject Seeds;

    public bool HasEffectTarget(Def target) => this.EffectsNew.Any(f => f.Target == target);
    public override void OnTooltipCreated(GameObject parent, Control tooltip)
    {
        foreach (var effect in this.EffectsNew)
            tooltip.Controls.Add(
                new Label(effect) { Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.ForestGreen }
                );
    }
    internal override void CopyFrom(EntityComp source)
    {
        var comp = source as ConsumableComp;
        foreach (var f in comp.EffectsNew)
            this.EffectsNew.Add(new EntityEffectWrapper(f.Def,  f.Target, f.Budget, f.TicksPerUnit));
    }

    internal void Consume(GameObject actor)
    {
    }

    public override void GetInventoryTooltip(GameObject parent, Control tooltip)
    {
        this.OnTooltipCreated(parent, tooltip);
    }

    public override void Write(IDataWriter w)
    {
        w.Write(this.EffectsNew);
    }
    public override void Read(IDataReader r)
    {
        this.EffectsNew = r.ReadList<EntityEffectWrapper>();
    }
    internal override void SaveExtra(SaveTag tag)
    {
        tag.Save("Effects", this.EffectsNew);
    }
    internal override void LoadExtra(SaveTag tag)
    {
        this.EffectsNew = tag.LoadList<EntityEffectWrapper>("Effects");
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
