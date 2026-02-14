using Project1.Core.AI.Behaviors;

namespace Project1.Core.Plants
{
    internal class BehaviorPlanting : BehaviorExecutePlanNew
    {
        protected override bool ShouldAbort()
        {
            if (!this.Plan.IsDesignationStillValid(this.Actor.Map))
                return true;
            return false;
        }
    }
}
