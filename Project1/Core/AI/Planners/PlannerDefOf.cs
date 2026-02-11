using Project1.Core.AI.Behaviors.Eating;
using Project1.Core.AI.Behaviors.Idle;
using Project1.Core.AI.Behaviors.Sleeping;
using Project1.Core.Plants;
using Project1.Core.Quests.AI;
using Project1.Core.Towns.AI.Behaviors;
using Project1.Core.Towns.Constructions.AI;
using Project1.Core.Towns.Labors;
using Project1.Core.Towns.Switch;
using Project1.Core.Towns.Tasks;
using Project1.Core.Towns;
using Project1.Core.Towns.Digging;
using Project1.Core.Towns.Farming;
using Project1.Core.Towns.Forestry;

namespace Project1.Core.AI.Planners
{
    static class PlannerDefOf
    {
        static public PlannerDef Eating = new("Eating", typeof(PlannerEating));
        static public PlannerDef Hauling = new("Hauling", typeof(PlannerHauling));
        static public PlannerDef Crafting = new("Crafting", typeof(PlannerCrafting));
        static public PlannerDef Building = new("Building", typeof(PlannerBuilding));
        static public PlannerDef Deconstructing = new("Deconstructing", typeof(DeconstructPlanner));
        static public PlannerDef Chopping = new("Chopping", typeof(PlannerChopping));
        static public PlannerDef Tilling = new("Tilling", typeof(PlannerTilling));
        static public PlannerDef Sowing = new("Sowing", typeof(PlannerSowing));
        static public PlannerDef Harvesting = new("Harvesting", typeof(PlannerHarvesting));
        static public PlannerDef Foraging = new("Foraging", typeof(PlannerForaging));
        static public PlannerDef Digging = new("Digging", typeof(PlannerDigging));
        static public PlannerDef Departure = new("Departure", typeof(PlannerDeparture));
        static public PlannerDef Sleeping = new("Sleeping", typeof(PlannerSleeping));
        static public PlannerDef Refueling = new("Refueling", typeof(PlannerRefueling));
        static public PlannerDef Inventory = new("Inventory", typeof(PlannerInventory));
        static public PlannerDef QuestGiving = new("QuestGiving", typeof(PlannerQuests));
        static public PlannerDef Switching = new("Switching", typeof(PlannerToggleSwitch));
        static public PlannerDef Workplace = new("Workplace", typeof(PlannerWorkplace));
        static public PlannerDef SmartEquip = new("SmartEquip", typeof(PlannerSmartEquip));
        static public PlannerDef Idle = new("Idle", typeof(PlannerIdle));
        static PlannerDefOf()
        {
            Def.Register(typeof(PlannerDefOf));
        }
    }
}
