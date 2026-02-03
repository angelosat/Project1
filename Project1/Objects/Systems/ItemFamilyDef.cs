using Project1.Framework.Base;
using System;

namespace Start_a_Town_
{
    public class ItemFamilyDef : Def
    {
        //public readonly IItemCreationSystem System;
        public ItemFamilyDef(string name) : base(name)
        {
            
        }
        //public ItemFamilyDef(string name, Type systemType) : base(name)
        //{
        //    this.System = (IItemCreationSystem)Activator.CreateInstance(systemType);
        //}
    }
}
