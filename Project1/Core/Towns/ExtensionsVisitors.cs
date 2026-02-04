using Project1.Core.World.WorldAreas;
using Project1.Framework.Entities.Actors;

namespace Project1.Core.Towns
{
    static class ExtensionsVisitors
    {
        internal static WorldInhabitantView GetVisitorProperties(this Actor actor)
        {
            return actor.Net.Map.World.Population.GetVisitorProperties(actor);
        }
        internal static void VisitOffsiteArea(this Actor actor, FrontierDef area)
        {
            actor.GetVisitorProperties().OffsiteArea = area;
        }
    }
}
