using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Start_a_Town_.AI.Behaviors;
using Project1.Framework.Pathing;

namespace Start_a_Town_
{
    class TaskBehaviorTilling : BehaviorExecutePlan
    {
        public const TargetIndex TargetInd = TargetIndex.A;
        TargetArgs Target { get { return this.Plan.GetTarget(TargetInd); } }
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(PathEndMode.Touching)
                .FailOnInvalidInteraction(this.Actor, this.Plan);
            yield return new BehaviorResolveInteraction();
        }
        //protected IEnumerable<Behavior> GetStepsOld()
        //{
        //    var actor = this.Actor;
        //    var map = actor.Map;
        //    var town = map.Town;
        //    this.FailOn(failOnInvalidTarget);
        //    yield return new BehaviorGrabTool().FailOnForbidden(TargetIndex.Tool);
        //    yield return new BehaviorResolvePath(TargetInd);
        //    yield return new BehaviorResolveInteraction(TargetInd);
        //    bool failOnInvalidTarget()
        //    {
        //        var zone = town.ZoneManager.GetZoneAt<GrowingZone>(Target.Global); // capture zone outside method? and check if it still exists?
        //        return !zone?.IsValidTilling(Target.Global) ?? true;
        //    }
        //}
        protected override bool ReserveExtra()
        {
            return this.Reserve(this.Plan.TargetA, 1);
        }
    }
}
