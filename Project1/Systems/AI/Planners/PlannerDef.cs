using Project1.Core.Plants;
using Project1.Core.Quests.AI;
using Project1.Core.Tavern;
using Project1.Core.Towns.AI.Behaviors;
using Project1.Framework.Base;
using Start_a_Town_.AI;
using System;

namespace Start_a_Town_
{
    public class PlannerDef(string name, Type workerType) : Def(name)
    {
        public Planner Worker = ActivatorSafe<Planner>.CreateInstance(workerType);
    }

    static class PlannerDefOf
    {
        static public PlannerDef Eating = new("Eating", typeof(EatingPlanner));
        static public PlannerDef Hauling = new("Hauling", typeof(HaulingPlanner));
        static public PlannerDef Crafting = new("Crafting", typeof(CraftingPlanner));
        static public PlannerDef Building = new("Building", typeof(BuildingPlanner));
        static public PlannerDef Deconstructing = new("Deconstructing", typeof(TaskGiverDeconstruct));
        static public PlannerDef Chopping = new("Chopping", typeof(ChoppingPlanner));
        static public PlannerDef Tilling = new("Tilling", typeof(TillingPlanner));
        static public PlannerDef Sowing = new("Sowing", typeof(SowingPlanner));
        static public PlannerDef Harvesting = new("Harvesting", typeof(HarvestingPlanner));
        static public PlannerDef Foraging = new("Foraging", typeof(TaskGiverForaging));
        static public PlannerDef Digging = new("Digging", typeof(DiggingPlanner));
        static public PlannerDef Departure = new("Departure", typeof(DeparturePlanner));
        static public PlannerDef Sleeping = new("Sleeping", typeof(SleepingPlanner));
        static public PlannerDef Refueling = new("Refueling", typeof(RefuelingPlanner));
        static public PlannerDef Inventory = new("Inventory", typeof(InventoryPlanner));
        static public PlannerDef QuestGiving = new("QuestGiving", typeof(TaskGiverOfferQuest));
        static public PlannerDef Switching = new("Switching", typeof(TaskGiverSwitchToggle));
        static public PlannerDef Workplace = new("Workplace", typeof(TaskGiverWorkplace));
        static public PlannerDef SmartEquip = new("SmartEquip", typeof(SmartEquipPlanner));
        static public PlannerDef Idle = new("Idle", typeof(TaskGiverIdle));
        static PlannerDefOf()
        {
            Def.Register(typeof(PlannerDefOf));
        }
    }
}
