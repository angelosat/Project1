using Project1.Framework.Base;
using Project1.Framework.Entities;
using Start_a_Town_.Components;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class ItemVariantDef : Def, IItemDefVariator
    {
        readonly public ItemDef BaseDef;
        List<EntityComp.Spec> Overrides = [];
        public string Description;
        public ItemVariantDef(ItemDef baseDef, string name) : base(name)
        {
            this.BaseDef = baseDef;
        }
        //public Entity CreateNew()
        //{
        //    var item = this.BaseDef.CreateBase(this);
        //    item.ApplySpecs(this.Overrides);
        //    //return this.ApplyVariantTo(item);
        //    return item;
        //}
        public ItemVariantDef AddSpec(EntityComp.Spec spec)
        {
            this.Overrides.Add(spec);
            return this;
        }
        //[Obsolete]
        //protected virtual Entity ApplyVariantTo(Entity obj) { return obj; }
        public StorageFilterNewNew GetFilter()
        {
            return new(this.Label, this.BaseDef, this);
        }
    }
}
