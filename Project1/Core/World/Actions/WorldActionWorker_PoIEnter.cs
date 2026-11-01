using Project1.Core.World.Frontiers;

namespace Project1.Core.World.Actions;

internal class WorldActionWorker_PoIEnter : WorldActionWorker
{
    static WorldActionData_PoI Typed(WorldActionData data) => (WorldActionData_PoI)data;
    internal override void Resolve(WorldActionData data)
    {
        var typed = (WorldActionData_PoI)data;
        var id = typed.PoIId;
        ((FrontierManager)data.Frontier.World.Space).ActorPoIEnter(data.Actor, data.Frontier, id);
        //var poi = data.Frontier.PoIs[id];
        ////var poi = typed.PoI;
        //poi.Add(data.Actor);
    }

    internal override string GetStatusText(WorldActionRuntime runtime)
        => $"Entering {Typed(runtime.Data).PoI.Def.LabelReadable}";
}
