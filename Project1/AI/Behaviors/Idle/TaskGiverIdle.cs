using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_
{
    class TaskGiverIdle : TaskGiver
    {
        const float MaxRange = 2;

        protected override AITask TryAssignTask(Actor actor)
        {
            int BaseWaitTime = 5;
            var composure = actor[TraitDefOf.Composure].Normalized;
            var waitTicks = (int)((BaseWaitTime + (.5f * BaseWaitTime * composure)) * Ticks.PerSecond);
            var dir = ChooseDirection(actor);
            return new AITask(TaskDefOf.Idle, new TargetArgs(dir)) { TicksTimeout = waitTicks };
        }

        static Vector2 ChooseDirection(Actor actor)
        {
            var rand = actor.Map.Random;
            var state = actor.State;
            double radians = rand.NextDouble() * 2 * Math.PI;
            var choice = new Vector3((float)Math.Cos(radians), (float)Math.Sin(radians), 0);
            var dist = Math.Min(Vector3.Distance(actor.Global, state.Leash) / (float)MaxRange, 1);
            var towardsLeash = state.Leash - actor.Global;
            towardsLeash.Z = 0;
            if (towardsLeash != Vector3.Zero)
                towardsLeash.Normalize();
            var dir = choice + dist * (towardsLeash - choice);
            dir.Normalize();
            return new(dir.X, dir.Y);
        }
    }
}
