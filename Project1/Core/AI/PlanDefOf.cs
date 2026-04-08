using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Eating;
using Project1.Core.AI.Behaviors.Idle;
using Project1.Core.AI.Behaviors.ItemOwnership;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors.Sleeping;
using Project1.Core.Interactions;
using Project1.Core.Systems.Plants;
using Project1.Core.Towns.AI.Behaviors;
using Project1.Core.Towns.Digging;
using Project1.Core.Towns.Farming;
using Project1.Core.Towns.Forestry;
using Project1.Framework;

namespace Project1.Core.AI
{
    [EnsureStaticCtorCall]
    public static class PlanDefOf
    {
        static public readonly PlanDef Moving = new("Moving", typeof(TaskBehaviorLeaveUnstandableCell))
        {
            Format = "Move {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public readonly PlanDef Digging = new("Digging", typeof(BehaviorDigging), InteractionDefOf.Dig)
        {
            Format = "Dig {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public readonly PlanDef Deconstruct = new("Deconstructing", typeof(BehaviorExecutePlanNew), InteractionDefOf.Deconstruct)
        {
            Format = "Dig {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
       
        static public readonly PlanDef Chatting = new("Chatting", typeof(TaskBehaviorTalkToAboutTopic))
        {
            Format = "Chat",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public readonly PlanDef PickUp = new("Picking Up", typeof(TaskBehaviorStoreInInventory))
        {
            Format = "Force equip {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public readonly PlanDef Chop = new("Chopping Designated", typeof(BehaviorChop), InteractionDefOf.Chop)
        {
            Format = "Chop down {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public readonly PlanDef Idle = new("Idleing", typeof(TaskBehaviorIdle)) { Idle = true };
        static public readonly PlanDef Wander = new("Wandering", typeof(TaskBehaviorWander)) { Idle = true };
        static public readonly PlanDef Depart = new("Departing", typeof(TaskBehaviorDepart), InteractionDefOf.Depart);
        //static public readonly PlanDef DropCarried = new("Dropping carried item", typeof(TaskBehaviorDropItem), InteractionDefOf.Drop);
        static public readonly PlanDef Equip = new("Equipping", typeof(BehaviorEquipItemNew), InteractionDefOf.Equip);
        static public readonly PlanDef Unequip = new("Unquipping", typeof(BehaviorUnequip), InteractionDefOf.Unequip);
        static public readonly PlanDef GoHaul = new("Fetching", typeof(TaskBehaviorGoHaul), InteractionDefOf.Pick);
        static public readonly PlanDef SwapCarried = new("Swapping", typeof(BehaviorExecutePlanNew), InteractionDefOf.Swap);
        static public readonly PlanDef Deposit = new("Depositing", typeof(BehaviorExecutePlanNew), InteractionDefOf.DepositResource);
        static public readonly PlanDef Withdraw = new("Withdrawing", typeof(BehaviorExecutePlanNew), InteractionDefOf.WithdrawCash);
        static public readonly PlanDef RetrieveFromInventory = new("FetchingFromInv", typeof(TaskBehaviorHaulFromInventory), InteractionDefOf.Pick);
        //static public readonly PlanDef GoPlace = new("Placing", typeof(TaskBehaviorGoPlace), InteractionDefOf.Place);
        static public readonly PlanDef GoPlace = new("Placing", typeof(BehaviorPlace), InteractionDefOf.Place);
        static public readonly PlanDef StoreInInventory = new("Storing", typeof(TaskBehaviorStoreInInventory), InteractionDefOf.Store);
        static public readonly PlanDef Construct = new("Constructing", typeof(TaskBehaviorGoConstruct), InteractionDefOf.Construct);
        static public readonly PlanDef Till = new("Tilling", typeof(TaskBehaviorTilling), InteractionDefOf.Till);
        static public readonly PlanDef Harvesting = new("Harvesting", typeof(BehaviorHarvesting), InteractionDefOf.Harvest);
        static public readonly PlanDef Crafting = new("Crafting", typeof(TaskBehaviorGoCraft), InteractionDefOf.Craft);
        static public readonly PlanDef CraftingUnfinishedAdvance = new("CraftingUnfinished", typeof(BehaviorCraftUnfinishedAdvance), InteractionDefOf.CraftUnfinished);
        static public readonly PlanDef CraftingUnfinishedBegin = new("CraftingUnfinishedBegi", typeof(BehaviorGoCraftUnfinishedBegin), InteractionDefOf.CraftUnfinishedBegin);
        static public readonly PlanDef Repairing = new("Repairing", typeof(TaskBehaviorRepairing), InteractionDefOf.Repair);
        static public readonly PlanDef HaulToStockpile = new("StockpileHauling", typeof(TaskBehaviorHaulToStockpile), InteractionDefOf.Place);
        //static public readonly PlanDef Plant = new("Plant", typeof(TaskBehaviorGoPlace), InteractionDefOf.Plant);
        static public readonly PlanDef Plant = new("Plant", typeof(BehaviorPlanting), InteractionDefOf.Plant);
        static public readonly PlanDef SleepingOnGround = new("SleepingOnGround", typeof(BehaviorExecutePlanNew/*TaskBehaviorSleepOnGround*/), InteractionDefOf.SleepOnGround);
        static public readonly PlanDef SleepingOnBed = new("SleepingOnBed", typeof(BehaviorExecutePlanNew/*TaskBehaviorSleepingNew*/), InteractionDefOf.SleepInBed);
        static public readonly PlanDef Eating = new("Eating", typeof(BehaviorEating), InteractionDefOf.Eat);
        static public readonly PlanDef Switching = new("Switching", typeof(BehaviorExecutePlanNew), InteractionDefOf.Switch);
        static public readonly PlanDef WaitForService = new ("Waiting", typeof(BehaviorExecutePlanNew), InteractionDefOf.WaitingService);
        static public readonly PlanDef WaitForPayment = new ("WaitingForPayment", typeof(BehaviorExecutePlanNew), InteractionDefOf.WaitingPayment);
        static public readonly PlanDef Pay = new ("Paying", typeof(BehaviorExecutePlanNew), InteractionDefOf.Pay);
        static public readonly PlanDef RingUp = new ("RingingUp", typeof(BehaviorExecutePlanNew), InteractionDefOf.RingUp);
        static public readonly PlanDef RingUpFinish = new ("RingingUpFinish", typeof(BehaviorExecutePlanNew), InteractionDefOf.RingUpFinish);
        static public readonly PlanDef ClaimBoughtItem = new ("ClaimBoughtItem", typeof(BehaviorExecutePlanNew), InteractionDefOf.ClaimBoughtItem);
        static public readonly PlanDef BrowseProduct = new ("BrowseProduct", typeof(BehaviorExecutePlanNew), InteractionDefOf.BrowseProduct);
        //static public readonly PlanDef CastSpell = new("CastSpell", typeof(BehaviorExecutePlanNew), InteractionDefOf.CastSpell);

        static PlanDefOf()
        {
            Def.Register(typeof(PlanDefOf));
        }
    }
}