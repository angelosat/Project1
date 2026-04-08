using Project1.Core.Animations;
using Project1.Core.Skills;
using Project1.Core.Systems.Plants;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns;
using Project1.Core.Towns.AI;
using Project1.Core.Towns.Healing;
using Project1.Core.Towns.Inns;
using Project1.Core.Towns.Shops;
using Project1.Framework;

namespace Project1.Core.Interactions;

[EnsureStaticCtorCall]
internal static class InteractionDefOf
{
    public static readonly InteractionDef Harvest = new("Harvesting", typeof(InteractionHarvestLogic))
    {
        Animation = AnimationDefOf.Tool,
        Controller = InteractionControllers.FirstContact// new InteractionProgressFirstContact()
    };
    public static readonly InteractionDef Pick = new("Picking", typeof(InteractionHaulLogic))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact// new InteractionProgressFirstContact()
    };
    public static readonly InteractionDef WithdrawCash = new("Withdrawing", typeof(InteractionWithdrawCashOverflow))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact// new InteractionProgressFirstContact()
    };
    public static readonly InteractionDef Swap = new("Swap", typeof(InteractionSwapItemLogic))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact
    };
    public static readonly InteractionDef Place = new("Placing", typeof(InteractionPlaceItemLogic))
    { 
        Animation = AnimationDefOf.TouchItem, 
        Controller = InteractionControllers.FirstContact,// new InteractionProgressFirstContact(),
        Range = InteractionRange.Any
    };
    public static readonly InteractionDef Pay = new("Paying", typeof(InteractionPayTransaction))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact,// new InteractionProgressFirstContact(),
        Range = InteractionRange.Any
    };
    public static readonly InteractionDef PayForBed = new("PayingForBed", typeof(InteractionPayForBed))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact,// new InteractionProgressFirstContact(),
        Range = InteractionRange.Any
    };
    public static readonly InteractionDef RingUp = new("RingingUp", typeof(InteractionRingUpTransaction))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact,
        Range = InteractionRange.Any
    };
    public static readonly InteractionDef RingUpFinish = new("RingingUpFinish", typeof(InteractionRingUpTransactionFinish))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact,
        Range = InteractionRange.Any
    };
    public static readonly InteractionDef DepositResource = new("Depositing", typeof(InteractionDepositLogic))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact,// new InteractionProgressFirstContact(),
        Range = InteractionRange.Any
    };
    public static readonly InteractionDef Plant = new("Planting", typeof(InteractionPlantLogic))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact// new InteractionProgressFirstContact()
    };
    public static readonly InteractionDef Chop = new("Chopping", typeof(InteractionChopLogic)) 
    { 
        Animation = AnimationDefOf.Tool, 
        Controller = InteractionControllers.External,// new InteractionProgressToolExternal(),
        Skill = SkillDefOf.Plantcutting,
        ToolUse = ToolUseDefOf.Chopping
    };
    public static readonly InteractionDef Dig = new("Digging", /*typeof(InteractionBreakBlock), */typeof(InteractionBreakBlockLogic))
    {
        Animation = AnimationDefOf.Tool,
        Controller = InteractionControllers.External,// new InteractionProgressToolExternal(),
        Skill = SkillDefOf.Digging,
        ToolUse = ToolUseDefOf.Digging
    };
    //public static readonly InteractionDef Store = new("Storing", typeof(InteractionStoreHauled), null);
    public static readonly InteractionDef Store = new("Storing", typeof(InteractionStoreCarriedLogic))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact// new InteractionProgressFirstContact()
    };
    public static readonly InteractionDef Equip = new("Equipping", typeof(InteractionEquipLogic))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact// new InteractionProgressFirstContact()
    };
    public static readonly InteractionDef Unequip = new("Unequipping", typeof(InteractionUnequipLogic))
      {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact// new InteractionProgressFirstContact()
    };
    public static readonly InteractionDef Construct = new("Building", typeof(InteractionBuildLogic))
    {
        Animation = AnimationDefOf.Tool,
        Controller = InteractionControllers.External,// new InteractionProgressToolExternal(),
        Skill = SkillDefOf.Construction,
        ToolUse = ToolUseDefOf.Building
    };
    public static readonly InteractionDef Deconstruct = new("Deconstructing", typeof(InteractionDeconstructLogic))
    {
        Animation = AnimationDefOf.Tool,
        Controller = InteractionControllers.Internal,// new InteractionProgressTool(),
        Skill = SkillDefOf.Construction,
        ToolUse = ToolUseDefOf.Building
    };
    public static readonly InteractionDef Craft = new("Crafting", typeof(InteractionCrafting))
    {
        Animation = AnimationDefOf.Tool,
        Controller = InteractionControllers.Internal,// new InteractionProgressTool(),
        Skill = SkillDefOf.Crafting,
        ToolUse = ToolUseDefOf.Carpentry,
        Range = InteractionRange.InteractionSpot
    };
    public static readonly InteractionDef CraftUnfinishedBegin = new("CraftingUnfinished", typeof(InteractionCommitUnfinishedLogic))
    {
        Animation = AnimationDefOf.Tool,
        Controller = InteractionControllers.FirstContact,// new InteractionProgressTool(),
        Skill = SkillDefOf.Crafting,
        ToolUse = ToolUseDefOf.Carpentry,
        Range = InteractionRange.InteractionSpot
    };
    public static readonly InteractionDef CraftUnfinished = new("CraftingUnfinishedAdvance", typeof(InteractionAdvanceUnfinishedLogic))
    {
        Animation = AnimationDefOf.Tool,
        Controller = InteractionControllers.External,// new InteractionProgressTool(),
        Skill = SkillDefOf.Crafting,
        ToolUse = ToolUseDefOf.Carpentry,
        Range = InteractionRange.InteractionSpot
    };
    public static readonly InteractionDef Repair = new("Repairing", typeof(InteractionRepairLogic))
    {
        Animation = AnimationDefOf.Tool,
        Controller = InteractionControllers.External,// new InteractionProgressToolExternal(),
        Skill = SkillDefOf.Tinkering
    };
    //public static readonly InteractionDef Drop = new("Dropping", typeof(InteractionThrow), null);
    public static readonly InteractionDef Depart = new("Departing", typeof(InteractionDepart))
    {
        Controller = InteractionControllers.Instant// new InteractionProgressInstant()
    };
    public static readonly InteractionDef Till = new("Tilling", typeof(InteractionTillingLogic))
    {
        Animation = AnimationDefOf.Tool,
        Controller = InteractionControllers.Internal,// new InteractionProgressTool(),
        Skill = SkillDefOf.Argiculture,
        ToolUse = ToolUseDefOf.Argiculture
    };
    public static readonly InteractionDef SleepOnGround = new("SleepOnGround", typeof(InteractionSleepOnGroundLogic))
    {
        Controller = InteractionControllers.Passive// new InteractionProgressPassive()
    };
    public static readonly InteractionDef SleepInBed = new("SleepInBed", typeof(InteractionSleepInBedLogic))
    {
        Controller = InteractionControllers.Passive// new InteractionProgressPassive()
    };
    public static readonly InteractionDef ToggleDoor = new("ToggleDoor", typeof(InteractionToggleDoorLogic))
    {
        Controller = InteractionControllers.Instant// new InteractionProgressInstant()
    };
    public static readonly InteractionDef Eat = new("Eat", typeof(InteractionEatingLogic))
    {
        Controller = InteractionControllers.Timed// new InteractionProgressTimed()
    };
    public static readonly InteractionDef Switch = new("Switch", typeof(InteractionFlipSwitchLogic))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact// new InteractionProgressTimed()
    };
    public static readonly InteractionDef WaitingService = new("WaitService", typeof(InteractionWaitingService))
    {
        //Controller = InteractionControllers.Passive
        Controller = InteractionControllers.ExternalFull
    };
    public static readonly InteractionDef WaitingPayment = new("WaitPayment", typeof(InteractionWaitForPayment))
    {
        Controller = InteractionControllers.Passive
    };
    public static readonly InteractionDef ClaimBoughtItem = new("ClaimBoughtItem", typeof(InteractionClaimBoughtItem))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact
    };
    public static readonly InteractionDef BrowseProduct = new("BrowseProduct", typeof(InteractionBrowseProduct))
    {
        Controller = InteractionControllers.Timed
        //ProgressHandler = InteractionProgressHandlers.External
    };
    public static readonly InteractionDef CastSpell = new("CastSpell", typeof(InteractionCastSpell), InteractionControllers.Timed);
    static InteractionDefOf()
    {
        Def.Register(typeof(InteractionDefOf));
    }
}
