using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    public class EffectsComponent : EntityComponent
    {
        public override string Name => "Effects";

        List<EntityEffectWrapper> ActiveEffects = [];

        public void Apply(EffectDef effect)
        {
            this.ActiveEffects.Add(new EntityEffectWrapper(effect));
            effect.Worker.OnStart(this.Parent as Actor);
        }
        internal void Remove(EffectDef effect)
        {
            this.ActiveEffects.RemoveAll(e => e.Def == effect);
        }

        public override void Write(BinaryWriter w)
        {
            w.WriteNew(this.ActiveEffects);
            //this.ActiveEffects.WriteNew(w);
        }
        public override void Read(IDataReader r)
        {
            this.ActiveEffects = r.ReadList<EntityEffectWrapper>();
        }
        internal override void SaveExtra(SaveTag tag)
        {
            //tag.Add(this.ActiveEffects.Save("ActiveEffects"));
            tag.Add("ActiveEffects", this.ActiveEffects);
        }
        internal override void LoadExtra(SaveTag tag)
        {
            //tag.TryGetTag("ActiveEffects", this.ActiveEffects.LoadFrom);
            tag.TryLoadList("ActiveEffects", ref this.ActiveEffects);
        }
        
        public override object Clone()
        {
            return new EffectsComponent();
        }
    }
}
