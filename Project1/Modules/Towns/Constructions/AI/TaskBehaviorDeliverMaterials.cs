using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorDeliverMaterials : BehaviorPerformTask
    {
        TargetArgs Material { get { return this.Task.GetTarget(MaterialID); } }
        TargetArgs Destination { get { return this.Task.GetTarget(DestinationID); } }
        public const TargetIndex MaterialID = TargetIndex.A, DestinationID = TargetIndex.B;
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var map = actor.Map;
            var town = map.Town;

            this.FailOnForbidden(MaterialID);
            var extractMaterial = BehaviorHelper.ExtractNextTargetAmount(MaterialID);
            yield return extractMaterial;
            yield return new BehaviorGetAtNewNew(MaterialID).FailOn(collectFail);
            yield return BehaviorHaulHelper.StartCarrying(this, MaterialID).FailOn(collectFail);
            yield return BehaviorHelper.JumpIfNextCarryStackable(extractMaterial, MaterialID, MaterialID);
            var extractDestination = BehaviorHelper.ExtractNextTargetAmount(DestinationID);
            yield return extractDestination;
            var gotoStorage = new BehaviorGetAtNewNew(DestinationID).FailOn(deliverFail);
            yield return gotoStorage;
            yield return new BehaviorInteractionNew(DestinationID, () => new UseHauledOnTarget(this.Actor.CurrentTask.GetAmount(DestinationID))
            ).FailOn(deliverFail);
            yield return BehaviorHelper.JumpIfMoreTargets(extractDestination, DestinationID);

            bool collectFail()
            {
                var o = Material.Object;
                foreach (var d in this.Task.GetTargetQueue(DestinationID))
                    if (!d.IsValidHaulDestinationNew(map, Material.Object))
                    {
                        "failed collecting".ToConsole();
                        return true;
                    }
                return false;
            }
            bool deliverFail()
            {
                var o = actor.Hauled;
                if (o == null)
                    return true;
                if (!this.Destination.IsValidHaulDestinationNew(map, o))
                {
                    "invalid haul destination".ToConsole();
                    return true;
                }
                return false;
            }
        }
        protected override bool InitExtraReservations()
        {
            return
                this.ReserveAll(MaterialID) &&
                //this.Task.GetTargetQueue(DestinationID).All(t => this.Actor.Reserve(this.Task, t, 1));
                this.Task.GetTargetQueue(DestinationID).All(t => this.Reserve(t, 1));
        }
    }
}
