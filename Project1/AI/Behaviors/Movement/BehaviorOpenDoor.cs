using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorOpenDoor : Behavior
    {
        readonly HashSet<IntVec3> OpenedDoors = new();

        public override BehaviorState Tick(Actor parent, AIState state)
        {
            foreach(var door in this.OpenedDoors.ToArray())
            { 
                if (!parent.Intersects(door))
                {
                    // if the actor's boundingbox doesn't intersect the doors boundingdoor, close the door UNLESS IT'S OBSTRUCTED by another entity
                    var allobj = parent.Map.GetObjectsLazy();
                    var intersecting = allobj.Where(o => o.Intersects(door)); // TODO not very fast
                    if (!intersecting.Any())   // if the door is obstructed, leave it open
                        parent.Interact(new InteractionToggleDoor(), door);
                    this.OpenedDoors.Remove(door);
                }
            }
            return HandleByCorners(parent);
        }
        private BehaviorState HandleByCorners(Actor parent)
        {
            // THE CHECKS TO OPEN OR CLOSE DOOR MUST BE THE SAME
            var corners = parent.GetBoundingBoxCorners(parent.Global + parent.Velocity);
            var map = parent.Map;
            var occupiedCells = corners.Select(c => c.ToCell()).Distinct();
            foreach (var cellVec in occupiedCells)
            {
                var door = Cell.GetOrigin(map, cellVec);
                var cell = map.GetCell(door);
                if (cell == null) // why check this? is actor at the edge of map? departing?
                    continue;
                if (cell.Block is not BlockDoor)
                    continue;
                if (this.OpenedDoors.Contains(door))
                    continue;
                var (open, locked) = BlockDoor.GetState(cell.BlockData);
                this.OpenedDoors.Add(door);
                if (!open)
                {
                    var openInteraction = new InteractionToggleDoor();
                    parent.Interact(openInteraction, door);
                }
                return BehaviorState.Fail;
            }
            return BehaviorState.Fail;
        }

        [Obsolete]
        private BehaviorState HandleByNextStep(Actor parent)
        {
            var map = parent.Map;
            var door = parent.NextCell;
            var cell = map.GetCell(door);
            if (cell == null) // why check this? is actor at the edge of map? departing?
                return BehaviorState.Fail;
            if (cell.Block is not BlockDoor)
                return BehaviorState.Fail;
            if (this.OpenedDoors.Contains(door))
                return BehaviorState.Fail;
            var (open, locked) = BlockDoor.GetState(cell.BlockData);
            this.OpenedDoors.Add(door);
            if (open) // IF IT'S ALREADY OPEN, STORE IT ANYWAY SO THAT THE ACTOR CLOSES IT WHEN HE LEAVES THE CELL
                return BehaviorState.Fail;

            var openInteraction = new InteractionToggleDoor();
            parent.Interact(openInteraction, door);
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

    //class BehaviorOpenDoor : Behavior
    //{
    //    IntVec3? CurrentOpenDoor;

    //    public override BehaviorState Execute(Actor parent, AIState state)
    //    {
    //        if (this.CurrentOpenDoor.HasValue)
    //        {
    //            if (!parent.Intersects(this.CurrentOpenDoor.Value))
    //            {
    //                // if the actor's boundingbox doesn't intersect the doors boundingdoor, close the door UNLESS IT'S OBSTRUCTED by another entity
    //                var allobj = parent.Map.GetObjectsLazy();
    //                var intersecting = allobj.Where(o => o.Intersects(this.CurrentOpenDoor.Value)); // TODO not very fast
    //                if (!intersecting.Any())
    //                {
    //                    parent.Interact(new InteractionToggleDoor(), this.CurrentOpenDoor.Value);
    //                    $"closed door at {this.CurrentOpenDoor.Value}".ToConsole();
    //                }
    //                // if the door is obstructed, leave it open
    //                this.CurrentOpenDoor = null;
    //                return BehaviorState.Fail;
    //            }
    //        }
    //        return HandleByCorners(parent);
    //    }
    //    private BehaviorState HandleByCorners(Actor parent)
    //    {
    //        // THE CHECKS TO OPEN OR CLOSE DOOR MUST BE THE SAME
    //        var corners = parent.GetBoundingBoxCorners(parent.Global + parent.Velocity);
    //        var map = parent.Map;
    //        var occupiedCells = corners.Select(c => c.ToCell()).Distinct();
    //        foreach (var door in occupiedCells)
    //        {
    //            var cell = map.GetCell(door);
    //            if (cell == null) // why check this? is actor at the edge of map? departing?
    //                continue;
    //            if (cell.Block is not BlockDoor)
    //                continue;
    //            var (open, locked) = BlockDoor.GetState(cell.BlockData);
    //            if (door.Z != parent.Global.Z)
    //                "ti fasi".ToConsole();
    //            this.CurrentOpenDoor = door; // IF IT'S ALREADY OPEN, STORE IT ANYWAY SO THAT THE ACTOR CLOSES IT WHEN HE LEAVES THE CELL
    //            if (!open)
    //            {
    //                var openInteraction = new InteractionToggleDoor();
    //                parent.Interact(openInteraction, door);
    //            }
    //            return BehaviorState.Fail;
    //        }
    //        return BehaviorState.Fail;
    //    }

    //    [Obsolete]
    //    private BehaviorState HandleByNextStep(Actor parent)
    //    {
    //        var map = parent.Map;
    //        var door = parent.NextCell;
    //        var cell = map.GetCell(door);
    //        if (cell == null) // why check this? is actor at the edge of map? departing?
    //            return BehaviorState.Fail;
    //        if (cell.Block is not BlockDoor)
    //            return BehaviorState.Fail;
    //        var (open, locked) = BlockDoor.GetState(cell.BlockData);
    //        this.CurrentOpenDoor = door; // IF IT'S ALREADY OPEN, STORE IT ANYWAY SO THAT THE ACTOR CLOSES IT WHEN HE LEAVES THE CELL

    //        if (open) // IF IT'S ALREADY OPEN, STORE IT ANYWAY SO THAT THE ACTOR CLOSES IT WHEN HE LEAVES THE CELL
    //            return BehaviorState.Fail;

    //        var openInteraction = new InteractionToggleDoor();
    //        parent.Interact(openInteraction, door);
    //        return BehaviorState.Fail;
    //    }

    //    public override object Clone()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
