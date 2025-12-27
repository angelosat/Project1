using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI.Behaviors.ItemOwnership;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public static class PlanDefOf
    {
        static public PlanDef Crafting = new("Crafting", typeof(TaskBehaviorGoCraft))//typeof(TaskBehaviorCrafting))
        {
            Format = "Force crafting at {0}",
            GetPrimaryTarget = t => t.GetTarget(TaskBehaviorCrafting.WorkstationIndex)
        };

        static public PlanDef Hauling = new("Hauling", typeof(TaskBehaviorHaulToStockpile))
        {
            Format = "Force haul {0}",
            GetPrimaryTarget = t => t.GetTarget(TaskBehaviorHaulToStockpile.ItemInd)
        };
        public static PlanDef Refueling = new("Refueling", typeof(TaskBehaviorRefueling))
        {
            Format = "Refuel {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public PlanDef HaulAside = new("HaulAside", typeof(TaskBehaviorHaulAside))
        {
            Format = "Haul aside {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        //static public PlanDef Construct = new("Construct", typeof(TaskBehaviorConstruct))
        //{
        //    Format = "Construct {0}",
        //    GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        //};

        static public PlanDef DeliverMaterials = new("DeliverMaterials", typeof(TaskBehaviorDeliverMaterials))
        {
            Format = "Deliver materials to {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public PlanDef Sowing = new("Sowing", typeof(TaskBehaviorDeliverMaterials))
        {
            Format = "Sow {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public PlanDef Tilling = new("Tilling", typeof(TaskBehaviorTilling))
        {
            Format = "Till {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public PlanDef Moving = new("Moving", typeof(TaskBehaviorLeaveUnstandableCell))
        {
            Format = "Move {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public PlanDef Harvesting = new("Harvesting", typeof(TaskBehaviorHarvestingNew))
        {
            Format = "Harvest {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public PlanDef Digging = new("Digging", typeof(TaskBehaviorDigging), InteractionDefOf.Dig)
        {
            Format = "Dig {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public PlanDef SleepingOnGround = new("SleepingOnGround", typeof(TaskBehaviorSleepOnGround))
        {
            Format = "Sleep on ground",
            GetPrimaryTarget = t => TargetArgs.Null
        };

        static public PlanDef SleepingOnBed = new("SleepingOnBed", typeof(TaskBehaviorSleepingNew))
        {
            Format = "Sleep on bed",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public PlanDef Chatting = new("Chatting", typeof(TaskBehaviorTalkToAboutTopic))
        {
            Format = "Chat",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public PlanDef PickUp = new("Picking Up", typeof(TaskBehaviorStoreInInventory))
        {
            Format = "Force equip {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public PlanDef Chop = new("Chopping Designated", typeof(PlanBehaviorInteraction), InteractionDefOf.Chop)
        {
            Format = "Chop down {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public readonly PlanDef Idle = new("Idleing", typeof(TaskBehaviorIdle)) { Idle = true };
        static public readonly PlanDef Wander = new("Wandering", typeof(TaskBehaviorWander)) { Idle = true };
        static public readonly PlanDef Depart = new("Departing", typeof(TaskBehaviorDepart));
        static public readonly PlanDef DropInventory = new("Dropping item from Inventory", typeof(TaskBehaviorDropInventoryItem));
        static public readonly PlanDef DropCarried = new("Dropping carried item", typeof(TaskBehaviorDropItem));
        static public readonly PlanDef Equip = new("Equipping", typeof(BehaviorEquipItemNew), InteractionDefOf.Equip);
        static public readonly PlanDef Unequip = new("Unquipping", typeof(BehaviorUnequip), InteractionDefOf.Unequip);
        static public readonly PlanDef GoHaul = new("Fetching", typeof(TaskBehaviorGoHaul), InteractionDefOf.Pick);
        static public readonly PlanDef GoPlace = new("Placing", typeof(TaskBehaviorGoPlace), InteractionDefOf.Place);
        static public readonly PlanDef Construct = new("Construct", typeof(TaskBehaviorGoConstruct), InteractionDefOf.Construct); 
        static PlanDefOf()
        {
            Def.Register(typeof(PlanDefOf));
        }
    }
}
