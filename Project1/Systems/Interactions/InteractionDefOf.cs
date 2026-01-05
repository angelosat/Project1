using Start_a_Town_.Interactions;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class InteractionDefOf
    {
        public static readonly InteractionDef Pick = new("Pick", typeof(InteractionHaul), null) { Animation = AnimationDefOf.TouchItem, ProgressHandler = new InteractionProgressInstant() }; 
        public static readonly InteractionDef Place = new("Place", typeof(InteractionPlaceItem), null) { Animation = AnimationDefOf.TouchItem, ProgressHandler = new InteractionProgressInstant() };
        public static readonly InteractionDef Chop = new("Chop", typeof(InteractionChop), typeof(InteractionChopLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressToolExternal() };
        public static readonly InteractionDef Dig = new("Dig", typeof(InteractionBreakBlock), typeof(InteractionBreakBlockLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressToolExternal() };
        public static readonly InteractionDef Store = new("Store", typeof(InteractionStoreHauled), null);
        public static readonly InteractionDef Equip = new("Equip", typeof(InteractionEquip), null);
        public static readonly InteractionDef Unequip = new("Unequip", typeof(InteractionUnequip), null);
        public static readonly InteractionDef Construct = new("Construct", typeof(InteractionConstruct), typeof(InteractionConstructLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressToolExternal() };
        //public static readonly InteractionDef Craft = new("Craft", typeof(InteractionCraftingNew), typeof(InteractionCraftingLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressTool() };
        public static readonly InteractionDef Craft = new("Craft", typeof(InteractionCraftingLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressTool() };
        public static readonly InteractionDef Drop = new("Drop", typeof(InteractionThrow), null);
        public static readonly InteractionDef Depart = new("Depart", typeof(InteractionDepart), null);
        public static readonly InteractionDef Till = new("Till", typeof(InteractionTilling), typeof(InteractionTillingLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressTool() };

        static InteractionDefOf()
        {
            Def.Register(typeof(InteractionDefOf));
        }
    }
}
