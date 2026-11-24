using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Consumables;
using Project1.Core.World.Actions;
using System;

namespace Project1.Core.World.Combat;

internal sealed class WorldActionWorker_Consume : WorldActionWorker
{
    sealed record Data(Actor Actor) : WorldActionData(Actor)
    {
        internal required EntityRefId ItemId;
    }

    internal override Type DataType => typeof(Data);

    public static (WorldActionDef, WorldActionData) Create(Actor actor, EntityRefId itemId)
        => (WorldActionDefOf.Consume, new Data(actor) { ItemId = itemId });

    internal override void Resolve(WorldActionData data)
    {
        var typed = (Data)data;
        var actor = data.Actor;
        var consumable = data.World.Resolve(typed.ItemId);
        if (!actor.Inventory.Contains(consumable))
            return;
        ConsumableSystem.Activate(consumable, actor);
    }
}
