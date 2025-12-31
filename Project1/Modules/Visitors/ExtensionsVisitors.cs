namespace Start_a_Town_
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
