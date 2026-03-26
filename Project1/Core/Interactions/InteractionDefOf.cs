using Project1.Core.Animations;
using Project1.Core.Skills;
using Project1.Core.Systems.Plants;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns.AI;
using Project1.Core.Towns.Shops;
using Project1.Framework;

namespace Project1.Core.Interactions
{
    [EnsureStaticCtorCall]
    internal static class InteractionDefOf
    {
        public static readonly InteractionDef Harvest = new("Harvesting", typeof(InteractionHarvestLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = InteractionProgressHandlers.FirstContact// new InteractionProgressFirstContact()
        };
        public static readonly InteractionDef Pick = new("Picking", typeof(InteractionHaulLogic))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact// new InteractionProgressFirstContact()
        };
        public static readonly InteractionDef Swap = new("Swap", typeof(InteractionSwapItemLogic))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact
        };
        public static readonly InteractionDef Place = new("Placing", typeof(InteractionPlaceItemLogic))
        { 
            Animation = AnimationDefOf.TouchItem, 
            ProgressHandler = InteractionProgressHandlers.FirstContact,// new InteractionProgressFirstContact(),
            Range = InteractionRange.Any
        };
        public static readonly InteractionDef Pay = new("Paying", typeof(InteractionPayTransaction))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact,// new InteractionProgressFirstContact(),
            Range = InteractionRange.Any
        };
        public static readonly InteractionDef RingUp = new("RingingUp", typeof(InteractionRingUpTransaction))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact,
            Range = InteractionRange.Any
        };
        public static readonly InteractionDef RingUpFinish = new("RingingUpFinish", typeof(InteractionRingUpTransactionFinish))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact,
            Range = InteractionRange.Any
        };
        public static readonly InteractionDef DepositResource = new("Depositing", typeof(InteractionDepositLogic))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact,// new InteractionProgressFirstContact(),
            Range = InteractionRange.Any
        };
        public static readonly InteractionDef Plant = new("Planting", typeof(InteractionPlantLogic))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact// new InteractionProgressFirstContact()
        };
        public static readonly InteractionDef Chop = new("Chopping", typeof(InteractionChopLogic)) 
        { 
            Animation = AnimationDefOf.Tool, 
            ProgressHandler = InteractionProgressHandlers.External,// new InteractionProgressToolExternal(),
            Skill = SkillDefOf.Plantcutting,
            ToolUse = ToolUseDefOf.Chopping
        };
        public static readonly InteractionDef Dig = new("Digging", /*typeof(InteractionBreakBlock), */typeof(InteractionBreakBlockLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = InteractionProgressHandlers.External,// new InteractionProgressToolExternal(),
            Skill = SkillDefOf.Digging,
            ToolUse = ToolUseDefOf.Digging
        };
        //public static readonly InteractionDef Store = new("Storing", typeof(InteractionStoreHauled), null);
        public static readonly InteractionDef Store = new("Storing", typeof(InteractionStoreCarriedLogic))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact// new InteractionProgressFirstContact()
        };
        public static readonly InteractionDef Equip = new("Equipping", typeof(InteractionEquipLogic))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact// new InteractionProgressFirstContact()
        };
        public static readonly InteractionDef Unequip = new("Unequipping", typeof(InteractionUnequipLogic))
          {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact// new InteractionProgressFirstContact()
        };
        public static readonly InteractionDef Construct = new("Building", typeof(InteractionBuildLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = InteractionProgressHandlers.External,// new InteractionProgressToolExternal(),
            Skill = SkillDefOf.Construction,
            ToolUse = ToolUseDefOf.Building
        };
        public static readonly InteractionDef Deconstruct = new("Deconstructing", typeof(InteractionDeconstructLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = InteractionProgressHandlers.Internal,// new InteractionProgressTool(),
            Skill = SkillDefOf.Construction,
            ToolUse = ToolUseDefOf.Building
        };
        public static readonly InteractionDef Craft = new("Crafting", typeof(InteractionCraftingLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = InteractionProgressHandlers.Internal,// new InteractionProgressTool(),
            Skill = SkillDefOf.Crafting,
            ToolUse = ToolUseDefOf.Carpentry,
            Range = InteractionRange.InteractionSpot
        };
        public static readonly InteractionDef CraftUnfinishedBegin = new("CraftingUnfinished", typeof(InteractionCommitUnfinishedLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = InteractionProgressHandlers.FirstContact,// new InteractionProgressTool(),
            Skill = SkillDefOf.Crafting,
            ToolUse = ToolUseDefOf.Carpentry,
            Range = InteractionRange.InteractionSpot
        };
        public static readonly InteractionDef CraftUnfinished = new("CraftingUnfinishedAdvance", typeof(InteractionAdvanceUnfinishedLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = InteractionProgressHandlers.External,// new InteractionProgressTool(),
            Skill = SkillDefOf.Crafting,
            ToolUse = ToolUseDefOf.Carpentry,
            Range = InteractionRange.InteractionSpot
        };
        public static readonly InteractionDef Repair = new("Repairing", typeof(InteractionRepairLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = InteractionProgressHandlers.External,// new InteractionProgressToolExternal(),
            Skill = SkillDefOf.Tinkering
        };
        //public static readonly InteractionDef Drop = new("Dropping", typeof(InteractionThrow), null);
        public static readonly InteractionDef Depart = new("Departing", typeof(InteractionDepartLogic))
        {
            ProgressHandler = InteractionProgressHandlers.Instant// new InteractionProgressInstant()
        };
        public static readonly InteractionDef Till = new("Tilling", typeof(InteractionTillingLogic))
        {
            Animation = AnimationDefOf.Tool,
            ProgressHandler = InteractionProgressHandlers.Internal,// new InteractionProgressTool(),
            Skill = SkillDefOf.Argiculture,
            ToolUse = ToolUseDefOf.Argiculture
        };
        public static readonly InteractionDef SleepOnGround = new("SleepOnGround", typeof(InteractionSleepOnGroundLogic))
        {
            ProgressHandler = InteractionProgressHandlers.Passive// new InteractionProgressPassive()
        };
        public static readonly InteractionDef SleepInBed = new("SleepInBed", typeof(InteractionSleepInBedLogic))
        {
            ProgressHandler = InteractionProgressHandlers.Passive// new InteractionProgressPassive()
        };
        public static readonly InteractionDef ToggleDoor = new("ToggleDoor", typeof(InteractionToggleDoorLogic))
        {
            ProgressHandler = InteractionProgressHandlers.Instant// new InteractionProgressInstant()
        };
        public static readonly InteractionDef Eat = new("Eat", typeof(InteractionEatingLogic))
        {
            ProgressHandler = InteractionProgressHandlers.Timed// new InteractionProgressTimed()
        };
        public static readonly InteractionDef Switch = new("Switch", typeof(InteractionFlipSwitchLogic))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact// new InteractionProgressTimed()
        };
        public static readonly InteractionDef WaitingService = new("WaitService", typeof(InteractionWaitingService))
        {
            ProgressHandler = InteractionProgressHandlers.Passive
        };
        public static readonly InteractionDef WaitingPayment = new("WaitPayment", typeof(InteractionWaitForPayment))
        {
            ProgressHandler = InteractionProgressHandlers.Passive
        };
        public static readonly InteractionDef ClaimBoughtItem = new("ClaimBoughtItem", typeof(InteractionClaimBoughtItem))
        {
            Animation = AnimationDefOf.TouchItem,
            ProgressHandler = InteractionProgressHandlers.FirstContact
        };
        public static readonly InteractionDef BrowseProduct = new("BrowseProduct", typeof(InteractionBrowseProduct))
        {
            ProgressHandler = InteractionProgressHandlers.Timed
            //ProgressHandler = InteractionProgressHandlers.External
        };
        static InteractionDefOf()
        {
            Def.Register(typeof(InteractionDefOf));
        }
    }
}
