using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Consumables;
using Project1.Core.World.Actions;
using System;

namespace Project1.Core.World.Combat;

internal sealed record WorldActionData_Consume(Actor Actor) : WorldActionData(Actor)
{
    internal override WorldActionDef Def => WorldActionDefOf.Consume;

    internal required EntityRefId ItemId;
}

internal sealed class WorldActionWorker_Consume : WorldActionWorker
{
    internal override Type DataType => typeof(WorldActionData_Consume);

    public static (WorldActionDef, WorldActionData) Create(Actor actor, EntityRefId itemId)
        => (WorldActionDefOf.Consume, new WorldActionData_Consume(actor) { ItemId = itemId });

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
