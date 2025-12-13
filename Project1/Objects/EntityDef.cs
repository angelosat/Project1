using Start_a_Town_.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class EntityDef : Def
    {
        public readonly Type ItemClass;// = typeof(Entity);
        public string Description;
        public float Height = 1;
        public float Weight = 1;
        public bool Haulable = true;

        /// <summary>
        /// move this to spritecomp.props
        /// </summary>
        //[Obsolete($"move this to {nameof(SpriteComp.Props)}")]
        //public Bone Body;

        public EntityDef(string name, Type itemClass) : base(name)
        {
            this.ItemClass = itemClass;
        }

        //public List<ComponentProps> CompProps = new List<ComponentProps>();
        public readonly List<EntityComp.Props> CompProps = [];// [new SpriteComp.Props()];

        public EntityComp.Props GetPropsFor<T>() where T: EntityComp
        {
            //return this.CompProps.First(p => p.GetType() == typeof(T));
            return this.CompProps.First(p => typeof(T).IsAssignableFrom(p.GetType()));

        }

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
