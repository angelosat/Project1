using SharpDX.XAudio2;
using Start_a_Town_.Components;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public abstract class VariantProps : Def
    {
        readonly public ItemDef BaseDef;
        //readonly protected List<EntityComp.Spec> Specs = [];
        protected VariantProps(ItemDef baseDef, string name) : base(name)
        {
            this.BaseDef = baseDef;
        }
        //protected virtual void ApplyTo(Entity item)
        //{

        //}
        public Entity CreateNew()
        {
            var item = this.BaseDef.CreateNew();
            item.VariantDef = this;
            return this.ApplyTo(item);
        }
        protected abstract Entity ApplyTo(Entity obj);
        //{
            //var item = this.BaseDef.CreateNew();
            //item.VariantDef = this;
            //foreach (var spec in this.Specs)
            //{
            //    if (!item.TryGetComponent(spec.CompClass, out var comp))
            //        throw new System.Exception();
            //    spec.Apply(comp);
            //}
            //item.Name = this.Label;
            //return item;
        //}
        //public abstract VariantProps AddSpec(EntityComp.Spec spec);
    }
    //public class VariantProps<T> : VariantProps where T : VariantProps<T>
    //{
    //    public VariantProps(ItemDef baseDef, string name) : base(baseDef, name)
    //    {
    //    }
    //    public override T AddSpec(EntityComp.Spec spec)
    //    {
    //        this.Specs.Add(spec);
    //        return (T)this;
    //    }
    //}
}
