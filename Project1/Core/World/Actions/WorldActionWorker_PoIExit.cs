using Project1.Core.Simulation;
using Project1.Core.World.Frontiers;
using System;

namespace Project1.Core.World.Actions;

internal class WorldActionWorker_PoIExit : WorldActionWorker
{
    internal override Type DataType => typeof(WorldActionData);

    internal override void Resolve(WorldActionData data)
    {
        ((FrontierManager)data.Frontier.World.Space).ActorPoIExit(data.Actor);
    }

    internal override string GetStatusText(WorldActionRuntime runtime)
        => $"Exiting {((StaticWorld)runtime.Data.Actor.World).Frontiers.GetPoi(runtime.Data.Actor).Def.LabelReadable}";
}
