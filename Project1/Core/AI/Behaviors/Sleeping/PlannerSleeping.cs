using Project1.Core.Needs;
using Project1.Core.Rooms;
using Project1.Core.Towns;
using Project1.Core.Entities.Actors;
using Project1.Core.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Framework;

namespace Project1.Core.AI.Behaviors.Sleeping
{
    class PlannerSleeping : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var map = actor.Map;
            var need = actor.GetNeed(NeedDefOf.Energy);
            var energyValue = need.Value;

            if (energyValue > 50)// need.Threshold)
                return null;

            //var possibleBeds = actor.Possessions.GetOwned<BlockBedEntity>();
            //if (!possibleBeds.Any())
            //    possibleBeds = map.GetBlockEntities<BlockBedEntity>().Where(b => b.Owner is null);// FindOrClaimBedNew(actor);
            var possibleBeds = actor.Map.BlockEntities.Where(e => e.HasComp<BlockBedComp>() && actor.CanReserve(e));
            foreach (var bed in possibleBeds)
            {
                var cell = map.GetCell(bed.OriginGlobal);
                var interactionSpot = bed.OriginGlobal + cell.Block.GetInteractionSpotsLocal(map, bed.OriginGlobal, cell.Orientation).First();
                return new Plan(PlanDefOf.SleepingOnBed, new TargetArgs(map, bed.OriginGlobal)) { TargetB = new TargetArgs(bed) };//, bed);
            }
            if (energyValue <= 10)//0) 
                return new Plan(PlanDefOf.SleepingOnGround);

            return null;
        }
        protected Plan TryPlanOld(Actor actor)
        {
            var map = actor.Map;
            var need = actor.GetNeed(NeedDefOf.Energy);
            var energyValue = need.Value;

            if (energyValue > 99)// need.Threshold)
                return null;

            var possibleBeds = actor.Possessions.GetOwned<BlockBedEntity>();
            if (!possibleBeds.Any())
                possibleBeds = map.GetBlockEntities<BlockBedEntity>().Where(b => b.Owner is null);// FindOrClaimBedNew(actor);
            foreach (var bed in possibleBeds)
            {
                // determine exact cell from which to operate bed
                var bedglobal = bed.OriginGlobal;
                if (!actor.CanReserve(bedglobal))
                    continue;
                var bedCell = map.GetCell(bedglobal);
                var operatingPositions = bedCell.GetInteractionSpots(map, bedglobal);
                foreach (var p in operatingPositions)
                {
                    if (!actor.CanReach(p))
                        continue;
                    if (!map.IsStandableIn(p))
                        continue;
                    // check to reserve operating position solid block below?
                    var operatingPosition = new TargetArgs(map, p);

                    var task = new Plan(PlanDefOf.SleepingOnBed, new TargetArgs(map, bedglobal), operatingPosition);//, bed);
                    return task;
                }
            }

            if (energyValue <= 10)//0) 
                return new Plan(PlanDefOf.SleepingOnGround);

            return null;
        }
        private static IEnumerable<TargetArgs> FindOrClaimBedNew(Actor actor)
        {
            var assignedBedrooms = actor.Map.Town.RoomManager.GetRoomsByOwner(actor).Where(r => r.HasRole(RoomRoleDefOf.Bedroom));
            var allbeds = actor.Map.Town.GetUtilities(Utility.Types.Sleeping);
            var bedsByRoomm = allbeds.Select<IntVec3, (IntVec3 bed, Room room)>(b => (b, actor.Town.RoomManager.GetRoomAt(b)));
            var bedsByRoom = bedsByRoomm.ToLookup(i => i.room);

            var availableBeds = assignedBedrooms.Any() ? assignedBedrooms.SelectMany(room => bedsByRoom[room]).Concat(bedsByRoom[null]) : bedsByRoom[null];
            foreach (var (bed, room) in availableBeds)
            {
                if (!actor.CanReserve(bed))
                    continue;
                if (!actor.CanOperate(bed))
                    continue;

                if ((room is Room r) // null check
                    && (r.Owner != actor || r.Workplace is not null))
                    throw new Exception();

                yield return new TargetArgs(actor.Map, bed);
            }
        }
    }

    //class TaskGiverSleeping : TaskGiver
    //{
    //    protected override AITask TryAssignTask(Actor actor)
    //    {
    //        var map = actor.Map;
    //        var need = actor.GetNeed(NeedDef.Energy);
    //        var energyValue = need.Value;

    //        if (energyValue > need.Threshold)
    //            return null;

    //        var possibleBeds = FindOrClaimBedNew(actor);

    //        foreach (var bed in possibleBeds)
    //        {
    //            // determine exact cell from which to operate bed
    //            var bedglobal = bed.Global;
    //            var bedCell = map.GetCell(bedglobal);
    //            var operatingPositions = bedCell.GetOperatingPositionsGlobal(bedglobal);
    //            foreach (var p in operatingPositions)
    //            {
    //                if (!actor.CanReach(p))
    //                    continue;
    //                if (!map.IsStandableIn(p))
    //                    continue;
    //                var operatingPosition = new TargetArgs(map, p);

    //                var task = new AITask(typeof(TaskBehaviorSleepingNew), bed, operatingPosition);//, bed);
    //                return task;
    //            }
    //        }

    //        if(energyValue == 0)
    //            return new AITask(TaskDefOf.SleepingOnGround);

    //        return null;
    //    }
    //    private static IEnumerable<TargetArgs> FindOrClaimBedNew(Actor actor)
    //    {
    //        var assignedBedrooms = actor.Map.Town.RoomManager.GetRoomsByOwner(actor).Where(r => r.HasRole(RoomRoleDefOf.Bedroom));
    //        var allbeds = actor.Map.Town.GetUtilities(Utility.Types.Sleeping);
    //        var bedsByRoomm = allbeds.Select<Vector3, (Vector3 bed, Room room)>(b => (b, actor.Town.RoomManager.GetRoomAt(b)));
    //        var bedsByRoom = bedsByRoomm.ToLookup(i => i.room);

    //        var availableBeds = assignedBedrooms.Any() ? assignedBedrooms.SelectMany(room => bedsByRoom[room]).Concat(bedsByRoom[null]) : bedsByRoom[null];
    //        foreach (var (bed, room) in availableBeds)
    //        {
    //            if (!actor.CanReserve(bed))
    //                continue;
    //            if (!actor.CanOperate(bed))
    //                continue;

    //            if ((room is Room r) // null check
    //                && (r.Owner != actor || r.Workplace is not null))
    //                throw new Exception();

    //            yield return new TargetArgs(actor.Map, bed);
    //        }
    //    }
    //}
}
