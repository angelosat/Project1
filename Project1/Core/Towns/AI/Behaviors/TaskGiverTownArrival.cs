using Microsoft.Xna.Framework;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Framework;
using Project1.Framework.Helpers;

namespace Project1.Core.Towns.AI.Behaviors
{
    class TaskGiverTownArrival : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var props = actor.Map.World.Population.GetVisitorProperties(actor);
            if (!props.HangAroundSpot.HasValue)
                props.HangAroundSpot = this.FindHangSpot(actor);
            var spot = props.HangAroundSpot.Value;
            var distance = Vector3.Distance(actor.Global, spot);
            if (distance < 10)
                return null;

            var task = new Plan(PlanDefOf.Moving, spot.At(actor.Map)) { Urgent = false };

            return task;
        }

        Vector3 FindHangSpot(Actor actor)
        {
            var town = actor.Town;
            var citizens = town.GetMembers().Shuffle(town.Map.Random);
            foreach(var citizen in citizens)
            {
                foreach (var spot in citizen.Global.ToCell().GetRadial(3))
                {
                    if(actor.Map.Contains(spot) && actor.CanStandIn(spot) && actor.CanReach(spot))
                    {
                        return spot;
                    }
                }
            }
            return actor.Global;
        }
    }
}
