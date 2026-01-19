using Start_a_Town_.Interactions;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class InteractionDefOf
    {
        //public static readonly InteractionDef Pick = new("Picking", typeof(InteractionHaul), null) { Animation = AnimationDefOf.TouchItem, ProgressHandler = new InteractionProgressFirstContact() };
        public static readonly InteractionDef Pick = new("Picking", typeof(InteractionHaulLogic))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = new InteractionProgressFirstContact()
        };
        //public static readonly InteractionDef Place = new("Placing", typeof(InteractionPlaceItem), null) { Animation = AnimationDefOf.TouchItem, ProgressHandler = new InteractionProgressFirstContact() };
        public static readonly InteractionDef Place = new("Placing", typeof(InteractionPlaceItemLogic))
        { 
            Animation = AnimationDefOf.TouchItem, 
            ProgressHandler = new InteractionProgressFirstContact() 
        };
        public static readonly InteractionDef Chop = new("Chopping", typeof(InteractionChopLogic)) 
        { 
            Animation = AnimationDefOf.Tool, 
            ProgressHandler = new InteractionProgressToolExternal(),
            Skill = SkillDefOf.Plantcutting,
            ToolUse = ToolUseDefOf.Chopping
        };
        public static readonly InteractionDef Dig = new("Digging", typeof(InteractionBreakBlock), typeof(InteractionBreakBlockLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = new InteractionProgressToolExternal(),
            Skill = SkillDefOf.Mining,
            ToolUse = ToolUseDefOf.Mining
        };
        //public static readonly InteractionDef Store = new("Storing", typeof(InteractionStoreHauled), null);
        public static readonly InteractionDef Store = new("Storing", typeof(InteractionStoreCarriedLogic))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = new InteractionProgressFirstContact()
        };
        public static readonly InteractionDef Equip = new("Equipping", typeof(InteractionEquip), null);
        public static readonly InteractionDef Unequip = new("Unequipping", typeof(InteractionUnequip), null);
        public static readonly InteractionDef Construct = new("Building", typeof(InteractionBuildLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = new InteractionProgressToolExternal(),
            Skill = SkillDefOf.Construction,
            ToolUse = ToolUseDefOf.Building
        };
        //public static readonly InteractionDef Craft = new("Craft", typeof(InteractionCraftingNew), typeof(InteractionCraftingLogic)) { Animation = AnimationDefOf.Tool, ProgressHandler = new InteractionProgressTool() };
        public static readonly InteractionDef Craft = new("Crafting", typeof(InteractionCraftingLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = new InteractionProgressTool(),
            Skill = SkillDefOf.Crafting,
            ToolUse = ToolUseDefOf.Carpentry
        };
        public static readonly InteractionDef Drop = new("Dropping", typeof(InteractionThrow), null);
        public static readonly InteractionDef Depart = new("Departing", typeof(InteractionDepartLogic))
        {
            ProgressHandler = new InteractionProgressInstant()
        };
        public static readonly InteractionDef Till = new("Tilling", typeof(InteractionTillingLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = new InteractionProgressTool(),
            Skill = SkillDefOf.Argiculture,
            ToolUse = ToolUseDefOf.Argiculture
        };
        public static readonly InteractionDef SleepOnGround = new("SleepOnGround", typeof(InteractionSleepOnGroundLogic))
        {
            ProgressHandler = new InteractionProgressPassive()
        };
        public static readonly InteractionDef SleepInBed = new("SleepInBed", typeof(InteractionSleepInBedLogic))
        {
            ProgressHandler = new InteractionProgressPassive()
        };
        public static readonly InteractionDef ToggleDoor = new("ToggleDoor", typeof(InteractionToggleDoorLogic))
        {
            ProgressHandler = new InteractionProgressInstant()
        };
        public static readonly InteractionDef Eat = new("Eat", typeof(InteractionEatingLogic))
        {
            ProgressHandler = new InteractionProgressTimed()
        };
        static InteractionDefOf()
        {
            Def.Register(typeof(InteractionDefOf));
        }
    }
}
