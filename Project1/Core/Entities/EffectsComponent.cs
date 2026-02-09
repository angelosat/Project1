using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Framework;
using Project1.Core.Effects;
using Project1.Core.Needs;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;

namespace Project1.Core.Entities
{
    public class EffectsComponent : EntityComp
    {
        public new class Spec : Spec<EffectsComponent> { }

        public override string Name => "Effects";

        List<EntityEffectWrapper> ActiveEffects = [];
        public void Apply(EntityEffectWrapper effect)
        {
            effect.Start(this.Owner as Actor);
            if (!effect.IsInstant)
                this.ActiveEffects.Add(effect);
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
}
