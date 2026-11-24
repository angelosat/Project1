using Project1.Core.Entities.Actors;
using Project1.Core.World.Frontiers;
using System;

namespace Project1.Core.World.Actions;

internal class WorldActionWorker_Exploration : WorldActionWorker
{
    internal override Type DataType => typeof(WorldActionData);

    internal static (WorldActionDef action, WorldActionData data) Create(Actor actor)
        => (WorldActionDefOf.Explore, new(actor));

    internal override void Resolve(WorldActionData data)
    {
        //WorldRoomSystem.Explore(data.Frontier, data.Actor);
        //(FrontierManager)data.
        ((FrontierManager)data.Frontier.World.Space).Explore(data.Frontier, data.Actor);
    }
}
