using Project1.Core.AI.Behaviors.Eating;
using Project1.Core.AI.Behaviors.Idle;
using Project1.Core.AI.Behaviors.Sleeping;
using Project1.Core.Systems.Plants;
using Project1.Core.Towns;
using Project1.Core.Towns.AI.Behaviors;
using Project1.Core.Towns.Constructions.AI;
using Project1.Core.Towns.Digging;
using Project1.Core.Towns.Farming;
using Project1.Core.Towns.Forestry;
using Project1.Core.Towns.Services.Healing;
using Project1.Core.Towns.Services.Inns;
using Project1.Core.Towns.Services.Shops;
using Project1.Core.Towns.Switch;
using Project1.Core.Towns.Tasks;

namespace Project1.Core.AI.Planners
{
    static class PlannerDefOf
    {
        static public readonly PlannerDef Eating = new("Eating", typeof(PlannerEating));
        static public readonly PlannerDef Hauling = new("Hauling", typeof(Planner_Hauling));
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
        static public readonly PlannerDef Restocking = new("Restocking", typeof(PlannerRestocking));
        static public readonly PlannerDef Withdraw = new("Withdrawing", typeof(PlannerWithdrawCashOverflow));
        static public readonly PlannerDef Inventory = new("Inventory", typeof(Planner_Inventory));
        static public readonly PlannerDef Switching = new("Switching", typeof(PlannerToggleSwitch));
        static public readonly PlannerDef Workplace = new("Workplace", typeof(PlannerWorkplace));
        static public readonly PlannerDef SmartEquip = new("SmartEquip", typeof(PlannerSmartEquip));
        static public readonly PlannerDef Idle = new("Idle", typeof(PlannerIdle));

        static public readonly PlannerDef LodgingCheckin = new("LodgingCheckin", typeof(Planner_Lodging_Customer));
        static public readonly PlannerDef LodgingRegister = new("LodgingRegister", typeof(Planner_Lodging_Vendor));
        static public readonly PlannerDef Browse = new("Browse", typeof(Planner_Shop_Browse));
        static public readonly PlannerDef SeekHealing = new("Healing", typeof(PlannerHealingSeek));
        static public readonly PlannerDef OfferHealing = new("Healer", typeof(PlannerHealingOffer));
        static public readonly PlannerDef Buy = new("Buy", typeof(Planner_Shop_Customer));
        static public readonly PlannerDef Sell = new("Sell", typeof(Planner_Shop_Vendor));
        static public readonly PlannerDef Departure = new("Departure", typeof(PlannerDeparture));

        static PlannerDefOf()
        {
            Def.Register(typeof(PlannerDefOf));
        }
    }
}
