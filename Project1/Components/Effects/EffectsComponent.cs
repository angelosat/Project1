using Start_a_Town_;
using Start_a_Town_.Components;
using System;
using System.Collections.Generic;

namespace Project1.Components.Effects
{
    public class EffectsComponent : EntityComponent
    {
        public override string Name => "Effects";

        readonly List<EntityEffectWrapper> ActiveEffects = [];

        public void Apply(EffectDef effect)
        {
            this.ActiveEffects.Add(new EntityEffectWrapper(effect));
            effect.Worker.OnStart(this.Parent as Actor);
        }
        public override object Clone()
        {
            return new EffectsComponent();
        }

        internal void Remove(EffectDef effect)
        {
            throw new NotImplementedException();
        }
    }
}
