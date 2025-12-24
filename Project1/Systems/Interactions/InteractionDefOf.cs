using System;
using System.Collections.Generic;
using System.Text;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class InteractionDefOf
    {
        public static readonly InteractionDef Pick = new("Pick", typeof(InteractionHaul)); 
        public static readonly InteractionDef Place = new("Place", typeof(InteractionPlaceItem));
        public static readonly InteractionDef Chop = new("Chop", typeof(InteractionChop));
        public static readonly InteractionDef Dig = new("Dig", typeof(InteractionBreakBlock));
        public static readonly InteractionDef Store = new("Store", typeof(InteractionStoreHauled));
        public static readonly InteractionDef Equip = new("Equip", typeof(InteractionEquip));
        public static readonly InteractionDef Unequip = new("Unequip", typeof(InteractionUnequip));

        static InteractionDefOf()
        {
            Def.Register(typeof(InteractionDefOf));
        }
    }
}
