using Start_a_Town_.Interactions;

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
        public static readonly InteractionDef Construct = new("Construct", typeof(InteractionConstruct), typeof(InteractionConstructWorker), typeof(InteractionConstructCache));

        static InteractionDefOf()
        {
            Def.Register(typeof(InteractionDefOf));
        }
    }
}
