using Project1.Core.Entities;
using Project1.Framework.Events;
using System;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops
{
    internal record struct ItemToggledForSaleEvent(Entity Item, bool ForSale) : IEventPayload;
}
