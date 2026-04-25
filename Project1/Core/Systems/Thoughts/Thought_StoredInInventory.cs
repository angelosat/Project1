using Project1.Core.Systems.Inventory;
using Project1.Framework.Events;

namespace Project1.Core.Systems.Thoughts
{
    internal sealed class Thought_StoredInInventory : ThoughtSource<InventoryItemAddedEvent>
    {
        internal override void Handle(InventoryItemAddedEvent e)
        {
            e.Actor.AI.State.Log.Write($"Stored {e.Item} in inventory");
        }
    }
}
