using System;
using Microsoft.Xna.Framework;
using Project1.Framework.Base;

namespace Start_a_Town_
{
    class TaskGiverWander : Planner
    {
        const float MaxRange = 2;

        protected override Plan TryPlan(Actor actor)
        {
            var direction = new TargetArgs(ChooseDirection(actor));
            return new Plan(PlanDefOf.Wander, direction) { TicksTimeout = Ticks.PerSecond };
        }

        static Vector2 ChooseDirection(Actor actor)
        {
            var rand = actor.Map.Random;
            var state = actor.AI.State;
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
