using Project1.Framework.Events;

namespace Project1.Core.Input.Orders
{
    internal record struct PlayerIssuedOrderCommandEvent(OrderCommandDef Def, SelectionIntent Selection) : IEventPayload { }
}