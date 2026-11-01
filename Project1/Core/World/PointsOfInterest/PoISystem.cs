using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.Frontiers;
using Project1.Framework.Helpers;
using System.Collections.Generic;

namespace Project1.Core.World.PointsOfInterest;

internal static class PoISystem
{
    public static PoIRuntime? GetPoI(Actor actor)
    {
        //internal PoIRuntime PoI => field ??= ((StaticWorld)this.Actor.World).Space.GetPoI(this.Actor);
        var frontier = ((StaticWorld)actor.World).Space.GetFrontier(actor);
        return frontier.FindPoI(actor);
    }

    public static (PoIRuntime poi, int id) SelectPoI(FrontierRuntime frontier, Actor actor)
    {
        var poi = frontier.PoIs.SelectRandom(actor.World.Random);
        var id = frontier.PoIs.IndexOf(poi);
        return (poi, id);
    }

    //public static IEnumerable<int> RollTimeline(PoIRuntime poi)
    //{
    //    for (var i = 0; i < poi.SecretsUndiscoveredCount; i++)
    //        yield return poi.Frontier.World.Random.Next(100);
    //}
}
