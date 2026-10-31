using Project1.Core.Entities.Actors;
using Project1.Core.World.PointsOfInterest;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.World.Actions;

internal sealed class WorldActionData_PoI(FrontierRuntime frontier, Actor actor) : WorldActionData(frontier, actor)
{
    internal required PoIRuntime PoI;
}

internal class WorldActionWorker_EnterPoI : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        var typed = (WorldActionData_PoI)data;
        var poi = typed.PoI;
        poi.Add(data.Actor);
    }
}
