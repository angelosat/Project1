using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class TaskGiverTownArrival : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var props = actor.Map.World.Population.GetVisitorProperties(actor);
            if (!props.HangAroundSpot.HasValue)
                props.HangAroundSpot = this.FindHangSpot(actor);
            var spot = props.HangAroundSpot.Value;
            var distance = Vector3.Distance(actor.Global, spot);
            if (distance < 10)
                return null;

            var task = new AITask(TaskDefOf.Moving, spot.At(actor.Map)) { Urgent = false };

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
