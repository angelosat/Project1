using Project1.Core.AI.Planners;
using Project1.Core.Base;
using System;

namespace Project1.Core.Needs
{
    public class NeedDef : Def
    {
        public float BaseThreshold = 20;
        //public float BaseDecayRate = .1f; // measure decay rate in ticks? how many ticks to drop value by 1
        public float BaseValue = 100;
        public float BaseRate = 1;
        public PlannerDef Planner;
        public NeedCategoryDef CategoryDef;
        public NeedWorker Worker;
        public PlannerDef[] Planners = [];

        public NeedDef(string name, Type needType, NeedCategoryDef category = null) : base(name)
        {
            //this.Type = needType;
            this.Worker = Activator.CreateInstance(needType) as NeedWorker;
            this.CategoryDef = category;
        }
    }
}
