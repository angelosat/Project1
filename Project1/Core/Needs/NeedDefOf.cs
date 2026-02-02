using Project1.Framework.Needs;
using Project1.Framework.Needs.Types;
using Start_a_Town_;

namespace Project1.Core.Needs
{
    [EnsureStaticCtorCall]
    public static class NeedDefOf
    {
        static public readonly NeedDef Comfort = new("Comfort", typeof(NeedComfortWorker))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological,
            BaseValue = 50
        };
        static public readonly NeedDef Hunger = new("Hunger", typeof(NeedHungerWorker))
        {
            Planner = PlannerDefOf.Eating,
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological
        };
        static public readonly NeedDef Energy = new("Energy", typeof(NeedEnergyWorker))
        {
            Planner = PlannerDefOf.Sleeping,
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological,
        };
        static public readonly NeedDef Work = new("Work", typeof(NeedWorkWorker))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryEsteem,
            Planners = [PlannerDefOf.Crafting, PlannerDefOf.Hauling]
        };
        static public readonly NeedDef Social = new("Social", typeof(NeedSocialWorker))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryRelationships
        };

        static public readonly NeedDef Curiosity = new("Curiosity", typeof(NeedCuriosityWorker))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryCognitive
        };
        static NeedDefOf()
        {
            Def.Register(typeof(NeedDefOf));
        }
    }
}
