using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI.Behaviors.ItemOwnership;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public static class TaskDefOf
    {
        static public TaskDef Crafting = new("Crafting", typeof(TaskBehaviorCrafting))
        {
            Format = "Force crafting at {0}", 
            GetPrimaryTarget = t => t.GetTarget(TaskBehaviorCrafting.WorkstationIndex)
        };

        static public TaskDef Hauling = new("Hauling", typeof(TaskBehaviorHaulToStockpile))
        {
            Format = "Force haul {0}",
            GetPrimaryTarget = t => t.GetTarget(TaskBehaviorHaulToStockpile.ItemInd)
        };
        public static TaskDef Refueling = new("Refueling", typeof(TaskBehaviorRefueling))
        {
            Format = "Refuel {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public TaskDef HaulAside = new("HaulAside", typeof(TaskBehaviorHaulAside))
        {
            Format = "Haul aside {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef Construct = new("Construct", typeof(TaskBehaviorConstruct))
        {
            Format = "Construct {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef DeliverMaterials = new("DeliverMaterials", typeof(TaskBehaviorDeliverMaterials))
        {
            Format = "Deliver materials to {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef Sowing = new("Sowing", typeof(TaskBehaviorDeliverMaterials))
        {
            Format = "Sow {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public TaskDef Tilling = new("Tilling", typeof(TaskBehaviorTilling))
        {
            Format = "Till {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public TaskDef Moving = new("Moving", typeof(TaskBehaviorLeaveUnstandableCell))
        {
            Format = "Move {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef Harvesting = new("Harvesting", typeof(TaskBehaviorHarvestingNew))
        {
            Format = "Harvest {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef Digging = new("Digging", typeof(TaskBehaviorDigging))
        {
            Format = "Dig {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef SleepingOnGround = new("SleepingOnGround", typeof(TaskBehaviorSleepOnGround))
        {
            Format = "Sleep on ground",
            GetPrimaryTarget = t => TargetArgs.Null
        };

        static public TaskDef SleepingOnBed = new("SleepingOnBed", typeof(TaskBehaviorSleepingNew))
        {
            Format = "Sleep on bed",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef Chatting = new("Chatting", typeof(TaskBehaviorTalkToAboutTopic))
        {
            Format = "Chat",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public TaskDef PickUp = new("Picking Up", typeof(TaskBehaviorStoreInInventory))
        {
            Format = "Force equip {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public TaskDef Chopping = new("Chopping", typeof(TaskBehaviorChopping))
        {
            Format = "Chop down {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public TaskDef Idle = new("Idleing", typeof(TaskBehaviorIdle)) { Idle = true };
        static public TaskDef Wander = new("Wandering", typeof(TaskBehaviorWander)) { Idle = true };
        static public TaskDef Depart = new("Departing", typeof(TaskBehaviorDepart));
        static public TaskDef DropInventory = new("Dropping item from Inventory", typeof(TaskBehaviorDropInventoryItem));
        static public TaskDef DropCarried = new("Dropping carried item", typeof(TaskBehaviorDropItem));
        static public TaskDef Equip = new("Equipping", typeof(BehaviorEquipItemNew));
        static public TaskDef Unequip = new("Unquipping", typeof(BehaviorUnequip));
        static TaskDefOf()
        {
            Def.Register(typeof(TaskDefOf));
        }
    }
}
