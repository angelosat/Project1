using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Start_a_Town_
{
    public abstract class EventPayloadBase
    {
    }

    //public abstract class WorldEventPayload : EventPayloadBase { }
    //public abstract class NetEventPayload : EventPayloadBase { }
    //public abstract class EntityEventPayload : EventPayloadBase { }

    //class EventRouter
    //{
    //    EventBus worldBus = new(), netBus = new();
    //    Dictionary<int, EventBus> entityBuses = [];
    //    NetEndpoint net;

    //    private readonly Dictionary<Type, EventBus> typeToBusMap = new();
    //    public EventRouter()
    //    {
    //        // map base types to buses
    //        typeToBusMap[typeof(NetEventPayload)] = netBus;
    //        typeToBusMap[typeof(WorldEventPayload)] = worldBus;
    //    }

    //    public EventBus Entity(int entityId)
    //    {
    //        if (!entityBuses.TryGetValue(entityId, out var bus))
    //            entityBuses[entityId] = bus = new EventBus();
    //        return bus;
    //    }

    //    EventBus ResolveBus<TPayload>() where TPayload : EventPayloadBase
    //    {
    //        var type = typeof(TPayload);

    //        if (typeof(EntityEventPayload).IsAssignableFrom(type))
    //            throw new Exception("Use entity bus accessor");

    //        // Walk up base classes until we find a mapping
    //        var current = type;
    //        while (current != null && current != typeof(object))
    //        {
    //            if (typeToBusMap.TryGetValue(current, out var bus))
    //                return bus;
    //            current = current.BaseType;
    //        }
    //        throw new Exception($"No bus mapping for {type.Name}");
    //    }

    //    public Action ListenTo<TPayload>(Action<TPayload> handler) where TPayload : EventPayloadBase
    //    {
    //        var bus = this.ResolveBus<TPayload>();
    //        return bus.ListenTo<TPayload>(handler);
    //    }
    //}
}
