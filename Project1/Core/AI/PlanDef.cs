using Project1.Core.Base;
using Project1.Core.Interactions;
using System;

namespace Project1.Core.AI
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
            return string.Format(this.Format, this.GetPrimaryTarget(task).LabelReadable);
        }
        public string GetForceText(TargetArgs target)
        {
            return string.Format(this.Format, target.LabelReadable);
        }
    }
}
