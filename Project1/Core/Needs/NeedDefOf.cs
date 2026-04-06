using Project1.Framework;
using Project1.Core.AI.Planners;
using Project1.Core.Systems.Conversations;

namespace Project1.Core.Needs
{
    [EnsureStaticCtorCall]
    public static class NeedDefOf
    {
        static public readonly NeedDef Comfort = new("Comfort", typeof(NeedComfortWorker))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological,
            BaseValue = 50,
            DecayTicksPerUnit = 1f / Ticks.FromMinutes(10)
        };
        static public readonly NeedDef Hunger = new("Hunger", typeof(NeedHungerWorker))
        {
            Planner = PlannerDefOf.Eating,
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological,
            DecayTicksPerUnit = 1f / Ticks.FromMinutes(10)
        };
        static public readonly NeedDef Energy = new("Energy", typeof(NeedEnergyWorker))
        {
            Planner = PlannerDefOf.Sleeping,
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological,
            DecayTicksPerUnit = 1f / Ticks.FromMinutes(10)
        };
        static public readonly NeedDef Work = new("Work", typeof(NeedWorkWorker))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryEsteem,
            Planners = [PlannerDefOf.Crafting, PlannerDefOf.Hauling],
            DecayTicksPerUnit = 1f / Ticks.FromMinutes(10)
        };
        static public readonly NeedDef Social = new("Social", typeof(NeedSocialWorker))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryRelationships,
            Planner = ConversationDefOf.PlannerConvo,
            DecayTicksPerUnit = 1f / Ticks.FromMinutes(10)
        };

        static public readonly NeedDef Curiosity = new("Curiosity", typeof(NeedCuriosityWorker))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryCognitive,
            DecayTicksPerUnit = 1f / Ticks.FromMinutes(10)
        };
        static NeedDefOf()
        {
            Def.Register(typeof(NeedDefOf));
        }
    }
}
