using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI.Behaviors.ItemOwnership;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public static class PlanDefOf
    {

        static public readonly PlanDef Hauling = new("Hauling", typeof(TaskBehaviorHaulToStockpile))
        {
            Format = "Force haul {0}",
            GetPrimaryTarget = t => t.GetTarget(TaskBehaviorHaulToStockpile.ItemInd)
        };
        static public readonly PlanDef Refueling = new("Refueling", typeof(TaskBehaviorRefueling))
        {
            Format = "Refuel {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public readonly PlanDef HaulAside = new("HaulAside", typeof(TaskBehaviorHaulAside))
        {
            Format = "Haul aside {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        //static public PlanDef Construct = new("Construct", typeof(TaskBehaviorConstruct))
        //{
        //    Format = "Construct {0}",
        //    GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        //};

        static public readonly PlanDef DeliverMaterials = new("DeliverMaterials", typeof(TaskBehaviorDeliverMaterials))
        {
            Format = "Deliver materials to {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public readonly PlanDef Sowing = new("Sowing", typeof(TaskBehaviorDeliverMaterials))
        {
            Format = "Sow {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };
        static public readonly PlanDef Moving = new("Moving", typeof(TaskBehaviorLeaveUnstandableCell))
        {
            Format = "Move {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public readonly PlanDef Harvesting = new("Harvesting", typeof(TaskBehaviorHarvestingNew))
        {
            Format = "Harvest {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public readonly PlanDef Digging = new("Digging", typeof(TaskBehaviorDigging), InteractionDefOf.Dig)
        {
            Format = "Dig {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public readonly PlanDef SleepingOnGround = new("SleepingOnGround", typeof(TaskBehaviorSleepOnGround))
        {
            Format = "Sleep on ground",
            GetPrimaryTarget = t => TargetArgs.Null
        };

        static public readonly PlanDef SleepingOnBed = new("SleepingOnBed", typeof(TaskBehaviorSleepingNew))
        {
            Format = "Sleep on bed",
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
        static public readonly PlanDef Chop = new("Chopping Designated", typeof(PlanBehaviorInteraction), InteractionDefOf.Chop)
        {
            Format = "Chop down {0}",
            GetPrimaryTarget = t => t.GetTarget(TargetIndex.A)
        };

        static public readonly PlanDef Idle = new("Idleing", typeof(TaskBehaviorIdle)) { Idle = true };
        static public readonly PlanDef Wander = new("Wandering", typeof(TaskBehaviorWander)) { Idle = true };
        static public readonly PlanDef Depart = new("Departing", typeof(TaskBehaviorDepart), InteractionDefOf.Depart);
        static public readonly PlanDef DropInventory = new("Dropping item from Inventory", typeof(TaskBehaviorDropInventoryItem));
        static public readonly PlanDef DropCarried = new("Dropping carried item", typeof(TaskBehaviorDropItem), InteractionDefOf.Drop);
        static public readonly PlanDef Equip = new("Equipping", typeof(BehaviorEquipItemNew), InteractionDefOf.Equip);
        static public readonly PlanDef Unequip = new("Unquipping", typeof(BehaviorUnequip), InteractionDefOf.Unequip);
        static public readonly PlanDef GoHaul = new("Fetching", typeof(TaskBehaviorGoHaul), InteractionDefOf.Pick);
        static public readonly PlanDef GoPlace = new("Placing", typeof(TaskBehaviorGoPlace), InteractionDefOf.Place);
        static public readonly PlanDef Construct = new("Constructing", typeof(TaskBehaviorGoConstruct), InteractionDefOf.Construct);
        static public readonly PlanDef GoTill = new("Tilling", typeof(TaskBehaviorTilling), InteractionDefOf.Till);
        static public readonly PlanDef Crafting = new("Crafting", typeof(TaskBehaviorGoCraft), InteractionDefOf.Craft);

        static PlanDefOf()
        {
            Def.Register(typeof(PlanDefOf));
        }
    }
}
