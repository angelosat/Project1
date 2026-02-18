using Project1.Framework.Events;
using System.Collections.Generic;

namespace Project1.Core.Input.Orders
{
    internal record struct PlayerIssuedOrderCommandEvent(OrderCommandDef Def, SelectionIntent Selection) : IEventPayload { }
}