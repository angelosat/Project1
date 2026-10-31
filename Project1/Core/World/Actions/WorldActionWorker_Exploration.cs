using Project1.Core.World.Frontiers;

namespace Project1.Core.World.Actions;

internal class WorldActionWorker_Exploration : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        //WorldRoomSystem.Explore(data.Frontier, data.Actor);
        //(FrontierManager)data.
        ((FrontierManager)data.Frontier.World.Space).Explore(data.Frontier, data.Actor);
    }
}
