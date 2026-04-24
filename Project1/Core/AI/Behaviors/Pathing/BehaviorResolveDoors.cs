using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Blocks.Doors;
using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI.Behaviors.Pathing;

class BehaviorResolveDoors : Behavior
{
    readonly HashSet<IntVec3> OpenedDoors = [];
    readonly Dictionary<IntVec3, BlockDoorComp> CellsToComps = [];

    public override BehaviorState Tick(Actor parent, AIState state)
    {
        var actorBox = parent.Physics.NextAABB;
        foreach (var door in this.OpenedDoors.ToArray())
        {
            var doorBox = this.CellsToComps[door].AABB;
            if (!actorBox.Intersects(doorBox))
            {
                this.OpenedDoors.Remove(door);
                var comp = this.CellsToComps[door];
                comp.OnActorExited(parent);
                this.CellsToComps.Remove(door);
                if(comp.CanClose())
                    parent.Work.Perform(InteractionDefOf.ToggleDoor, new InteractionTarget(parent.Map, door));
            }
        }
        return HandleByCorners(parent);
    }
    private BehaviorState HandleByCorners(Actor parent)
    {
        // THE CHECKS TO OPEN OR CLOSE DOOR MUST BE THE SAME
        var corners = parent.Physics.NextAABB.GetCorners();
        var map = parent.Map;
        var occupiedCells = corners.Select(c => c.ToCell()).Distinct();
        foreach (var cellVec in occupiedCells)
        {
            var cell = map.GetCell(cellVec);
            if (cell is null)
                continue;
            var door = Cell.GetOrigin(map, cellVec);
            var cellOrigin = map.GetCell(door);
            if (cellOrigin == null) // why check this? is actor at the edge of map? departing?
                continue;
            if (cellOrigin.Block is not BlockDoor)
                continue;
            if (this.OpenedDoors.Contains(door))
                continue;
            var (open, locked) = BlockDoor.GetState(cellOrigin.BlockData);
            var doorComp = map.GetBlockComp<BlockDoorComp>(cellVec);
            this.OpenedDoors.Add(door);
            this.CellsToComps.Add(door, doorComp);
            doorComp.OnActorEntered(parent);
            if (!open)
                parent.Work.Perform(InteractionDefOf.ToggleDoor, new InteractionTarget(parent.Map, door));
            return BehaviorState.Fail;
        }
        return BehaviorState.Fail;
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }

    protected override void AddSaveData(SaveTag tag)
    {
        this.OpenedDoors.Save(tag, "OpenedDoors");
    }
    internal override void Load(SaveTag tag)
    {
        this.OpenedDoors.Load(tag, "OpenedDoors");
    }
}
