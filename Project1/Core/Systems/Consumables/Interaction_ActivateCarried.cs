using Project1.Core.Interactions;

namespace Project1.Core.Systems.Consumables;

internal class Interaction_ActivateCarried : InteractionLogic
{
    internal override void OnFinish(Interaction i)
    {
        var carried = i.Actor.Hauled;
        //carried.Consume(1);
        //var comp = carried.Consumable;
        //comp.ApplyEffects(i.Actor);
        ConsumableSystem.Activate(carried, i.Actor);
    }
}
