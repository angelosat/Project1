using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Project1.Core.Events;

[EnsureStaticCtorCall]
internal static class EventReplicationSystem
{
    static EventReplicationSystem()
    {
        var registerMethod = typeof(EventReplicationSystem)
            .GetMethod(nameof(Register), BindingFlags.NonPublic | BindingFlags.Static)!;

        foreach (var eventType in DiscoverReplicatedEvents())
            registerMethod.MakeGenericMethod(eventType).Invoke(null, null);
    }

    private static void Register<T>()
        where T : IWorldEvent, ISerializableNewNew<T>
    {
        var packetId = Registry.PacketHandlers.Register(Receive<T>);

        Registry.WorldEventHooksServer.Register<T>(
            e => e.Write(Server.Instance.BeginPacket(packetId)));
    }

    private static void Receive<T>(NetEndpoint endpoint, Packet packet)
        where T : IWorldEvent, ISerializableNewNew<T>
    {
        var world = (StaticWorld)endpoint.World;
        var e = T.Create(packet.PacketReader);
        e.World = world;
        world.Events.Post(e);
    }

    private static IEnumerable<Type> DiscoverReplicatedEvents()
    {
        return typeof(EventReplicationSystem).Assembly
            .GetTypes()
            .Where(t => t.IsDefined(typeof(NetworkReplicateAttribute), false));
    }
}