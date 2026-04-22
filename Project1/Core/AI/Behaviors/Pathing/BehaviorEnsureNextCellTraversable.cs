using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using System;
using System.Linq;

namespace Project1.Core.AI.Behaviors.Pathing;

class BehaviorEnsureNextCellTraversable : Behavior
{
    public override object Clone()
    {
        throw new NotImplementedException();
    }

    public override BehaviorState Tick(Actor parent, AIState state)
    {
        var actorBox = parent.Physics.NextAABB;
        var map = parent.Map;
        var regions = map.Regions;
        var sourceCell = parent.Cell;
        var corners = actorBox.GetCorners();
        var dir = parent.Velocity;
        var leadingCorners = corners.Where(c => c.Z == sourceCell.Z)
            .Where(c =>
            (dir.X > 0 && c.X == actorBox.Max.X) ||
            (dir.X < 0 && c.X == actorBox.Min.X) ||
            (dir.Y > 0 && c.Y == actorBox.Max.Y) ||
            (dir.Y < 0 && c.Y == actorBox.Min.Y)
        );
        foreach (var corner in leadingCorners)// corners.Where(c => c.Z == sourceCell.Z))
        {
            var cornercell = corner.ToCell();
            if (cornercell == sourceCell)
                continue;
            var cornercellBelow = cornercell.Below;
            var nextNode = regions.GetNodeAt(cornercellBelow) ?? regions.GetNodeAt(cornercell) ?? regions.GetNodeAt(cornercellBelow.Below);
            if (nextNode is null)
            {
                parent.StopPathing();
                return BehaviorState.Success;
            }
        }
        return BehaviorState.Success;
    }
}
