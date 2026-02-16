using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Eating;
using Project1.Core.AI.Behaviors.Idle;
using Project1.Core.AI.Behaviors.ItemOwnership;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors.Sleeping;
using Project1.Core.Interactions;
using Project1.Core.Plants;
using Project1.Core.Towns;
using Project1.Core.Towns.AI.Behaviors;
using Project1.Core.Towns.Constructions.AI;
using Project1.Core.Towns.Digging;
using Project1.Core.Towns.Farming;
using Project1.Core.Towns.Forestry;
using Project1.Framework;

namespace Project1.Core.AI
{
    [EnsureStaticCtorCall]
    public static class PlanDefOf
    {

        static public readonly PlanDef Refueling = new("Refueling", typeof(BehaviorRefueling))
        {
            Format = "Refuel {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public readonly PlanDef DeliverMaterials = new("DeliverMaterials", typeof(TaskBehaviorDeliverMaterials))
        {
            Format = "Deliver materials to {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public readonly PlanDef Moving = new("Moving", typeof(TaskBehaviorLeaveUnstandableCell))
        {
            Format = "Move {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };


        static public readonly PlanDef Digging = new("Digging", typeof(TaskBehaviorDigging), InteractionDefOf.Dig)
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
        static public readonly PlanDef DropCarried = new("Dropping carried item", typeof(TaskBehaviorDropItem), InteractionDefOf.Drop);
        static public readonly PlanDef Equip = new("Equipping", typeof(BehaviorEquipItemNew), InteractionDefOf.Equip);
        static public readonly PlanDef Unequip = new("Unquipping", typeof(BehaviorUnequip), InteractionDefOf.Unequip);
        static public readonly PlanDef GoHaul = new("Fetching", typeof(TaskBehaviorGoHaul), InteractionDefOf.Pick);
        static public readonly PlanDef RetrieveFromInventory = new("FetchingFromInv", typeof(TaskBehaviorHaulFromInventory), InteractionDefOf.Pick);
        //static public readonly PlanDef GoPlace = new("Placing", typeof(TaskBehaviorGoPlace), InteractionDefOf.Place);
        static public readonly PlanDef GoPlace = new("Placing", typeof(BehaviorPlace), InteractionDefOf.Place);
        static public readonly PlanDef StoreInInventory = new("Storing", typeof(TaskBehaviorStoreInInventory), InteractionDefOf.Store);
        static public readonly PlanDef Construct = new("Constructing", typeof(TaskBehaviorGoConstruct), InteractionDefOf.Construct);
        static public readonly PlanDef Till = new("Tilling", typeof(TaskBehaviorTilling), InteractionDefOf.Till);
        static public readonly PlanDef Harvesting = new("Harvesting", typeof(BehaviorHarvesting), InteractionDefOf.Harvest);
        static public readonly PlanDef Crafting = new("Crafting", typeof(TaskBehaviorGoCraft), InteractionDefOf.Craft);
        static public readonly PlanDef Repairing = new("Repairing", typeof(TaskBehaviorRepairing), InteractionDefOf.Repair);
        static public readonly PlanDef HaulToStockpile = new("StockpileHauling", typeof(TaskBehaviorHaulToStockpile), InteractionDefOf.Place);
        //static public readonly PlanDef Plant = new("Plant", typeof(TaskBehaviorGoPlace), InteractionDefOf.Plant);
        static public readonly PlanDef Plant = new("Plant", typeof(BehaviorPlanting), InteractionDefOf.Plant);
        static public readonly PlanDef SleepingOnGround = new("SleepingOnGround", typeof(TaskBehaviorSleepOnGround), InteractionDefOf.SleepOnGround);
        static public readonly PlanDef SleepingOnBed = new("SleepingOnBed", typeof(TaskBehaviorSleepingNew), InteractionDefOf.SleepInBed);
        static public readonly PlanDef Eating = new("Eating", typeof(BehaviorEating), InteractionDefOf.Eat);
        static PlanDefOf()
        {
            Def.Register(typeof(PlanDefOf));
        }
    }
}