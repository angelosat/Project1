using System;
using System.Linq;

namespace Start_a_Town_
{
    public abstract class PathEndMode
    {
        static public readonly PathEndMode Touching = new FinishModeDefault();
        static public readonly PathEndMode Exact = new FinishModeOnGoal();
        static public readonly PathEndMode Any = new FinishModeOnGoalOrTouching();
        static public readonly PathEndMode InteractionSpot = new FinishModeInteractionSpot();

        public abstract bool IsFinish(Actor actor, IntVec3 goal, IntVec3 current);

        class FinishModeDefault : PathEndMode
        {
            public override bool IsFinish(Actor actor, IntVec3 goal, IntVec3 current)
            {
                if ((goal.Z < current.Z - 1) || (goal.Z > current.Z + actor.Physics.Reach))
                    return false;
                var xdist = Math.Abs(goal.X - current.X);
                var ydist = Math.Abs(goal.Y - current.Y);
                if ((xdist == 1 && ydist == 0) || (xdist == 0 && ydist == 1))
                    return true;
                return false;
            }
        }
        class FinishModeOnGoal : PathEndMode
        {
            public override bool IsFinish(Actor actor, IntVec3 g, IntVec3 c)
            {
                return g == c;
            }
        }
        class FinishModeInteractionSpot : PathEndMode
        {
            public override bool IsFinish(Actor actor, IntVec3 g, IntVec3 c)
            {
                return Cell.GetFreeInteractionSpots(actor.Map, g, actor).Contains(c);
            }
        }
        class FinishModeOnGoalOrTouching : PathEndMode
        {
            public override bool IsFinish(Actor actor, IntVec3 g, IntVec3 c)
            {
                if (g == c)
                    return true;
                if ((g.Z < c.Z - 1) || (g.Z > c.Z + actor.Physics.Reach))
                    return false;
                var xdist = Math.Abs(g.X - c.X);
                var ydist = Math.Abs(g.Y - c.Y);
                if ((xdist == 1 && ydist == 0) || (xdist == 0 && ydist == 1))
                    return true;
                return false;
            }
        }
    }
}
