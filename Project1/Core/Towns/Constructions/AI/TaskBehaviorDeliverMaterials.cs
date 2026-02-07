using System.Collections.Generic;
using System.Linq;
using Project1.Core.Interactions;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Base;
using Project1.Core.AI.Behaviors.Helpers;
using Project1.Core.AI.Behaviors;
using Project1.Core.Towns.Stockpiles;
using Project1.Core.Helpers;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Towns.Constructions.AI
{
    class TaskBehaviorDeliverMaterials : BehaviorExecutePlan
    {
        TargetArgs Material { get { return this.Plan.GetTarget(MaterialID); } }
        TargetArgs Destination { get { return this.Plan.GetTarget(DestinationID); } }
        public const TargetIndex MaterialID = TargetIndex.A, DestinationID = TargetIndex.B;
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var map = actor.Map;
            var town = map.Town;

            this.FailOnForbidden(MaterialID);
            var extractMaterial = BehaviorHelper.ExtractNextTargetAmount(MaterialID);
            yield return extractMaterial;
            yield return new BehaviorResolvePath(MaterialID).FailOn(collectFail);
            yield return BehaviorHaulHelper.StartCarrying(this, MaterialID).FailOn(collectFail);
            yield return BehaviorHelper.JumpIfNextCarryStackable(extractMaterial, MaterialID, MaterialID);
            var extractDestination = BehaviorHelper.ExtractNextTargetAmount(DestinationID);
            yield return extractDestination;
            var gotoStorage = new BehaviorResolvePath(DestinationID).FailOn(deliverFail);
            yield return gotoStorage;
            yield return new BehaviorResolveInteraction(DestinationID, () => new UseHauledOnTarget(this.Actor.CurrentTask.GetAmount(DestinationID))
            ).FailOn(deliverFail);
            yield return BehaviorHelper.JumpIfMoreTargets(extractDestination, DestinationID);

            bool collectFail()
            {
                var o = Material.Object;
                foreach (var d in this.Plan.GetTargetQueue(DestinationID))
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
        protected override bool ReserveExtra()
        {
            return
                this.ReserveAll(MaterialID) &&
                //this.Task.GetTargetQueue(DestinationID).All(t => this.Actor.Reserve(this.Task, t, 1));
                this.Plan.GetTargetQueue(DestinationID).All(t => this.Reserve(t, 1));
        }
    }
}
