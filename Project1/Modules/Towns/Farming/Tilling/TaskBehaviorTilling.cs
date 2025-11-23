using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_
{
    class TaskBehaviorTilling : BehaviorPerformTask
    {
        public const TargetIndex TargetInd = TargetIndex.A;
        TargetArgs Target { get { return this.Task.GetTarget(TargetInd); } }
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var map = actor.Map;
            var town = map.Town;
            this.FailOn(failOnInvalidTarget);
            yield return new BehaviorGrabTool().FailOnForbidden(TargetIndex.Tool);
            yield return new BehaviorGetAtNewNew(TargetInd);
            yield return new BehaviorInteractionNew(TargetInd, new InteractionTilling());
            bool failOnInvalidTarget()
            {
                var zone = town.ZoneManager.GetZoneAt<GrowingZone>(Target.Global); // capture zone outside method? and check if it still exists?
                return !zone?.IsValidTilling(Target.Global) ?? true;
            }
        }
        protected override bool InitExtraReservations()
        {
            return this.Actor.Reserve(this.Task.TargetA, 1);
        }
    }
}
