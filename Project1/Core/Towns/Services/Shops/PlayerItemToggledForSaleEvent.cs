using Project1.Core.Entities;
using Project1.Framework.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops
{
    internal record struct PlayerItemToggledForSaleEvent(IReadOnlyCollection<Entity> Items) : IEventPayload;
}
