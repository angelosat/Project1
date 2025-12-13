using Start_a_Town_.Components;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class EntityDef : Def
    {
        public readonly Type ItemClass;// = typeof(Entity);
        public string Description;
        public float Height = 1;
        public float Weight = 1;
        public bool Haulable = true;
        public Bone Body;

        public EntityDef(string name, Type itemClass) : base(name)
        {
            this.ItemClass = itemClass;
        }
        
        //public List<ComponentProps> CompProps = new List<ComponentProps>();
        public List<EntityComp.Props> CompProps = [];

        //public EntityDef AddCompProp(ComponentProps props)
        //{
        //    this.CompProps.Add(props);
        //    return this;
        //}
        //public T AddCompProp<T>(ComponentProps props) where T : ItemDef
        //{
        //    this.CompProps.Add(props);
        //    return this as T;
        //}
    }
}
