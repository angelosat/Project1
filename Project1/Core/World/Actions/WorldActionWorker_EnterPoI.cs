namespace Project1.Core.World.Actions;

internal class WorldActionWorker_EnterPoI : WorldActionWorker
{
    static WorldActionData_PoI Typed(WorldActionData data) => (WorldActionData_PoI)data;
    internal override void Resolve(WorldActionData data)
    {
        var typed = (WorldActionData_PoI)data;
        var id = typed.PoIId;
        var poi = data.Frontier.PoIs[id];
        //var poi = typed.PoI;
        poi.Add(data.Actor);
    }

    internal override string GetStatusText(WorldActionRuntime runtime)
        => $"Enterint {Typed(runtime.Data).PoI.Def.LabelReadable}";
}
