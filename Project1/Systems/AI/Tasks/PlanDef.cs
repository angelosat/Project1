using Project1.Framework.Base;
using Project1.Framework.Interactions;
using System;

namespace Start_a_Town_
{
    public sealed class PlanDef : Def
    {
        public Type BehaviorClass;
        public string Format;
        public TargetIndex PrimaryTargetIndex;
        public Func<Plan, TargetArgs> GetPrimaryTarget;
        public bool Idle;
        internal InteractionDef Interaction;

        public PlanDef(string name, Type bhavClass, InteractionDef interaction = null) : base(name)
        {
            this.BehaviorClass = bhavClass;
            this.Interaction = interaction;
        }
        public string GetForceText(Plan task)
        {
            return string.Format(this.Format, this.GetPrimaryTarget(task).Label);
        }
        public string GetForceText(TargetArgs target)
        {
            return string.Format(this.Format, target.Label);
        }
    }
}
