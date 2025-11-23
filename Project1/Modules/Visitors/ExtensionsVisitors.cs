namespace Start_a_Town_
{
    static class ExtensionsVisitors
    {
        internal static VisitorProperties GetVisitorProperties(this Actor actor)
        {
            return actor.Net.Map.World.Population.GetVisitorProperties(actor);
        }
        internal static void VisitOffsiteArea(this Actor actor, OffsiteAreaDef area)
        {
            actor.GetVisitorProperties().OffsiteArea = area;
        }
    }
}
