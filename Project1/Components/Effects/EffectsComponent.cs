using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class EffectsComponent : EntityComp
    {
        public new class Spec : Spec<EffectsComponent> { }

        public override string Name => "Effects";

        List<EntityEffectWrapper> ActiveEffects = [];

        public void Apply(EffectDef effect)
        {
            this.ActiveEffects.Add(new EntityEffectWrapper(effect));
            effect.Worker.OnStart(this.Owner as Actor);
        }
        internal void Remove(EffectDef effect)
        {
            this.ActiveEffects.RemoveAll(e => e.Def == effect);
            effect.Worker.OnFinish(this.Owner as Actor);
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
