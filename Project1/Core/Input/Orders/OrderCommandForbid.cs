using Project1.Core.Entities;
using Project1.Core.Screens;
using Project1.Core.UI;

namespace Project1.Core.Input.Orders
{
    internal sealed class OrderCommandForbid : OrderCommandWorker
    {
        internal override bool CanIssue(ISelectable target) => target is Entity entity && entity.IsForbiddable();
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
            => Ingame.Instance.Events.Post(new PlayerForbiddingItemsEvent(selection.Entities));
    }
}
