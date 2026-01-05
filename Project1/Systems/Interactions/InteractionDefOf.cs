using Start_a_Town_.Crafting;
using Start_a_Town_.Interactions;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class InteractionDefOf
    {
        public static readonly InteractionDef Pick = new("Pick", typeof(InteractionHaul)) { Animation = AnimationDefOf.TouchItem, ProgressHandler = new InteractionProgressInstant() }; 
        public static readonly InteractionDef Place = new("Place", typeof(InteractionPlaceItem)) { Animation = AnimationDefOf.TouchItem, ProgressHandler = new InteractionProgressInstant() };
        public static readonly InteractionDef Chop = new("Chop", typeof(InteractionChop), typeof(InteractionChopLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressToolExternal() };
        public static readonly InteractionDef Dig = new("Dig", typeof(InteractionBreakBlock), typeof(InteractionBreakBlockLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressToolExternal() };
        public static readonly InteractionDef Store = new("Store", typeof(InteractionStoreHauled));
        public static readonly InteractionDef Equip = new("Equip", typeof(InteractionEquip));
        public static readonly InteractionDef Unequip = new("Unequip", typeof(InteractionUnequip));
        public static readonly InteractionDef Construct = new("Construct", typeof(InteractionConstruct), typeof(InteractionConstructLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressToolExternal() };
        public static readonly InteractionDef Craft = new("Craft", typeof(InteractionCraftingNew), typeof(InteractionCraftingLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressTool() };
        public static readonly InteractionDef Drop = new("Drop", typeof(InteractionThrow));
        public static readonly InteractionDef Depart = new("Depart", typeof(InteractionDepart));
        public static readonly InteractionDef Till = new("Till", typeof(InteractionTilling), typeof(InteractionTillingLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressTool() };

        static InteractionDefOf()
        {
            Def.Register(typeof(InteractionDefOf));
        }
    }
}
