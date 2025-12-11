using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorConstruct : BehaviorPerformTask
    {
        TargetArgs Construction { get { return this.Task.GetTarget(ConstructionsID); } }
        public const TargetIndex ConstructionsID = TargetIndex.A;

        protected override IEnumerable<Behavior> GetSteps()
        {
            var manager = this.Actor.Map.Town.ConstructionsManager;
            var actor = this.Actor;
            var map = actor.Map;
            
            bool endCond()
            {
                var constrTar = Construction;
                var result = constrTar.Type == TargetType.Null || !manager.IsBuildableCurrently(constrTar.Global);
                return result;
            }
            this.FailOn(endCond);

            bool designationOccupied()
            {
                if (map.GetObjects(Construction.Global).Any(o => o != actor))
                {
                    return true;
                }
                return false;
            }
            yield return new BehaviorGrabTool();
            yield return new BehaviorGetAtNewNew(ConstructionsID).FailOn(designationOccupied);
            yield return new BehaviorInteractionNew(ConstructionsID, new Interactions.InteractionConstruct()).FailOn(designationOccupied);
        }

        protected override bool InitExtraReservations()
        {
            var task = this.Task;
            var actor = this.Actor;
            var map = actor.Map;

            var target = task.GetTarget(ConstructionsID);
            var entity = target.BlockEntity as IConstructible;
            foreach (var child in entity.Children)
            {
                if(!actor.Reserve(task, new TargetArgs(map, child), 1))
                {
                    map.Net.ConsoleBox.Write("failed to reserve child of multi-blocked construction");
                    return false;
                }
            }
            return true;
        }
    }
}
