using Project1.Core.Interactions;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using Project1.Framework.WorldGen;
using Start_a_Town_;
using Start_a_Town_.AI;
using Start_a_Town_.Framework.AI.NodeTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI.Behaviors.Pathing
{
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
                        parent.Work.Perform(InteractionDefOf.ToggleDoor, new TargetArgs(parent.Map, door));
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
                var doorComp = map.GetBlockEntityComp<BlockDoorComp>(cellVec);
                this.OpenedDoors.Add(door);
                this.CellsToComps.Add(door, doorComp);
                doorComp.OnActorEntered(parent);
                if (!open)
                    parent.Work.Perform(InteractionDefOf.ToggleDoor, new TargetArgs(parent.Map, door));
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
}
