using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Screens;
using Project1.Core.UI;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal sealed class OrderCommandCancelUnfinished : CommandWorker
    {
        internal override bool CanIssue(ISelectable target)
            => target is Entity item && item.HasComponent<UnfinishedItemComp>();
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
        {
            if (selection.Targets.Count == 1 && selection.Targets.First() is Entity item)
                Ingame.Instance.Events.Post(new PlayerCancellingUnfinishedItemEvent(item));
        }
    }
}
