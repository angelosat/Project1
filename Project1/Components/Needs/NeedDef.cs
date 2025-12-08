using System;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    public class NeedDef : Def
    {
        public float BaseThreshold = 20;
        public float BaseDecayRate = .1f; // measure decay rate in ticks? how many ticks to drop value by 1
        public float BaseValue = 100;
        public TaskGiver TaskGiver;
        public NeedCategoryDef CategoryDef;
        public NeedWorker Worker;

        public NeedDef(string name, Type needType, NeedCategoryDef category = null) : base(name)
        {
            //this.Type = needType;
            this.Worker = Activator.CreateInstance(needType) as NeedWorker;
            this.CategoryDef = category;
        }
    }
    [EnsureStaticCtorCall]
    public static class NeedDefOf
    {
        static public readonly NeedDef Comfort = new("Comfort", typeof(NeedComfortWorker))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological,
            //Worker = new NeedComfortWorker(),
            BaseDecayRate = 0,
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
            CategoryDef = NeedCategoryDef.NeedCategoryEsteem
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
