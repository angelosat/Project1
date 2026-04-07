using Project1.Core.AI.Reservations;
using Project1.Core.Blocks;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.Rooms;
using Project1.Core.Towns;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI.Behaviors.Sleeping
{
    class PlannerSleeping : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            //if (!actor.IsTownMember)
            //    return null;
            var map = actor.Map;

            if (actor.Hauled is not null)
                return null;

            var need = actor.GetNeed(NeedDefOf.Energy);
            var energyValue = need.Value;

            if (energyValue > 50)// need.Threshold)
                return null;

            if(map.Town.Ownership.TryGetAssignedBed(actor, out var bedComp))
            {
                var bed = bedComp.Parent;
                if(actor.CanReachAndReserve(bed))
                    return new Plan(PlanDefOf.SleepingOnBed, new InteractionTarget(map, bed.OriginGlobal)) { TargetB = new InteractionTarget(bed) };
            }
            if (actor.IsTownMember)
            {
                //var possibleBeds = actor.Possessions.GetOwned<BlockBedEntity>();
                //if (!possibleBeds.Any())
                //    possibleBeds = map.GetBlockEntities<BlockBedEntity>().Where(b => b.Owner is null);// FindOrClaimBedNew(actor);
                var possibleBeds = actor.Map.BlockEntities.Where(e => e.HasComp<BlockBedComp>() && actor.CanReserve(e));
                foreach (var bed in possibleBeds)
                {
                    var cell = map.GetCell(bed.OriginGlobal);
                    //var interactionSpot = bed.OriginGlobal + cell.Block.GetInteractionSpotsLocal(map, bed.OriginGlobal, cell.Orientation).First();
                    return new Plan(PlanDefOf.SleepingOnBed, new InteractionTarget(map, bed.OriginGlobal)) { TargetB = new InteractionTarget(bed) };//, bed);
                }
            }

            if (energyValue <= 0)
                return new Plan(PlanDefOf.SleepingOnGround);

            return null;
        }
    }
}
