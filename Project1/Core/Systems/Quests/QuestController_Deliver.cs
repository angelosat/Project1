using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Inventory;

namespace Project1.Core.Systems.Quests;

sealed class QuestController_Deliver : QuestController
{
    protected override void OnRegister()
    {
        this.Comp.Map.World.Events.ListenTo<InventoryItemAddedEvent>(HandleInventoryItemAdded);
        this.Comp.Map.World.Events.ListenTo<InventoryItemMergedEvent>(HandleInventoryItemMerged);
    }

    private void HandleInventoryItemMerged(InventoryItemMergedEvent e)
    {
        var actor = e.Actor;
        var item = e.Existing;
        var amount = e.MergeAmount;
        this.TryIncrementProgress(actor, item, amount);
    }

    private void HandleInventoryItemAdded(InventoryItemAddedEvent e)
    {
        var actor = e.Actor;
        var item = e.Item;
        var amount = item.StackSize;
        this.TryIncrementProgress(actor, item, amount);
    }

    private void TryIncrementProgress(Actor actor, Entity item, int amount)
    {
        var quests = this.Comp.GetAcceptedQuestsByActor<QuestRuntime_Deliver>(actor);

        foreach (var q in quests)
        {
            if (q.Matches(item))
                this.Comp.IncrementProgress(actor, q, amount);
        }
    }
}
