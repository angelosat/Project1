using Start_a_Town_.AI;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public static class NeedDefOf
    {
        static public readonly NeedDef Comfort = new("Comfort", typeof(NeedComfortWorker))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological,
            //Worker = new NeedComfortWorker(),
            //BaseDecayRate = 0,
            BaseValue = 50
        };
        static public readonly NeedDef Hunger = new("Hunger", typeof(NeedHungerWorker))
        {
            TaskGiver = new TaskGiverEat(),
            //Worker = new NeedHungerWorker(),
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological
        };
        static public readonly NeedDef Energy = new("Energy", typeof(NeedEnergyWorker))
        {
            TaskGiver = new TaskGiverSleeping(),
            //Worker = new NeedEnergyWorker(),
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological,
        };
        static public readonly NeedDef Work = new("Work", typeof(NeedWorkWorker))
        {
            //Worker = new NeedWorkWorker(),
            CategoryDef = NeedCategoryDef.NeedCategoryEsteem,
            TaskGivers = [new CraftingPlanner(), new HaulingPlanner()]
        };
        static public readonly NeedDef Social = new("Social", typeof(NeedSocialWorker))
        {
            //Worker = new NeedSocialWorker(),
            CategoryDef = NeedCategoryDef.NeedCategoryRelationships
        };

        static public readonly NeedDef Curiosity = new("Curiosity", typeof(NeedCuriosityWorker))
        {
            //Worker = new NeedCuriosityWorker(),
            CategoryDef = NeedCategoryDef.NeedCategoryCognitive
        };

  

        static NeedDefOf()
        {
            Def.Register(typeof(NeedDefOf));
        }
    }
}
