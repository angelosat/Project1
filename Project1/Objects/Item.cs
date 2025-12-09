using System;
using System.Collections.Generic;
using System.Text;

namespace Start_a_Town_
{
    internal sealed class Item : Entity
    {
        public Item()
        {
            
        }
        public Item(ItemDef def) : base(def)
        {
                
        }
        public override GameObject Create()
        {
            return new Item(this.Def);
        }
    }
}
