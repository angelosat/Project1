using System.Collections.Generic;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.AI.Behaviors.Sleeping
{
    class TaskBehaviorSleepingNew : BehaviorExecutePlan
    {
        static public TargetIndex BedIndex = TargetIndex.A;
       
        public override string Name => "Sleeping";
     
        public TaskBehaviorSleepingNew()
        {

        }
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(PathEndMode.InteractionSpot);
            yield return new BehaviorResolveInteraction();
        }
      
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
        }
    }
}
