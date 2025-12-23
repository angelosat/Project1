using System;

namespace Start_a_Town_
{
    public class NeedDef : Def
    {
        public float BaseThreshold = 20;
        public float BaseDecayRate = .1f; // measure decay rate in ticks? how many ticks to drop value by 1
        public float BaseValue = 100;
        public Planner TaskGiver;
        public NeedCategoryDef CategoryDef;
        public NeedWorker Worker;
        public Planner[] TaskGivers = [];

        public NeedDef(string name, Type needType, NeedCategoryDef category = null) : base(name)
        {
            //this.Type = needType;
            this.Worker = Activator.CreateInstance(needType) as NeedWorker;
            this.CategoryDef = category;
        }
    }
}
