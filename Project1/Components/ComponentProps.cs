using System;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class ComponentProps
    {
        public virtual Type CompClass { get; set; }
        public ComponentProps()
        {

        }
        public ComponentProps(Type compClass)
        {
            this.CompClass = compClass;
        }
        public T CreateComponent<T>() where T : EntityComponent
        {
            return Activator.CreateInstance(this.CompClass) as T; 
        }
        public EntityComponent CreateComponent()
        {
            var comp = Activator.CreateInstance(this.CompClass) as EntityComponent;
            comp.Initialize(this);
            return comp;
        }
    }
}
