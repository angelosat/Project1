using Project1.Core.Systems.Biology;
using Project1.Core.Systems.Inventory;

namespace Project1.Core.Systems.Thoughts;

internal sealed class Thought_Incapacitated : ThoughtSource<ActorIncapacitatedEvent>
{
    internal override void Handle(ActorIncapacitatedEvent e)
    {
        e.Actor.AI.State.Log.Write($"I was incapacitated");
    }
}

internal sealed class Thought_StoredInInventory : ThoughtSource<InventoryItemAddedEvent>
{
    internal override void Handle(InventoryItemAddedEvent e)
    {
        e.Actor.AI.State.Log.Write($"Stored {e.Item} in inventory");
    }
}
