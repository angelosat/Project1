using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Consumables;
using Project1.Core.World.Actions;

namespace Project1.Core.World.Combat;

internal sealed class WorldActionWorker_Consume : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        var typed = (WorldActionData_Consume)data;
        var actor = data.Actor;
        var consumable = data.World.Resolve(typed.ItemId);
        if (!actor.Inventory.Contains(consumable))
            return;
        ConsumableSystem.Activate(consumable, actor);
    }
}

internal sealed record WorldActionData_Consume(Actor Actor) : WorldActionData(Actor)
{
    internal required EntityRefId ItemId;
}
