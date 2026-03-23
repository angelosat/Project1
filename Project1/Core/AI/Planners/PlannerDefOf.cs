using Project1.Core.AI.Behaviors.Eating;
using Project1.Core.AI.Behaviors.Idle;
using Project1.Core.AI.Behaviors.Sleeping;
using Project1.Core.Quests.AI;
using Project1.Core.Systems.Plants;
using Project1.Core.Towns;
using Project1.Core.Towns.AI.Behaviors;
using Project1.Core.Towns.Constructions.AI;
using Project1.Core.Towns.Digging;
using Project1.Core.Towns.Farming;
using Project1.Core.Towns.Forestry;
using Project1.Core.Towns.Shops;
using Project1.Core.Towns.Switch;
using Project1.Core.Towns.Tasks;

namespace Project1.Core.AI.Planners
{
    static class PlannerDefOf
    {
        static public readonly PlannerDef Eating = new("Eating", typeof(PlannerEating));
        static public readonly PlannerDef Hauling = new("Hauling", typeof(PlannerHauling));
        static public readonly PlannerDef Crafting = new("Crafting", typeof(PlannerCrafting));
        static public readonly PlannerDef Building = new("Building", typeof(PlannerBuilding));
        static public readonly PlannerDef Deconstructing = new("Deconstructing", typeof(PlannerDeconstruct));
        static public readonly PlannerDef Chopping = new("Chopping", typeof(PlannerChopping));
        static public readonly PlannerDef Tilling = new("Tilling", typeof(PlannerTilling));
        static public readonly PlannerDef Sowing = new("Sowing", typeof(PlannerPlanting));
        static public readonly PlannerDef Harvesting = new("Harvesting", typeof(PlannerHarvesting));
        static public readonly PlannerDef Foraging = new("Foraging", typeof(PlannerForaging));
        static public readonly PlannerDef Digging = new("Digging", typeof(PlannerDigging));
        static public readonly PlannerDef Sleeping = new("Sleeping", typeof(PlannerSleeping));
        static public readonly PlannerDef Refueling = new("Refueling", typeof(PlannerRefueling));
        static public readonly PlannerDef Inventory = new("Inventory", typeof(PlannerInventory));
        static public readonly PlannerDef QuestGiving = new("QuestGiving", typeof(PlannerQuests));
        static public readonly PlannerDef Switching = new("Switching", typeof(PlannerToggleSwitch));
        static public readonly PlannerDef Workplace = new("Workplace", typeof(PlannerWorkplace));
        static public readonly PlannerDef SmartEquip = new("SmartEquip", typeof(PlannerSmartEquip));
        static public readonly PlannerDef Idle = new("Idle", typeof(PlannerIdle));

        static public readonly PlannerDef Buy = new("Buy", typeof(PlannerBuy));
        static public readonly PlannerDef Sell = new("Sell", typeof(PlannerSell));
        static public readonly PlannerDef Departure = new("Departure", typeof(PlannerDeparture));

        static PlannerDefOf()
        {
            Def.Register(typeof(PlannerDefOf));
        }
    }
}
