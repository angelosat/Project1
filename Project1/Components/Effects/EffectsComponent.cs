using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class EffectsComponent : EntityComp
    {
        public new class Spec : Spec<EffectsComponent> { }

        public override string Name => "Effects";

        List<EntityEffectWrapper> ActiveEffects = [];
        public void Apply(EntityEffectWrapper effect)
        {
            this.ActiveEffects.Add(effect);
            effect.Def.Worker.OnStart(this.Owner as Actor, effect);
        }
        [Obsolete("add EntityEffectWrapper instead")]
        public void Apply(EffectDef effect)
        {
            var wrapper = new EntityEffectWrapper(effect, null, 1);
            this.ActiveEffects.Add(wrapper);
            wrapper.Start(this.Owner as Actor);
            //effect.Worker.OnStart(this.Owner as Actor, wrapper);
        }
        internal void Remove(EffectDef effect)
        {
            var relevantEffects = this.ActiveEffects.Where(f => f.Def == effect);
            foreach (var f in relevantEffects)
            {
                //f.Def.Worker.OnFinish(this.Owner as Actor, f);
                f.Finish(this.Owner as Actor);
                this.ActiveEffects.Remove(f);
            }
            //this.ActiveEffects.RemoveAll(e => e.Def == effect);
            //effect.Worker.OnFinish(this.Owner as Actor);
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
            tag.Add("ActiveEffects", this.ActiveEffects);
        }
        internal override void LoadExtra(SaveTag tag)
        {
            tag.TryLoadList("ActiveEffects", ref this.ActiveEffects);
        }
        internal override GroupBox GetDetailedGui()
        {
            return base.GetDetailedGui();
        }
        public override object Clone()
        {
            return new EffectsComponent();
        }
    }
}
