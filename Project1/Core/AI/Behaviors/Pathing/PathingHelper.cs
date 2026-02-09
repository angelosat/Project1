using Project1.Core.Entities;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework;

namespace Project1.Core.AI.Behaviors.Pathing
{
    public static class PathingHelper
    {
        public static IEnumerable<Entity> SortByReachableRegionDistance(this IEnumerable<Entity> targets, Actor actor)
        {
            return from t in targets
                   let dist = actor.Map.GetRegionDistance(actor.GetCellStandingOn(), t.Global.ToCell(), actor)
                   where dist != -1
                   orderby dist
                   select t;
        }
        public static IEnumerable<GameObject> OrderByReachableRegionDistance(this IEnumerable<GameObject> targets, Actor actor)
        {
            return from t in targets
                   let dist = actor.Map.GetRegionDistance(actor.GetCellStandingOn(), t.Global.ToCell(), actor)
                   where dist != -1
                   orderby dist
                   select t;
        }
        public static IEnumerable<TargetArgs> OrderByReachableRegionDistance(this IEnumerable<TargetArgs> targets, Actor actor)
        {
            return from t in targets
                   let dist = actor.Map.GetRegionDistance(actor.GetCellStandingOn(), t.Global.ToCell(), actor)
                   where dist != -1
                   orderby dist
                   select t;
        }
        public static IEnumerable<IntVec3> OrderByReachableRegionDistance(this IEnumerable<IntVec3> positions, Actor actor)
        {
            return from pos in positions
                   let dist = actor.Map.GetRegionDistance(actor.GetCellStandingOn(), pos, actor)
                   where dist != -1
                   orderby dist
                   select pos;
        }
    }
}
