using Project1.Framework.Events;
using System;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops
{
    internal record struct PlayerCreateShopEvent(MapId MapId) : IEventPayload;
}
